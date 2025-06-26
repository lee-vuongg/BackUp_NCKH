using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using TCN_NCKH.Models.DBModel;
using System.Security.Claims;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;
using SixLabors.ImageSharp.Formats.Png;

namespace TCN_NCKH.Areas.HocSinh.Controllers
{
    [Area("HocSinh")]
    public class NguoidungsController : Controller
    {
        private readonly NghienCuuKhoaHocContext _context;
        private object userId;

        public NguoidungsController(NghienCuuKhoaHocContext context)
        {
            _context = context;
        }

        // GET: HocSinh/Nguoidungs
        public async Task<IActionResult> Index()
        {
            var nghienCuuKhoaHocContext = _context.Nguoidungs.Include(n => n.Loainguoidung);
            return View(await nghienCuuKhoaHocContext.ToListAsync());
        }

        // GET: HocSinh/Nguoidungs/Details/5
        public async Task<IActionResult> Details(string id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var nguoidung = await _context.Nguoidungs
                .Include(n => n.Loainguoidung)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (nguoidung == null)
            {
                return NotFound();
            }

            return View(nguoidung);
        }

        // GET: HocSinh/Nguoidungs/Create
        public IActionResult Create()
        {
            ViewData["Loainguoidungid"] = new SelectList(_context.Loainguoidungs, "Id", "Id");
            return View();
        }

