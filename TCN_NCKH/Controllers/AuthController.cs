using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using System.ComponentModel.DataAnnotations;
using System.Security.Claims;
using TCN_NCKH.Helpers;
using TCN_NCKH.Models.DBModel;

namespace TCN_NCKH.Controllers
{
    public class AuthController : Controller
    {
        private readonly NghienCuuKhoaHocContext _context;

        public AuthController(NghienCuuKhoaHocContext context)
        {
            _context = context;
            Console.WriteLine("[Constructor] AuthController khởi tạo thành công.");
        }

        [HttpGet]
        public async Task<IActionResult> Login(bool forceLogout = false)
        {
            if (forceLogout || User.Identity?.IsAuthenticated == true)
            {
                await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
                foreach (var cookie in Request.Cookies.Keys)
                {
                    Response.Cookies.Delete(cookie);
                }
                ViewBag.Info = "Bạn đã đăng xuất. Mời đăng nhập lại.";
            }
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Login(LoginModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var user = await _context.Nguoidungs
                .Include(u => u.Sinhvien)
                .FirstOrDefaultAsync(u => u.Email == model.Email && u.Trangthai == true);

            // TODO: Thay thế kiểm tra mật khẩu thẳng bằng mã hóa bcrypt hoặc tương tự
            if (user == null || user.Matkhau != model.Password)
            {
                ModelState.AddModelError("", "Sai tài khoản hoặc mật khẩu!");
                return View(model);
            }

            var role = await _context.Loainguoidungs
                .Where(l => l.Id == user.Loainguoidungid)
                .Select(l => l.Tenloai)
                .FirstOrDefaultAsync();

            if (string.IsNullOrEmpty(role))
            {
                ModelState.AddModelError("", "Không xác định được vai trò người dùng!");
                return View(model);
            }

            var msv = user.Sinhvien?.Msv ?? "Chưa có MSV";

            var claims = new List<Claim>
{
                 new Claim("MaSinhVien", msv), // 👈 THÊM NÀY!
                 new Claim(ClaimTypes.Name, msv),
                 new Claim(ClaimTypes.Email, user.Email),
                 new Claim(ClaimTypes.Role, role),
                 new Claim(ClaimTypes.NameIdentifier, user.Id.ToString())
};


            var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            var principal = new ClaimsPrincipal(identity);

            await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal);

            return role switch
            {
                "Admin" => RedirectToAction("Index", "AdminHome", new { area = "Admin" }),
                "Giáo viên" => RedirectToAction("Index", "GiaoVienHome", new { area = "GiaoVien" }),
                "Sinh viên" => RedirectToAction("Index", "HocSinhHome", new { area = "HocSinh" }),
                _ => RedirectToAction("Index", "Home")
            };
        }


        [HttpGet]
        public IActionResult Register()
        {

            Console.WriteLine("[Register-GET] Truy cập form đăng ký.");
           

            return View(new RegisterModel());
        }

        [HttpPost]
        public async Task<IActionResult> Register(RegisterModel model)
        {
            Console.WriteLine($"[Register-POST] Nhận dữ liệu: Email = {model.Email}, Msv = {model.Msv}");

            if (!ModelState.IsValid)
            {
                Console.WriteLine("[Register-POST] ModelState không hợp lệ.");
                return View(model);
            }

            Console.WriteLine("[Register-POST] Kiểm tra tài khoản tồn tại...");
            var existingUser = await _context.Nguoidungs
                .FirstOrDefaultAsync(u => u.Email == model.Email || u.Id == model.Msv);

            if (existingUser != null)
            {
                Console.WriteLine("[Register-POST] Email hoặc Mã sinh viên đã tồn tại.");
                ModelState.AddModelError("", "Email hoặc mã sinh viên đã tồn tại.");
                return View(model);
            }

            var verificationCode = new Random().Next(100000, 999999).ToString();
            TempData["VerificationCode"] = verificationCode;
            TempData["UserEmail"] = model.Email;
            TempData["RegisterModel"] = JsonConvert.SerializeObject(model);

            Console.WriteLine($"[Register-POST] Mã xác nhận: {verificationCode}");

            string subject = "Mã xác nhận đăng ký tài khoản";
            string body = $"<h3>Mã xác nhận của bạn là: <strong>{verificationCode}</strong></h3>";
            await EmailSender.SendEmailAsync(model.Email, subject, body);

            return RedirectToAction("~/Views/Auth/VerifyCode.cshtml"); 


        }

        [HttpGet]
        public IActionResult VerifyCode()
        {
            Console.WriteLine("[GET VerifyCode] Truy cập trang nhập mã xác nhận.");

            if (TempData["VerificationCode"] == null)
            {
                Console.WriteLine("[GET VerifyCode] Không có mã xác nhận, chuyển về Register.");
                return RedirectToAction("Register");
            }

            TempData.Keep(); // giữ lại sau GET
            return View("VerifyCode");
        }

