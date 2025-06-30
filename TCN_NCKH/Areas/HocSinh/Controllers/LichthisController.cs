using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using TCN_NCKH.Models.DBModel;

namespace TCN_NCKH.Areas.HocSinh.Controllers
{
    [Area("HocSinh")]
    public class LichthisController : Controller
    {
        private readonly NghienCuuKhoaHocContext _context;

        public LichthisController(NghienCuuKhoaHocContext context)
        {
            _context = context;
        }

        // GET: HocSinh/Lichthis
        // Hiển thị danh sách các lịch thi có sẵn cho học sinh
        public async Task<IActionResult> Index()
        {
            // Tải danh sách lịch thi, bao gồm thông tin đề thi và lớp học
            // Sắp xếp theo ngày thi để lịch thi mới nhất/sắp tới hiển thị đầu tiên
            var lichthis = await _context.Lichthis
                                         .Include(l => l.Dethi)
                                             .ThenInclude(d => d.Monhoc) // Eager load Môn học của Đề thi
                                         .Include(l => l.Lophoc)
                                         .OrderByDescending(l => l.Ngaythi) // Sắp xếp giảm dần theo ngày thi
                                         .ToListAsync();
            return View(lichthis);
        }

        // GET: HocSinh/Lichthis/Details/5
        // Hiển thị chi tiết một lịch thi cụ thể
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                TempData["ErrorMessage"] = "Không tìm thấy ID lịch thi.";
                return NotFound(); // Trả về lỗi 404
            }

            var lichthi = await _context.Lichthis
                .Include(l => l.Dethi)
                .ThenInclude(d => d.Monhoc) // Tải thông tin môn học của đề thi
                .Include(l => l.Lophoc)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (lichthi == null)
            {
                TempData["ErrorMessage"] = "Lịch thi không tồn tại.";
                return RedirectToAction(nameof(Index)); // Chuyển hướng về trang Index với thông báo
            }

