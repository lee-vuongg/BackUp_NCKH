using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using TCN_NCKH.Models.DBModel;
using System.Security.Claims; // Cần thiết để lấy thông tin người dùng từ Claims

namespace TCN_NCKH.Areas.GiaoVien.Controllers
{
    [Area("GiaoVien")] // Đảm bảo area là "GiaoVien"
    public class DethisController : Controller
    {
        private readonly NghienCuuKhoaHocContext _context;

        public DethisController(NghienCuuKhoaHocContext context)
        {
            _context = context;
        }

        // GET: GiaoVien/Dethis
        // Phương thức Index được cập nhật để trả về toàn bộ đề thi của giáo viên hiện tại cho DataTables xử lý
        public async Task<IActionResult> Index()
        {
            // Lấy email của giáo viên hiện tại từ Claims
            var userEmail = User.FindFirstValue(ClaimTypes.Email);
            // Tìm thông tin giáo viên trong bảng Nguoidung dựa trên email
            var currentGiaoVien = await _context.Nguoidungs.FirstOrDefaultAsync(u => u.Email == userEmail);

            // Kiểm tra nếu không tìm thấy thông tin giáo viên (có thể do session hết hạn hoặc lỗi đăng nhập)
            if (currentGiaoVien == null)
            {
                TempData["ErrorMessage"] = "Không tìm thấy thông tin tài khoản giáo viên của bạn. Vui lòng đăng nhập lại.";
                // Chuyển hướng về trang đăng nhập chung (đảm bảo đường dẫn này là chính xác)
                return RedirectToAction("Login", "Auth", new { area = "" });
            }

            // Truy vấn đề thi:
            // - Bao gồm thông tin Môn học và Người tạo
            // - LỌC CHỈ NHỮNG ĐỀ THI MÀ GIÁO VIÊN HIỆN TẠI LÀ NGƯỜI TẠO
            // - Sắp xếp theo ngày tạo giảm dần (mới nhất trước)
            var dethisOfCurrentGiaoVien = _context.Dethis
                                        .Include(d => d.Monhoc)
                                        .Include(d => d.NguoitaoNavigation)
                                        .Where(d => d.Nguoitao == currentGiaoVien.Id) // <-- Lọc theo người tạo
                                        .OrderByDescending(d => d.Ngaytao);

            return View(await dethisOfCurrentGiaoVien.ToListAsync()); // Trả về list đã lọc
        }

        // --- Giữ nguyên các Action Details, Create, Edit, Delete như Controller Giáo viên đã chỉnh sửa trước đó ---
        // (Với logic kiểm tra quyền sở hữu và thông báo TempData)

        // GET: GiaoVien/Dethis/Details/5
        public async Task<IActionResult> Details(string id)
        {
            if (id == null) return NotFound();
            var userEmail = User.FindFirstValue(ClaimTypes.Email);
            var currentGiaoVien = await _context.Nguoidungs.FirstOrDefaultAsync(u => u.Email == userEmail);
            if (currentGiaoVien == null) { TempData["ErrorMessage"] = "Không tìm thấy thông tin tài khoản giáo viên của bạn."; return RedirectToAction("Login", "Auth", new { area = "" }); }
            var dethi = await _context.Dethis
                .Include(d => d.Monhoc)
                .Include(d => d.NguoitaoNavigation)
                .FirstOrDefaultAsync(m => m.Id == id && m.Nguoitao == currentGiaoVien.Id);
            if (dethi == null) { TempData["ErrorMessage"] = "Đề thi không tồn tại hoặc bạn không có quyền truy cập đề thi này."; return RedirectToAction(nameof(Index)); }
            return View(dethi);
        }

        // GET: GiaoVien/Dethis/Create
        public IActionResult Create()
        {
            ViewData["Monhocid"] = new SelectList(_context.Monhocs, "Id", "Tenmon"); // Đổi Tenmh -> Tenmon
            return View();
        }

        // POST: GiaoVien/Dethis/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Tendethi,Monhocid")] Dethi dethi)
        {
            var userEmail = User.FindFirstValue(ClaimTypes.Email);
            var currentGiaoVien = await _context.Nguoidungs.FirstOrDefaultAsync(u => u.Email == userEmail);
            if (currentGiaoVien == null) { TempData["ErrorMessage"] = "Không tìm thấy thông tin tài khoản giáo viên của bạn."; return RedirectToAction("Login", "Auth", new { area = "" }); }

            dethi.Nguoitao = currentGiaoVien.Id;
            dethi.Ngaytao = DateTime.Now;
            dethi.Id = Guid.NewGuid().ToString();

            if (ModelState.IsValid)
            {
                _context.Add(dethi);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Đề thi đã được tạo thành công!";
                return RedirectToAction(nameof(Index));
            }
            ViewData["Monhocid"] = new SelectList(_context.Monhocs, "Id", "Tenmon", dethi.Monhocid); // Đổi Tenmh -> Tenmon
            TempData["ErrorMessage"] = "Có lỗi xảy ra khi tạo đề thi. Vui lòng kiểm tra lại thông tin.";
            return View(dethi);
        }