        // POST: HocSinh/Nguoidungs/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Hoten,Email,Matkhau,Sdt,Ngaysinh,Gioitinh,Trangthai,Nguoitao,Ngaytao,Loainguoidungid")] Nguoidung nguoidung)
        {
            if (ModelState.IsValid)
            {
                _context.Add(nguoidung);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["Loainguoidungid"] = new SelectList(_context.Loainguoidungs, "Id", "Id", nguoidung.Loainguoidungid);
            return View(nguoidung);
        }

        // GET: HocSinh/Nguoidungs/Edit/5
        public async Task<IActionResult> Edit(string id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var nguoidung = await _context.Nguoidungs.FindAsync(id);
            if (nguoidung == null)
            {
                return NotFound();
            }
            ViewData["Loainguoidungid"] = new SelectList(_context.Loainguoidungs, "Id", "Id", nguoidung.Loainguoidungid);
            return View(nguoidung);
        }

        // POST: HocSinh/Nguoidungs/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(string id, [Bind("Id,Hoten,Email,Matkhau,Sdt,Ngaysinh,Gioitinh,Trangthai,Nguoitao,Ngaytao,Loainguoidungid")] Nguoidung nguoidung)
        {
            if (id != nguoidung.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(nguoidung);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!NguoidungExists(nguoidung.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(AccountDetails));
            }
            ViewData["Loainguoidungid"] = new SelectList(_context.Loainguoidungs, "Id", "Id", nguoidung.Loainguoidungid);
            return View(nguoidung);
        }

        // GET: HocSinh/Nguoidungs/Delete/5
        public async Task<IActionResult> Delete(string id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var nguoidung = await _context.Nguoidungs
                .Include(n => n.Loainguoidung)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (nguoidung == null)
            {
                return NotFound();
            }

            return View(nguoidung);
        }

        // POST: HocSinh/Nguoidungs/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(string id)
        {
            var nguoidung = await _context.Nguoidungs.FindAsync(id);
            if (nguoidung != null)
            {
                _context.Nguoidungs.Remove(nguoidung);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool NguoidungExists(string id)
        {
            return _context.Nguoidungs.Any(e => e.Id == id);
        }


        // ======== PHẦN MỚI DÀNH CHO SINH VIÊN XEM TÀI KHOẢN ========

        // GET: HocSinh/Nguoidungs/AccountDetails
        public async Task<IActionResult> AccountDetails()
        {
            if (!User.Identity.IsAuthenticated || !User.IsInRole("Sinh viên"))
            {
                return RedirectToAction("Login", "Auth");
            }

            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
            {
                return RedirectToAction("Login", "Auth");
            }

            var nguoidung = await _context.Nguoidungs
                .Include(n => n.Loainguoidung)
                .FirstOrDefaultAsync(u => u.Id == userId);

            if (nguoidung == null)
            {
                return NotFound("Không tìm thấy thông tin tài khoản.");
            }

            return View(nguoidung);
        }
        // GET: HocSinh/Nguoidungs/UpdateAvatar
        [HttpGet]
        public IActionResult UpdateAvatar(string id)
        {
            var nguoidung = _context.Nguoidungs.FirstOrDefault(x => x.Id == id);
            if (nguoidung == null) return NotFound();
            return View(nguoidung);
        }

        // POST: HocSinh/Nguoidungs/UpdateAvatar
        [HttpPost]
        public async Task<IActionResult> UpdateAvatar(IFormFile avatarFile)
        {
            const long MaxFileSize = 2 * 1024 * 1024; // 2MB
            var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/images");

            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
            {
                TempData["Error"] = "Không xác định được người dùng.";
                return RedirectToAction("AccountDetails");
            }

            if (avatarFile == null || avatarFile.Length == 0)
            {
                TempData["Error"] = "Vui lòng chọn ảnh.";
                return RedirectToAction("AccountDetails");
            }

            if (avatarFile.Length > MaxFileSize)
            {
                TempData["Error"] = "Ảnh vượt quá kích thước cho phép (tối đa 2MB).";
                return RedirectToAction("AccountDetails");
            }

            var nguoidung = await _context.Nguoidungs.FirstOrDefaultAsync(x => x.Id == userId);
            if (nguoidung == null)
            {
                TempData["Error"] = "Không tìm thấy người dùng.";
                return RedirectToAction("AccountDetails");
            }

            var fileExt = Path.GetExtension(avatarFile.FileName).ToLower();
            var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif" };
            if (!allowedExtensions.Contains(fileExt))
            {
                TempData["Error"] = "Định dạng ảnh không hợp lệ.";
                return RedirectToAction("AccountDetails");
            }

            var newFileName = Guid.NewGuid().ToString() + fileExt;
            var newFilePath = Path.Combine(uploadsFolder, newFileName);

            try
            {
                using (var image = await Image.LoadAsync(avatarFile.OpenReadStream()))
                {
                    image.Mutate(x => x.Resize(new ResizeOptions
                    {
                        Mode = ResizeMode.Crop,
                        Size = new Size(300, 300)
                    }));

                    await image.SaveAsync(newFilePath);
                }

                if (!string.IsNullOrEmpty(nguoidung.Anhdaidien))
                {
                    var oldPath = Path.Combine(uploadsFolder, nguoidung.Anhdaidien);
                    if (System.IO.File.Exists(oldPath) && !nguoidung.Anhdaidien.Equals("default-avatar.png"))
                    {
                        System.IO.File.Delete(oldPath);
                    }
                }

                nguoidung.Anhdaidien = newFileName;
                _context.Update(nguoidung);
                await _context.SaveChangesAsync();

                TempData["Success"] = "Cập nhật ảnh thành công.";
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Lỗi xử lý ảnh: " + ex.Message;
            }

            return RedirectToAction("AccountDetails");
        }

        // ======== PHẦN MỚI CHO SINH VIÊN ĐỔI MẬT KHẨU ========

        // GET: HocSinh/Nguoidungs/ChangePassword
        public IActionResult ChangePassword()
        {
            if (!User.Identity.IsAuthenticated || !User.IsInRole("Sinh viên"))
            {
                return RedirectToAction("Login", "Auth");
            }

            return View();
        }

        // POST: HocSinh/Nguoidungs/ChangePassword
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ChangePassword(string currentPassword, string newPassword, string confirmNewPassword)
        {
            if (!User.Identity.IsAuthenticated || !User.IsInRole("Sinh viên"))
            {
                return RedirectToAction("Login", "Auth");
            }

            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
            {
                return RedirectToAction("Login", "Auth");
            }

            if (string.IsNullOrEmpty(currentPassword) || string.IsNullOrEmpty(newPassword) || string.IsNullOrEmpty(confirmNewPassword))
            {
                ModelState.AddModelError("", "Vui lòng nhập đầy đủ thông tin.");
                return View();
            }

            if (newPassword != confirmNewPassword)
            {
                ModelState.AddModelError("", "Mật khẩu mới và xác nhận mật khẩu không khớp.");
                return View();
            }

            var nguoidung = await _context.Nguoidungs.FindAsync(userId);
            if (nguoidung == null)
            {
                return NotFound("Không tìm thấy tài khoản.");
            }

            // Kiểm tra mật khẩu hiện tại (giả sử mật khẩu lưu plaintext, nếu mã hóa thì phải kiểm tra khác)
            if (nguoidung.Matkhau != currentPassword)
            {
                ModelState.AddModelError("", "Mật khẩu hiện tại không đúng.");
                return View();
            }

            // Cập nhật mật khẩu mới
            nguoidung.Matkhau = newPassword;

            try
            {
                _context.Update(nguoidung);
                await _context.SaveChangesAsync();
                ViewData["SuccessMessage"] = "Đổi mật khẩu thành công!";
            }
            catch (Exception)
            {
                ModelState.AddModelError("", "Có lỗi xảy ra khi lưu thay đổi.");
                return View();
            }

            return View();
        }
    }
}
