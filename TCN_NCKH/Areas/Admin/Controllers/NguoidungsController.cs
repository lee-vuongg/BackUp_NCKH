using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using TCN_NCKH.Models.DBModel;
using System.Security.Claims; // Cần thiết nếu bạn muốn lấy thông tin người dùng đang đăng nhập
using X.PagedList; 

namespace TCN_NCKH.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class NguoidungsController : Controller
    {
        private readonly NghienCuuKhoaHocContext _context;

        public NguoidungsController(NghienCuuKhoaHocContext context)
        {
            _context = context;
        }

        // GET: Admin/Nguoidungs
        // Khi sử dụng DataTables phía client, chúng ta chỉ cần trả về tất cả dữ liệu
        public async Task<IActionResult> Index()
        {
            // Bao gồm thông tin Loại người dùng (Loainguoidung)
            // Sắp xếp mặc định để DataTables có dữ liệu ban đầu có thứ tự
            var nguoidungs = _context.Nguoidungs
                                     .Include(n => n.Loainguoidung)
                                     .OrderBy(n => n.Hoten);

            return View(await nguoidungs.ToListAsync());
        }

        // GET: Admin/Nguoidungs/Details/5
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
                TempData["ErrorMessage"] = "Người dùng không tồn tại."; // Thông báo lỗi
                return RedirectToAction(nameof(Index)); // Chuyển hướng về trang Index nếu không tìm thấy
            }

            return View(nguoidung);
        }

        // GET: Admin/Nguoidungs/Create
        public IActionResult Create()
        {
            // Hiển thị Tenloai thay vì Id trong dropdown
            ViewData["Loainguoidungid"] = new SelectList(_context.Loainguoidungs, "Id", "Tenloai");
            return View();
        }

        // POST: Admin/Nguoidungs/Create
        // [Bind] các trường mà admin có thể nhập từ form
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Hoten,Email,Matkhau,Sdt,Ngaysinh,Gioitinh,Trangthai,Loainguoidungid")] Nguoidung nguoidung)
        {
            // Tự động gán ID mới nếu Id là string (thường dùng Guid)
            nguoidung.Id = Guid.NewGuid().ToString();
            // Tự động gán ngày tạo
            nguoidung.Ngaytao = DateTime.Now;

            // Lấy ID người tạo từ người dùng Admin hiện tại nếu bạn muốn lưu ai là người tạo tài khoản này
            // var adminUserId = User.FindFirstValue(ClaimTypes.NameIdentifier); // Hoặc ClaimTypes.NameIdentifier
            // nguoidung.Nguoitao = adminUserId; // Gán người tạo

            if (ModelState.IsValid)
            {
                _context.Add(nguoidung);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Người dùng đã được tạo thành công!"; // Thông báo thành công
                return RedirectToAction(nameof(Index));
            }
            // Nếu ModelState không hợp lệ, load lại SelectList và hiển thị lỗi
            ViewData["Loainguoidungid"] = new SelectList(_context.Loainguoidungs, "Id", "Tenloai", nguoidung.Loainguoidungid);
            TempData["ErrorMessage"] = "Có lỗi xảy ra khi tạo người dùng. Vui lòng kiểm tra lại thông tin.";
            return View(nguoidung);
        }

        // GET: Admin/Nguoidungs/Edit/5
        public async Task<IActionResult> Edit(string id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var nguoidung = await _context.Nguoidungs.FindAsync(id);
            if (nguoidung == null)
            {
                TempData["ErrorMessage"] = "Người dùng không tồn tại.";
                return RedirectToAction(nameof(Index));
            }
            ViewData["Loainguoidungid"] = new SelectList(_context.Loainguoidungs, "Id", "Tenloai", nguoidung.Loainguoidungid); // Hiển thị Tenloai
            return View(nguoidung);
        }

        // POST: Admin/Nguoidungs/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(string id, [Bind("Id,Hoten,Email,Matkhau,Sdt,Ngaysinh,Gioitinh,Trangthai,Loainguoidungid,Anhdaidien")] Nguoidung nguoidungFromForm)
        {
            if (id != nguoidungFromForm.Id)
            {
                TempData["ErrorMessage"] = "ID người dùng không hợp lệ.";
                return NotFound();
            }

            var existingNguoidung = await _context.Nguoidungs.FindAsync(id);

            if (existingNguoidung == null)
            {
                TempData["ErrorMessage"] = "Người dùng không tồn tại.";
                return NotFound();
            }

            // --- THÊM ĐOẠN CODE NÀY ĐỂ XỬ LÝ VALIDATION MẬT KHẨU ---
            // Nếu người dùng KHÔNG nhập mật khẩu mới, xóa lỗi validation cho Matkhau
            // Lý do: Form gửi Matkhau rỗng, nếu Matkhau trong Model là [Required],
            // ModelState.IsValid sẽ là false.
            if (string.IsNullOrEmpty(nguoidungFromForm.Matkhau))
            {
                // Remove the password validation errors from ModelState
                ModelState.Remove("Matkhau");
            }
            // -----------------------------------------------------

            if (!ModelState.IsValid)
            {
                string validationErrors = string.Join("<br/>", ModelState.Values
                                                      .SelectMany(v => v.Errors)
                                                      .Select(e => e.ErrorMessage));
                TempData["ErrorMessage"] = "Có lỗi xác thực: <br/>" + validationErrors;

                ViewData["Loainguoidungid"] = new SelectList(_context.Loainguoidungs, "Id", "Tenloai", existingNguoidung.Loainguoidungid);
                return View(existingNguoidung);
            }

            // Manually update ONLY the properties that are allowed to be changed from the form
            existingNguoidung.Hoten = nguoidungFromForm.Hoten;
            existingNguoidung.Email = nguoidungFromForm.Email;
            existingNguoidung.Sdt = nguoidungFromForm.Sdt;
            existingNguoidung.Ngaysinh = nguoidungFromForm.Ngaysinh;
            existingNguoidung.Gioitinh = nguoidungFromForm.Gioitinh;
            existingNguoidung.Trangthai = nguoidungFromForm.Trangthai;
            existingNguoidung.Loainguoidungid = nguoidungFromForm.Loainguoidungid;
            existingNguoidung.AnhDaiDien = nguoidungFromForm.AnhDaiDien;

            // Handle password: ONLY UPDATE if a new password has been entered in the form
            if (!string.IsNullOrEmpty(nguoidungFromForm.Matkhau))
            {
                // Hash the new password before assigning to existingNguoidung.Matkhau
                // Example: existingNguoidung.Matkhau = YourPasswordHasher.HashPassword(nguoidungFromForm.Matkhau);
                existingNguoidung.Matkhau = nguoidungFromForm.Matkhau; // Placeholder, MUST HASH
            }
            // If nguoidungFromForm.Matkhau is null or empty, existingNguoidung.Matkhau will retain its old value automatically.

            try
            {
                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = "Thông tin người dùng đã được cập nhật thành công!";
                return RedirectToAction(nameof(Index));
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!NguoidungExists(existingNguoidung.Id))
                {
                    TempData["ErrorMessage"] = "Người dùng không tồn tại.";
                    return NotFound();
                }
                else
                {
                    TempData["ErrorMessage"] = "Đã xảy ra lỗi đồng thời khi cập nhật dữ liệu. Vui lòng thử lại.";
                    throw;
                }
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Đã xảy ra lỗi hệ thống: {ex.Message}";
                ViewData["Loainguoidungid"] = new SelectList(_context.Loainguoidungs, "Id", "Tenloai", existingNguoidung.Loainguoidungid);
                return View(existingNguoidung);
            }
        }
        // GET: Admin/Nguoidungs/Delete/5
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
                TempData["ErrorMessage"] = "Người dùng không tồn tại.";
                return RedirectToAction(nameof(Index));
            }

            return View(nguoidung);
        }

        // POST: Admin/Nguoidungs/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(string id)
        {
            var nguoidung = await _context.Nguoidungs.FindAsync(id);
            if (nguoidung == null) // Đảm bảo người dùng tồn tại trước khi xóa
            {
                TempData["ErrorMessage"] = "Người dùng không tồn tại hoặc đã bị xóa.";
                return RedirectToAction(nameof(Index));
            }

            _context.Nguoidungs.Remove(nguoidung);
            await _context.SaveChangesAsync();
            TempData["SuccessMessage"] = "Người dùng đã được xóa thành công!"; // Thông báo thành công
            return RedirectToAction(nameof(Index));
        }

        private bool NguoidungExists(string id)
        {
            return _context.Nguoidungs.Any(e => e.Id == id);
        }
    }
}