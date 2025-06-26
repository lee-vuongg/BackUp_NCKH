using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using TCN_NCKH.Models.DBModel;
using System.Security.Claims; // Cần thiết nếu bạn muốn lấy thông tin người dùng đang đăng nhập

namespace TCN_NCKH.Areas.Admin.Controllers // Hoặc TCN_NCKH.Areas.GiaoVien.Controllers nếu Controller này là của GV
{
    [Area("Admin")] // Hoặc "GiaoVien"
    public class DethisController : Controller
    {
        private readonly NghienCuuKhoaHocContext _context;

        public DethisController(NghienCuuKhoaHocContext context)
        {
            _context = context;
        }

        // GET: Admin/Dethis
        public async Task<IActionResult> Index()
        {
            var dethis = _context.Dethis
                                 .Include(d => d.Monhoc)
                                 .Include(d => d.NguoitaoNavigation)
                                 .OrderByDescending(d => d.Ngaytao);
            return View(await dethis.ToListAsync());
        }

        // GET: Admin/Dethis/Details/5
        public async Task<IActionResult> Details(string id)
        {
            if (id == null)
            {
                TempData["ErrorMessage"] = "Không tìm thấy ID đề thi.";
                return NotFound();
            }

            var dethi = await _context.Dethis
                .Include(d => d.Monhoc)
                .Include(d => d.NguoitaoNavigation)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (dethi == null)
            {
                TempData["ErrorMessage"] = "Đề thi không tồn tại.";
                return RedirectToAction(nameof(Index));
            }

            return View(dethi);
        }

        // GET: Admin/Dethis/Create
        public IActionResult Create()
        {
            // Đảm bảo "Tenmon" là đúng tên thuộc tính trong Monhoc model
            ViewData["Monhocid"] = new SelectList(_context.Monhocs.OrderBy(m => m.Tenmon), "Id", "Tenmon");
            ViewData["Nguoitao"] = new SelectList(_context.Nguoidungs.OrderBy(n => n.Hoten), "Id", "Hoten");
            return View();
        }

        // POST: Admin/Dethis/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        // Đã loại bỏ "Thoigianthi" và "Trangthai" khỏi Bind
        public async Task<IActionResult> Create([Bind("Tendethi,Monhocid,Ngaytao")] Dethi dethi)
        {
            dethi.Id = Guid.NewGuid().ToString();

            // Nếu Ngaytao không được nhập từ form, gán thời gian hiện tại
            if (dethi.Ngaytao == default(DateTime) || dethi.Ngaytao == null) // Thêm kiểm tra null cho DateTime?
            {
                dethi.Ngaytao = DateTime.Now;
            }

            var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!string.IsNullOrEmpty(currentUserId))
            {
                dethi.Nguoitao = currentUserId;
            }
            else
            {
                ModelState.AddModelError("Nguoitao", "Không thể xác định người tạo đề thi.");
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Add(dethi);
                    await _context.SaveChangesAsync();
                    TempData["SuccessMessage"] = "Đề thi đã được tạo thành công!";
                    return RedirectToAction(nameof(Index));
                }
                catch (Exception ex)
                {
                    TempData["ErrorMessage"] = $"Đã xảy ra lỗi khi tạo đề thi: {ex.Message}";
                }
            }
            // Nếu ModelState không hợp lệ hoặc có lỗi khi lưu, cần khởi tạo lại SelectList
            ViewData["Monhocid"] = new SelectList(_context.Monhocs.OrderBy(m => m.Tenmon), "Id", "Tenmon", dethi.Monhocid);
            ViewData["Nguoitao"] = new SelectList(_context.Nguoidungs.OrderBy(n => n.Hoten), "Id", "Hoten", dethi.Nguoitao);