        // GET: GiaoVien/Dethis/Edit/5
        public async Task<IActionResult> Edit(string id)
        {
            if (id == null) return NotFound();
            var userEmail = User.FindFirstValue(ClaimTypes.Email);
            var currentGiaoVien = await _context.Nguoidungs.FirstOrDefaultAsync(u => u.Email == userEmail);
            if (currentGiaoVien == null) { TempData["ErrorMessage"] = "Không tìm thấy thông tin tài khoản giáo viên của bạn."; return RedirectToAction("Login", "Auth", new { area = "" }); }

            var dethi = await _context.Dethis.FindAsync(id);
            if (dethi == null || dethi.Nguoitao != currentGiaoVien.Id) { TempData["ErrorMessage"] = "Đề thi không tồn tại hoặc bạn không có quyền chỉnh sửa đề thi này."; return RedirectToAction(nameof(Index)); }

            ViewData["Monhocid"] = new SelectList(_context.Monhocs, "Id", "Tenmon", dethi.Monhocid); // Đổi Tenmh -> Tenmon
            return View(dethi);
        }

        // POST: GiaoVien/Dethis/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(string id, [Bind("Id,Tendethi,Monhocid")] Dethi dethi)
        {
            if (id != dethi.Id) return NotFound();
            var userEmail = User.FindFirstValue(ClaimTypes.Email);
            var currentGiaoVien = await _context.Nguoidungs.FirstOrDefaultAsync(u => u.Email == userEmail);
            if (currentGiaoVien == null) { TempData["ErrorMessage"] = "Không tìm thấy thông tin tài khoản giáo viên của bạn."; return RedirectToAction("Login", "Auth", new { area = "" }); }

            var dethiToUpdate = await _context.Dethis.AsNoTracking().FirstOrDefaultAsync(m => m.Id == id);
            if (dethiToUpdate == null || dethiToUpdate.Nguoitao != currentGiaoVien.Id) { TempData["ErrorMessage"] = "Đề thi không tồn tại hoặc bạn không có quyền chỉnh sửa đề thi này."; return RedirectToAction(nameof(Index)); }

            dethi.Nguoitao = dethiToUpdate.Nguoitao;
            dethi.Ngaytao = dethiToUpdate.Ngaytao;

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(dethi);
                    await _context.SaveChangesAsync();
                    TempData["SuccessMessage"] = "Đề thi đã được cập nhật thành công!";
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!DethiExists(dethi.Id)) { TempData["ErrorMessage"] = "Đề thi không tồn tại."; return RedirectToAction(nameof(Index)); }
                    else { throw; }
                }
                return RedirectToAction(nameof(Index));
            }
            ViewData["Monhocid"] = new SelectList(_context.Monhocs, "Id", "Tenmon", dethi.Monhocid); // Đổi Tenmh -> Tenmon
            TempData["ErrorMessage"] = "Có lỗi xảy ra khi cập nhật đề thi. Vui lòng kiểm tra lại thông tin.";
            return View(dethi);
        }

        // GET: GiaoVien/Dethis/Delete/5
        public async Task<IActionResult> Delete(string id)
        {
            if (id == null) return NotFound();
            var userEmail = User.FindFirstValue(ClaimTypes.Email);
            var currentGiaoVien = await _context.Nguoidungs.FirstOrDefaultAsync(u => u.Email == userEmail);
            if (currentGiaoVien == null) { TempData["ErrorMessage"] = "Không tìm thấy thông tin tài khoản giáo viên của bạn."; return RedirectToAction("Login", "Auth", new { area = "" }); }

            var dethi = await _context.Dethis
                .Include(d => d.Monhoc)
                .Include(d => d.NguoitaoNavigation)
                .FirstOrDefaultAsync(m => m.Id == id && m.Nguoitao == currentGiaoVien.Id);
            if (dethi == null) { TempData["ErrorMessage"] = "Đề thi không tồn tại hoặc bạn không có quyền xóa đề thi này."; return RedirectToAction(nameof(Index)); }

            return View(dethi);
        }

        // POST: GiaoVien/Dethis/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(string id)
        {
            var userEmail = User.FindFirstValue(ClaimTypes.Email);
            var currentGiaoVien = await _context.Nguoidungs.FirstOrDefaultAsync(u => u.Email == userEmail);
            if (currentGiaoVien == null) { TempData["ErrorMessage"] = "Không tìm thấy thông tin tài khoản giáo viên của bạn."; return RedirectToAction("Login", "Auth", new { area = "" }); }

            var dethi = await _context.Dethis.FindAsync(id);
            if (dethi == null || dethi.Nguoitao != currentGiaoVien.Id) { TempData["ErrorMessage"] = "Đề thi không tồn tại hoặc bạn không có quyền xóa đề thi này."; return RedirectToAction(nameof(Index)); }

            _context.Dethis.Remove(dethi);
            await _context.SaveChangesAsync();
            TempData["SuccessMessage"] = "Đề thi đã được xóa thành công!";
            return RedirectToAction(nameof(Index));
        }

        private bool DethiExists(string id)
        {
            return _context.Dethis.Any(e => e.Id == id);
        }
    }
}