            return View(lichthi);
        }

        // GET: HocSinh/Lichthis/Create
        // (LƯU Ý: Học sinh thường không tạo lịch thi. Action này có thể dành cho mục đích Admin hoặc phát triển)
        public IActionResult Create()
        {
            // Populate ViewDatas cho dropdowns
            PopulateLichthiViewData();
            return View();
        }

        // POST: HocSinh/Lichthis/Create
        // (LƯU Ý: Học sinh thường không tạo lịch thi. Action này có thể dành cho mục đích Admin hoặc phát triển)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Dethiid,Lophocid,Ngaythi,Thoigian,Phongthi,ThoigianBatdau,ThoigianKetthuc")] Lichthi lichthi)
        {
            // Kiểm tra Ngaythi và Thoigian để đảm bảo tính hợp lệ của thời gian kết thúc
            // Sửa lỗi CS1503: AddMinutes yêu cầu double, Thoigian là int?. Sử dụng GetValueOrDefault(0).
            if (lichthi.Ngaythi.AddMinutes(lichthi.Thoigian.GetValueOrDefault(0)) < DateTime.Now)
            {
                ModelState.AddModelError("Ngaythi", "Thời gian kết thúc phải lớn hơn thời gian hiện tại.");
                ModelState.AddModelError("Thoigian", "Thời gian kết thúc phải lớn hơn thời gian hiện tại.");
            }

            // Nếu ThoigianBatdau hoặc ThoigianKetthuc được cung cấp, cũng cần kiểm tra logic
            if (lichthi.ThoigianBatdau.HasValue && lichthi.ThoigianKetthuc.HasValue)
            {
                if (lichthi.ThoigianBatdau.Value >= lichthi.ThoigianKetthuc.Value)
                {
                    ModelState.AddModelError("ThoigianKetthuc", "Thời gian kết thúc phải sau thời gian bắt đầu.");
                }
            }


            if (ModelState.IsValid)
            {
                try
                {
                    _context.Add(lichthi);
                    await _context.SaveChangesAsync();
                    TempData["SuccessMessage"] = "Lịch thi đã được tạo thành công!";
                    return RedirectToAction(nameof(Index));
                }
                catch (Exception ex)
                {
                    TempData["ErrorMessage"] = $"Đã xảy ra lỗi khi tạo lịch thi: {ex.Message}";
                }
            }
            // Nếu ModelState không hợp lệ hoặc có lỗi, populate lại ViewDatas và hiển thị lỗi
            // Sửa lỗi CS1503: Argument 2 for PopulateLichthiViewData. Dựa trên model, Lophocid là string?.
            PopulateLichthiViewData(lichthi.Dethiid, lichthi.Lophocid);
            string validationErrors = string.Join("<br/>", ModelState.Values
                                                                    .SelectMany(v => v.Errors)
                                                                    .Select(e => e.ErrorMessage));
            if (!string.IsNullOrEmpty(validationErrors))
            {
                TempData["ErrorMessage"] = (TempData["ErrorMessage"] as string ?? "") + "<br/>" + validationErrors;
            }
            else if (TempData["ErrorMessage"] == null)
            {
                TempData["ErrorMessage"] = "Thông tin lịch thi không hợp lệ. Vui lòng kiểm tra lại.";
            }
            return View(lichthi);
        }

        // GET: HocSinh/Lichthis/Edit/5
        // (LƯU Ý: Học sinh thường không chỉnh sửa lịch thi. Action này có thể dành cho mục đích Admin hoặc phát triển)
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                TempData["ErrorMessage"] = "Không tìm thấy ID lịch thi.";
                return NotFound();
            }

            // Cần Include Dethi và Lophoc nếu Edit View hiển thị thông tin này chi tiết.
            // Nếu View chỉ dùng cho dropdown thì FindAsync() là đủ.
            var lichthi = await _context.Lichthis
                                        .Include(l => l.Dethi)
                                            .ThenInclude(d => d.Monhoc)
                                        .Include(l => l.Lophoc)
                                        .FirstOrDefaultAsync(m => m.Id == id);

            if (lichthi == null)
            {
                TempData["ErrorMessage"] = "Lịch thi không tồn tại.";
                return RedirectToAction(nameof(Index));
            }
            // Populate ViewDatas cho dropdowns, chọn giá trị hiện tại
            // Sửa lỗi CS1503: Argument 2 for PopulateLichthiViewData. Dựa trên model, Lophocid là string?.
            PopulateLichthiViewData(lichthi.Dethiid, lichthi.Lophocid);
            return View(lichthi);
        }

        // POST: HocSinh/Lichthis/Edit/5
        // (LƯU Ý: Học sinh thường không chỉnh sửa lịch thi. Action này có thể dành cho mục đích Admin hoặc phát triển)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Dethiid,Lophocid,Ngaythi,Thoigian,Phongthi,ThoigianBatdau,ThoigianKetthuc")] Lichthi lichthi)
        {
            if (id != lichthi.Id)
            {
                TempData["ErrorMessage"] = "ID lịch thi không hợp lệ.";
                return NotFound();
            }

            // Kiểm tra Ngaythi và Thoigian để đảm bảo tính hợp lệ của thời gian kết thúc
            // Sửa lỗi CS1503: AddMinutes yêu cầu double, Thoigian là int?. Sử dụng GetValueOrDefault(0).
            if (lichthi.Ngaythi.AddMinutes(lichthi.Thoigian.GetValueOrDefault(0)) < DateTime.Now)
            {
                ModelState.AddModelError("Ngaythi", "Thời gian kết thúc phải lớn hơn thời gian hiện tại.");
                ModelState.AddModelError("Thoigian", "Thời gian kết thúc phải lớn hơn thời gian hiện tại.");
            }

            // Nếu ThoigianBatdau hoặc ThoigianKetthuc được cung cấp, cũng cần kiểm tra logic
            if (lichthi.ThoigianBatdau.HasValue && lichthi.ThoigianKetthuc.HasValue)
            {
                if (lichthi.ThoigianBatdau.Value >= lichthi.ThoigianKetthuc.Value)
                {
                    ModelState.AddModelError("ThoigianKetthuc", "Thời gian kết thúc phải sau thời gian bắt đầu.");
                }
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(lichthi);
                    await _context.SaveChangesAsync();
                    TempData["SuccessMessage"] = "Lịch thi đã được cập nhật thành công!";
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!LichthiExists(lichthi.Id))
                    {
                        TempData["ErrorMessage"] = "Lịch thi không tồn tại hoặc đã bị xóa bởi người khác.";
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
            // Nếu ModelState không hợp lệ hoặc có lỗi, populate lại ViewDatas và hiển thị lỗi
            // Sửa lỗi CS1503: Argument 2 for PopulateLichthiViewData. Dựa trên model, Lophocid là string?.
            PopulateLichthiViewData(lichthi.Dethiid, lichthi.Lophocid);
            string validationErrors = string.Join("<br/>", ModelState.Values
                                                                    .SelectMany(v => v.Errors)
                                                                    .Select(e => e.ErrorMessage));
            if (!string.IsNullOrEmpty(validationErrors))
            {
                TempData["ErrorMessage"] = (TempData["ErrorMessage"] as string ?? "") + "<br/>" + validationErrors;
            }
            else if (TempData["ErrorMessage"] == null)
            {
                TempData["ErrorMessage"] = "Thông tin lịch thi không hợp lệ. Vui lòng kiểm tra lại.";
            }
            return View(lichthi);
        }

        // GET: HocSinh/Lichthis/Delete/5
        // (LƯU Ý: Học sinh thường không xóa lịch thi. Action này có thể dành cho mục đích Admin hoặc phát triển)
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                TempData["ErrorMessage"] = "Không tìm thấy ID lịch thi.";
                return NotFound();
            }

            var lichthi = await _context.Lichthis
                .Include(l => l.Dethi)
                .Include(l => l.Lophoc)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (lichthi == null)
            {
                TempData["ErrorMessage"] = "Lịch thi không tồn tại.";
                return RedirectToAction(nameof(Index));
            }

            return View(lichthi);
        }

        // POST: HocSinh/Lichthis/Delete/5
        // (LƯU Ý: Học sinh thường không xóa lịch thi. Action này có thể dành cho mục đích Admin hoặc phát triển)
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var lichthi = await _context.Lichthis.FindAsync(id);
            if (lichthi == null)
            {
                TempData["ErrorMessage"] = "Lịch thi không tồn tại hoặc đã bị xóa trước đó.";
                return RedirectToAction(nameof(Index));
            }

            try
            {
                _context.Lichthis.Remove(lichthi);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Lịch thi đã được xóa thành công!";
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Đã xảy ra lỗi khi xóa lịch thi: {ex.Message}";
            }
            return RedirectToAction(nameof(Index));
        }

        // Helper method to populate ViewDatas for Create and Edit actions
        // Sửa kiểu của selectedLophocId thành string? dựa trên model Lichthi.
        private void PopulateLichthiViewData(string? selectedDethiId = null, string? selectedLophocId = null)
        {
            // SelectList cho Đề thi: Hiển thị tên đề thi và tên môn học
            var dethis = _context.Dethis
                                 .Include(d => d.Monhoc)
                                 .AsEnumerable() // Đảm bảo thực thi trên client để tạo chuỗi DisplayText
                                 .OrderBy(d => d.Tendethi)
                                 .Select(d => new
                                 {
                                     Id = d.Id,
                                     // Sửa lỗi CS1501: Thoigian không phải DateTime để định dạng "dd/MM".
                                     // Giữ lại DisplayText chỉ cho Dethi và Monhoc.
                                     DisplayText = $"{d.Tendethi} - ({d.Monhoc?.Tenmon ?? "Không rõ môn"})"
                                 })
                                 .ToList();
            ViewData["Dethiid"] = new SelectList(dethis, "Id", "DisplayText", selectedDethiId);

            // SelectList cho Lớp học: Hiển thị tên lớp
            var lophocs = _context.Lophocs
                                  .OrderBy(l => l.Tenlop)
                                  .ToList();
            ViewData["Lophocid"] = new SelectList(lophocs, "Id", "Tenlop", selectedLophocId);
        }

        private bool LichthiExists(int id)
        {
            return _context.Lichthis.Any(e => e.Id == id);
        }
    }
}