            if (TempData["ErrorMessage"] == null)
            {
                TempData["ErrorMessage"] = "Thông tin đề thi không hợp lệ. Vui lòng kiểm tra lại.";
            }
            return View(dethi);
        }

        // GET: Admin/Dethis/Edit/5
        public async Task<IActionResult> Edit(string id)
        {
            if (id == null)
            {
                TempData["ErrorMessage"] = "Không tìm thấy ID đề thi.";
                return NotFound();
            }

            var dethi = await _context.Dethis.FindAsync(id);
            if (dethi == null)
            {
                TempData["ErrorMessage"] = "Đề thi không tồn tại.";
                return RedirectToAction(nameof(Index));
            }
            ViewData["Monhocid"] = new SelectList(_context.Monhocs.OrderBy(m => m.Tenmon), "Id", "Tenmon", dethi.Monhocid);
            ViewData["Nguoitao"] = new SelectList(_context.Nguoidungs.OrderBy(n => n.Hoten), "Id", "Hoten", dethi.Nguoitao);
            return View(dethi);
        }

        // POST: Admin/Dethis/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        // Đã loại bỏ "Thoigianthi" và "Trangthai" khỏi Bind
        public async Task<IActionResult> Edit(string id, [Bind("Id,Tendethi,Monhocid,Nguoitao,Ngaytao")] Dethi dethiFromForm)
        {
            if (id != dethiFromForm.Id)
            {
                TempData["ErrorMessage"] = "ID đề thi không hợp lệ.";
                return NotFound();
            }

            var existingDethi = await _context.Dethis.FindAsync(id);

            if (existingDethi == null)
            {
                TempData["ErrorMessage"] = "Đề thi không tồn tại.";
                return NotFound();
            }

            if (!ModelState.IsValid)
            {
                string validationErrors = string.Join("<br/>", ModelState.Values
                                                      .SelectMany(v => v.Errors)
                                                      .Select(e => e.ErrorMessage));
                TempData["ErrorMessage"] = "Có lỗi xác thực: <br/>" + validationErrors;

                ViewData["Monhocid"] = new SelectList(_context.Monhocs.OrderBy(m => m.Tenmon), "Id", "Tenmon", existingDethi.Monhocid);
                ViewData["Nguoitao"] = new SelectList(_context.Nguoidungs.OrderBy(n => n.Hoten), "Id", "Hoten", existingDethi.Nguoitao);
                return View(existingDethi);
            }

            try
            {
                existingDethi.Tendethi = dethiFromForm.Tendethi;
                existingDethi.Monhocid = dethiFromForm.Monhocid;
                existingDethi.Nguoitao = dethiFromForm.Nguoitao;
                existingDethi.Ngaytao = dethiFromForm.Ngaytao; // Giữ nguyên Ngaytao nếu có trong form và bạn muốn cho phép sửa

                // Đã loại bỏ các dòng sau vì không có trong model Dethi
                // existingDethi.Thoigianthi = dethiFromForm.Thoigianthi;
                // existingDethi.Trangthai = dethiFromForm.Trangthai;

                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Đề thi đã được cập nhật thành công!";
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!DethiExists(dethiFromForm.Id))
                {
                    TempData["ErrorMessage"] = "Đề thi không tồn tại hoặc đã bị xóa bởi người khác.";
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
            }

            return RedirectToAction(nameof(Index));
        }

        // GET: Admin/Dethis/Delete/5
        public async Task<IActionResult> Delete(string id)
        {
            if (id == null)
            {
                TempData["ErrorMessage"] = "Không tìm thấy ID đề thi.";
                return NotFound();
            }

            var dethi = await _context.Dethis
                .Include(d => d.Monhoc)
                .Include(d => d.NguoitaoNavigation)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (dethi == null)
            {
                TempData["ErrorMessage"] = "Đề thi không tồn tại.";
                return RedirectToAction(nameof(Index));
            }

            return View(dethi);
        }

        // POST: Admin/Dethis/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(string id)
        {
            var dethi = await _context.Dethis.FindAsync(id);
            if (dethi == null)
            {
                TempData["ErrorMessage"] = "Đề thi không tồn tại hoặc đã bị xóa trước đó.";
                return RedirectToAction(nameof(Index));
            }

            try
            {
                _context.Dethis.Remove(dethi);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Đề thi đã được xóa thành công!";
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Đã xảy ra lỗi khi xóa đề thi: {ex.Message}";
            }
            return RedirectToAction(nameof(Index));
        }

        private bool DethiExists(string id)
        {
            return _context.Dethis.Any(e => e.Id == id);
        }
    }
}