        [HttpPost]
        public async Task<IActionResult> VerifyCode(string code)
        {
            TempData.Keep();
            Console.WriteLine($"[VerifyCode-POST] Mã người dùng nhập: {code}");

            var storedCode = TempData["VerificationCode"]?.ToString();
            var email = TempData["UserEmail"]?.ToString();
            var jsonModel = TempData["RegisterModel"]?.ToString();

            Console.WriteLine($"[VerifyCode-POST] Mã lưu: {storedCode}, Email: {email}");

            if (string.IsNullOrEmpty(storedCode) || string.IsNullOrEmpty(code))
            {
                Console.WriteLine("[VerifyCode-POST] Mã hoặc TempData rỗng.");
                ModelState.AddModelError("", "Mã xác nhận không hợp lệ.");
                return View();
            }

            if (code == storedCode && jsonModel != null)
            {
                Console.WriteLine("[VerifyCode-POST] Mã xác nhận đúng. Bắt đầu tạo tài khoản...");

                var model = JsonConvert.DeserializeObject<RegisterModel>(jsonModel);

                if (model == null)
                {
                    Console.WriteLine("[VerifyCode-POST] Deserialize model thất bại.");
                    ModelState.AddModelError("", "Dữ liệu đăng ký bị lỗi.");
                    return View();
                }

                var sinhVienRole = await _context.Loainguoidungs
                    .FirstOrDefaultAsync(r => r.Tenloai == "Sinh viên");

                var user = new Nguoidung
                {
                    Id = model.Msv,
                    Hoten = model.Hoten,
                    Email = model.Email,
                    Matkhau = model.Password,
                    Sdt = model.Sdt,
                    Ngaysinh = model.Ngaysinh,
                    Gioitinh = model.Gioitinh,
                    Trangthai = true,
                    Nguoitao = model.Msv,
                    Ngaytao = DateTime.Now,
                    Loainguoidungid = sinhVienRole?.Id ?? 1
                };

                var sinhvien = new Sinhvien
                {
                    Sinhvienid = model.Msv,
                    Msv = model.Msv,
                    Lophocid = model.Lophocid // <-- Gán lớp học
                };

                try
                {
                    _context.Nguoidungs.Add(user);
                    _context.Sinhviens.Add(sinhvien);
                    await _context.SaveChangesAsync();

                    Console.WriteLine("[VerifyCode-POST] Đăng ký thành công.");
                    TempData["SuccessToast"] = "Đăng ký thành công!";
                    return RedirectToAction("Login");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[VerifyCode-POST] Lỗi khi lưu vào DB: {ex.Message}");
                    ModelState.AddModelError("", "Đã xảy ra lỗi khi lưu dữ liệu. Vui lòng thử lại.");
                    return View();
                }
            }
            else
            {
                Console.WriteLine("[VerifyCode-POST] Mã xác nhận không khớp hoặc hết hạn.");
                ModelState.AddModelError("", "Mã xác nhận không đúng hoặc đã hết hạn.");
                return View();
            }
        }


        public async Task<IActionResult> Logout()
        {
            Console.WriteLine("[Logout] Bắt đầu đăng xuất...");
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            Console.WriteLine("[Logout] Đăng xuất thành công.");
            return RedirectToAction("Login", new { forceLogout = true });
        }
    }
    public class LoginModel
    {
        [Required(ErrorMessage = "Email không được để trống")]
        [EmailAddress(ErrorMessage = "Email không hợp lệ")]
        public string Email { get; set; } = "";

        [Required(ErrorMessage = "Mật khẩu không được để trống")]
        public string Password { get; set; } = "";
    }

    public class RegisterModel
    {
        [Required(ErrorMessage = "Mã sinh viên không được để trống")]
        [StringLength(20, ErrorMessage = "Mã sinh viên tối đa 20 ký tự")]
        public string Msv { get; set; } = "";

        [Required(ErrorMessage = "Họ tên không được để trống")]
        [StringLength(100, ErrorMessage = "Họ tên tối đa 100 ký tự")]
        public string Hoten { get; set; } = "";

        [Required(ErrorMessage = "Email không được để trống")]
        [EmailAddress(ErrorMessage = "Email không hợp lệ")]
        public string Email { get; set; } = "";

        [Required(ErrorMessage = "Mật khẩu không được để trống")]
        [DataType(DataType.Password)]
        [StringLength(100, MinimumLength = 6, ErrorMessage = "Mật khẩu ít nhất 6 ký tự")]
        public string Password { get; set; } = "";

        [Required(ErrorMessage = "Số điện thoại không được để trống")]
        [Phone(ErrorMessage = "Số điện thoại không hợp lệ")]
        public string Sdt { get; set; } = "";

        [Required(ErrorMessage = "Ngày sinh không được để trống")]
        [DataType(DataType.Date)]
        public DateOnly Ngaysinh { get; set; }

        [Required(ErrorMessage = "Giới tính không được để trống")]
        public bool Gioitinh { get; set; }
        public string? Lophocid { get; set; }
    }
}
