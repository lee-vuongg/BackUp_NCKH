using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using TCN_NCKH.Models.DBModel;


namespace TCN_NCKH.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class DapansController : Controller
    {
        private readonly NghienCuuKhoaHocContext _context;

        public DapansController(NghienCuuKhoaHocContext context)
        {
            _context = context;
        }

        // GET: Admin/Dapans
        // Đã thêm tham số phân trang page và pageSize
        public async Task<IActionResult> Index(int page = 1, int pageSize = 5) // Mặc định 5 đề thi mỗi trang
        {
            // Đảm bảo số trang và kích thước trang hợp lệ
            if (page < 1) page = 1;
            if (pageSize < 1) pageSize = 5;

            // Lấy tất cả đề thi để đếm tổng số mục trước khi phân trang
            // Sắp xếp theo ID để đảm bảo thứ tự nhất quán
            var query = _context.Dethis.OrderBy(d => d.Id);

            // Đếm tổng số đề thi
            var totalItems = await query.CountAsync();
            // Tính tổng số trang
            var totalPages = (int)Math.Ceiling((double)totalItems / pageSize);

            // Lấy danh sách đề thi cho trang hiện tại
            // Eager load các Cauhoi của mỗi Dethi
            // Eager load các Dapans của mỗi Cauhoi
            var dethisOnPage = await query
                                    .Skip((page - 1) * pageSize) // Bỏ qua các đề thi của các trang trước
                                    .Take(pageSize) // Lấy số lượng đề thi cho trang hiện tại
                                    .Include(d => d.Cauhois.OrderBy(c => c.Id)) // Bao gồm câu hỏi, sắp xếp theo ID
                                        .ThenInclude(c => c.Dapans.OrderBy(da => da.Id)) // Bao gồm đáp án, sắp xếp theo ID
                                    .ToListAsync();

            // Tạo ViewModel để truyền dữ liệu và thông tin phân trang sang View
            var viewModel = new DethiListViewModel
            {
                Dethis = dethisOnPage,
                PageNumber = page,
                PageSize = pageSize,
                TotalPages = totalPages,
                TotalItems = totalItems
            };

            return View(viewModel);
        }

        // GET: Admin/Dapans/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                TempData["ErrorMessage"] = "Không tìm thấy ID đáp án.";
                return NotFound();
            }

            var dapan = await _context.Dapans
                .Include(d => d.Cauhoi)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (dapan == null)
            {
                TempData["ErrorMessage"] = "Đáp án không tồn tại.";
                return RedirectToAction(nameof(Index));
            }

            return View(dapan);
        }

        // GET: Admin/Dapans/Create
        public IActionResult Create()
        {
            // Vẫn cần DethiId để lọc Cauhoi sau này nếu muốn
            ViewData["Dethiid"] = new SelectList(_context.Dethis.OrderBy(d => d.Tendethi), "Id", "Tendethi");
            ViewData["Cauhoiid"] = new SelectList(_context.Cauhois.OrderBy(c => c.Noidung), "Id", "Noidung");
            return View();
        }

        // POST: Admin/Dapans/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Cauhoiid,Noidung,Dung")] Dapan dapan)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    _context.Add(dapan);
                    await _context.SaveChangesAsync();
                    TempData["SuccessMessage"] = "Đáp án đã được tạo thành công!";
                    return RedirectToAction(nameof(Index));
                }
                catch (Exception ex)
                {
                    TempData["ErrorMessage"] = $"Đã xảy ra lỗi khi tạo đáp án: {ex.Message}";
                }
            }
            // Populate ViewDatas again on error
            ViewData["Dethiid"] = new SelectList(_context.Dethis.OrderBy(d => d.Tendethi), "Id", "Tendethi");
            ViewData["Cauhoiid"] = new SelectList(_context.Cauhois.OrderBy(c => c.Noidung), "Id", "Noidung", dapan.Cauhoiid);
            if (TempData["ErrorMessage"] == null)
            {
                TempData["ErrorMessage"] = "Thông tin đáp án không hợp lệ. Vui lòng kiểm tra lại.";
            }
            string validationErrors = string.Join("<br/>", ModelState.Values
                                                  .SelectMany(v => v.Errors)
                                                  .Select(e => e.ErrorMessage));
            if (!string.IsNullOrEmpty(validationErrors))
            {
                TempData["ErrorMessage"] = (TempData["ErrorMessage"] as string ?? "") + "<br/>" + validationErrors;
            }
            return View(dapan);
        }

        // GET: Admin/Dapans/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                TempData["ErrorMessage"] = "Không tìm thấy ID đáp án.";
                return NotFound();
            }

            // Load Dapan và Cauhoi, Dethi liên quan
            var dapan = await _context.Dapans
                                    .Include(d => d.Cauhoi)
                                        .ThenInclude(c => c.Dethi) // Cần để lấy Dethiid cho dropdown
                                    .FirstOrDefaultAsync(d => d.Id == id);

            if (dapan == null)
            {
                TempData["ErrorMessage"] = "Đáp án không tồn tại.";
                return RedirectToAction(nameof(Index));
            }

            // Populate Dethi dropdown
            ViewData["Dethiid"] = new SelectList(_context.Dethis.OrderBy(d => d.Tendethi), "Id", "Tendethi", dapan.Cauhoi?.Dethiid);

            // Populate Cauhoi dropdown, filtered by selected Dethiid (if any)
            var cauhoisForDethi = _context.Cauhois.AsQueryable();
            if (!string.IsNullOrEmpty(dapan.Cauhoi?.Dethiid))
            {
                cauhoisForDethi = cauhoisForDethi.Where(c => c.Dethiid == dapan.Cauhoi.Dethiid);
            }
            ViewData["Cauhoiid"] = new SelectList(cauhoisForDethi.OrderBy(c => c.Noidung), "Id", "Noidung", dapan.Cauhoiid);

            return View(dapan);
        }

        // POST: Admin/Dapans/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Cauhoiid,Noidung,Dung")] Dapan dapanFromForm)
        {
            if (id != dapanFromForm.Id)
            {
                TempData["ErrorMessage"] = "ID đáp án không hợp lệ.";
                return NotFound();
            }

            var existingDapan = await _context.Dapans.FindAsync(id);

            if (existingDapan == null)
            {
                TempData["ErrorMessage"] = "Đáp án không tồn tại.";
                return NotFound();
            }

            if (!ModelState.IsValid)
            {
                string validationErrors = string.Join("<br/>", ModelState.Values
                                                      .SelectMany(v => v.Errors)
                                                      .Select(e => e.ErrorMessage));
                TempData["ErrorMessage"] = "Có lỗi xác thực: <br/>" + validationErrors;

                // Repopulate ViewDatas on error, ensuring related Dethi/Cauhoi are loaded
                await _context.Entry(existingDapan).Reference(d => d.Cauhoi).LoadAsync(); // Load Cauhoi để truy cập DethiId

                ViewData["Dethiid"] = new SelectList(_context.Dethis.OrderBy(d => d.Tendethi), "Id", "Tendethi", existingDapan.Cauhoi?.Dethiid);

                var cauhoisForDethi = _context.Cauhois.AsQueryable();
                if (!string.IsNullOrEmpty(existingDapan.Cauhoi?.Dethiid))
                {
                    cauhoisForDethi = cauhoisForDethi.Where(c => c.Dethiid == existingDapan.Cauhoi.Dethiid);
                }
                ViewData["Cauhoiid"] = new SelectList(cauhoisForDethi.OrderBy(c => c.Noidung), "Id", "Noidung", existingDapan.Cauhoiid);

                return View(existingDapan);
            }

            try
            {
                // Cập nhật các giá trị của existingDapan với dapanFromForm
                _context.Entry(existingDapan).CurrentValues.SetValues(dapanFromForm);

                // Nếu bạn muốn đảm bảo Cauhoi navigation property được load sau khi thay đổi Cauhoiid (nếu cần)
                // if (existingDapan.Cauhoiid != dapanFromForm.Cauhoiid)
                // {
                //     await _context.Entry(existingDapan).Reference(d => d.Cauhoi).LoadAsync();
                // }

                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Đáp án đã được cập nhật thành công!";
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!DapanExists(dapanFromForm.Id))
                {
                    TempData["ErrorMessage"] = "Đáp án không tồn tại hoặc đã bị xóa bởi người khác.";
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

        // GET: Admin/Dapans/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                TempData["ErrorMessage"] = "Không tìm thấy ID đáp án.";
                return NotFound();
            }

            var dapan = await _context.Dapans
                .Include(d => d.Cauhoi)
                    .ThenInclude(c => c.Dethi) // Include Dethi for display in Details/Delete view
                .FirstOrDefaultAsync(m => m.Id == id);

            if (dapan == null)
            {
                TempData["ErrorMessage"] = "Đáp án không tồn tại.";
                return RedirectToAction(nameof(Index));
            }

            return View(dapan);
        }

        // POST: Admin/Dapans/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var dapan = await _context.Dapans.FindAsync(id);
            if (dapan == null)
            {
                TempData["ErrorMessage"] = "Đáp án không tồn tại hoặc đã bị xóa trước đó.";
                return RedirectToAction(nameof(Index));
            }

            try
            {
                _context.Dapans.Remove(dapan);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Đáp án đã được xóa thành công!";
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Đã xảy ra lỗi khi xóa đáp án: {ex.Message}";
            }
            return RedirectToAction(nameof(Index));
        }

        private bool DapanExists(int id)
        {
            return _context.Dapans.Any(e => e.Id == id);
        }
    }
}