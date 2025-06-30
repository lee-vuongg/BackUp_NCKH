using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using TCN_NCKH.Models.DBModel;
using TCN_NCKH.Models.ViewModels; // Thêm để sử dụng DethiListViewModel (và các ViewModels khác nếu có)
using Microsoft.Data.SqlClient; // Thêm để bắt lỗi SQL Server cụ thể

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
            Console.WriteLine($"DEBUG: [DapansController][Index] - Vào action Index. Page: {page}, PageSize: {pageSize}");

            // Đảm bảo số trang và kích thước trang hợp lệ
            if (page < 1) page = 1;
            if (pageSize < 1) pageSize = 5;

            // Lấy tất cả đề thi để đếm tổng số mục trước khi phân trang
            // Sắp xếp theo ID để đảm bảo thứ tự nhất quán
            var query = _context.Dethis.OrderBy(d => d.Id);

            // Đếm tổng số đề thi
            var totalItems = await query.CountAsync();
            Console.WriteLine($"DEBUG: [DapansController][Index] - Tổng số đề thi: {totalItems}");

            // Tính tổng số trang
            var totalPages = (int)Math.Ceiling((double)totalItems / pageSize);
            Console.WriteLine($"DEBUG: [DapansController][Index] - Tổng số trang: {totalPages}");

            // Lấy danh sách đề thi cho trang hiện tại
            // CHUỖI INCLUDE ĐÃ ĐƯỢC SỬA LẠI ĐƠN GIẢN HƠN VÌ GIẢ ĐỊNH CAUHOI CÓ DETHIID TRỰC TIẾP
            var dethisOnPage = await query
                                        .Skip((page - 1) * pageSize) // Bỏ qua các đề thi của các trang trước
                                        .Take(pageSize) // Lấy số lượng đề thi cho trang hiện tại
                                        .Include(d => d.Cauhois.OrderBy(c => c.Id)) // Include Cauhois trực tiếp từ Dethi, sắp xếp theo ID
                                            .ThenInclude(ch => ch.Dapans.OrderBy(da => da.Id)) // Sau đó include Dapans từ Cauhois, sắp xếp theo ID
                                        .ToListAsync();

            Console.WriteLine($"DEBUG: [DapansController][Index] - Đã tải {dethisOnPage.Count} đề thi cho trang {page}.");
            foreach (var dethi in dethisOnPage)
            {
                Console.WriteLine($"DEBUG: Đề thi ID: {dethi.Id}, Tên: {dethi.Tendethi}, Số câu hỏi: {dethi.Cauhois?.Count ?? 0}");
                if (dethi.Cauhois != null)
                {
                    foreach (var cauhoi in dethi.Cauhois)
                    {
                        Console.WriteLine($"  - Câu hỏi ID: {cauhoi.Id}, Nội dung: {cauhoi.Noidung?.Substring(0, Math.Min(cauhoi.Noidung.Length, 50))}..., Số đáp án: {cauhoi.Dapans?.Count ?? 0}");
                        if (cauhoi.Dapans != null)
                        {
                            foreach (var dapan in cauhoi.Dapans)
                            {
                                Console.WriteLine($"    - Đáp án ID: {dapan.Id}, Nội dung: {dapan.Noidung?.Substring(0, Math.Min(dapan.Noidung.Length, 50))}..., Đúng: {dapan.Dung}");
                            }
                        }
                    }
                }
            }


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
            Console.WriteLine($"DEBUG: [DapansController][Details] - Vào action Details. ID: {id ?? null}");

            if (id == null)
            {
                TempData["ErrorMessage"] = "Không tìm thấy ID đáp án.";
                Console.WriteLine("DEBUG: [DapansController][Details] - ID đáp án là NULL, trả về NotFound.");
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
                Console.WriteLine($"DEBUG: [DapansController][Details] - Đáp án với ID {id} không tồn tại.");
                return RedirectToAction(nameof(Index));
            }

            Console.WriteLine($"DEBUG: [DapansController][Details] - Đã tìm thấy đáp án ID: {dapan.Id}, Nội dung: {dapan.Noidung}");
            Console.WriteLine($"DEBUG: [DapansController][Details] - Thuộc câu hỏi ID: {dapan.Cauhoiid}, Đề thi ID: {dapan.Cauhoi?.Dethiid}");


            // Populate Dethi dropdown
            ViewData["Dethiid"] = new SelectList(_context.Dethis.OrderBy(d => d.Tendethi), "Id", "Tendethi", dapan.Cauhoi?.Dethiid);

            // Populate Cauhoi dropdown, filtered by selected Dethiid (if any)
            var cauhoisForDethi = _context.Cauhois.AsQueryable();
            if (dapan.Cauhoi?.Dethiid != null) // Kiểm tra Dethiid có giá trị
            {
                Console.WriteLine($"DEBUG: [DapansController][Details] - Lọc câu hỏi theo Dethiid: {dapan.Cauhoi.Dethiid}");
                cauhoisForDethi = cauhoisForDethi.Where(c => c.Dethiid == dapan.Cauhoi.Dethiid);
            }
            ViewData["Cauhoiid"] = new SelectList(cauhoisForDethi.OrderBy(c => c.Noidung), "Id", "Noidung", dapan.Cauhoi?.Id);

            return View(dapan);
        }

        // GET: Admin/Dapans/Create
        public IActionResult Create()
        {
            Console.WriteLine("DEBUG: [DapansController][Create] - Vào action Create (GET).");
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
            Console.WriteLine($"DEBUG: [DapansController][Create] - Vào action Create (POST). Dữ liệu nhận được: CauhoiID={dapan.Cauhoiid}, NoiDung='{dapan.Noidung}', Dung={dapan.Dung}");

            if (ModelState.IsValid)
            {
                Console.WriteLine("DEBUG: [DapansController][Create] - ModelState.IsValid là TRUE.");
                try
                {
                    _context.Add(dapan);
                    await _context.SaveChangesAsync();
                    Console.WriteLine($"DEBUG: [DapansController][Create] - Đã lưu đáp án ID: {dapan.Id} thành công.");
                    TempData["SuccessMessage"] = "Đáp án đã được tạo thành công!";
                    return RedirectToAction(nameof(Index));
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"ERROR: [DapansController][Create] - Lỗi khi tạo đáp án: {ex.Message}");
                    TempData["ErrorMessage"] = $"Đã xảy ra lỗi khi tạo đáp án: {ex.Message}";
                }
            }
            else
            {
                Console.WriteLine("DEBUG: [DapansController][Create] - ModelState.IsValid là FALSE. Có lỗi validation.");
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
                Console.WriteLine($"DEBUG: [DapansController][Create] - Lỗi validation: {validationErrors}");
            }
            return View(dapan);
        }

        // GET: Admin/Dapans/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            Console.WriteLine($"DEBUG: [DapansController][Edit] - Vào action Edit (GET). ID: {id ?? null}");
            if (id == null)
            {
                TempData["ErrorMessage"] = "Không tìm thấy ID đáp án.";
                Console.WriteLine("DEBUG: [DapansController][Edit] - ID đáp án là NULL, trả về NotFound.");
                return NotFound();
            }

            // Load Dapan và Cauhoi, Dethi liên quan
            var dapan = await _context.Dapans
                                        .Include(d => d.Cauhoi)
                                            .ThenInclude(c => c.Dethi)
                                        .FirstOrDefaultAsync(d => d.Id == id);

            if (dapan == null)
            {
                TempData["ErrorMessage"] = "Đáp án không tồn tại.";
                Console.WriteLine($"DEBUG: [DapansController][Edit] - Đáp án với ID {id} không tồn tại.");
                return RedirectToAction(nameof(Index));
            }

            Console.WriteLine($"DEBUG: [DapansController][Edit] - Đã tìm thấy đáp án ID: {dapan.Id}, Nội dung: {dapan.Noidung}");
            Console.WriteLine($"DEBUG: [DapansController][Edit] - Thuộc câu hỏi ID: {dapan.Cauhoiid}, Đề thi ID: {dapan.Cauhoi?.Dethiid}");


            ViewData["Dethiid"] = new SelectList(_context.Dethis.OrderBy(d => d.Tendethi), "Id", "Tendethi", dapan.Cauhoi?.Dethiid);

            var cauhoisForDethi = _context.Cauhois.AsQueryable();
            if (dapan.Cauhoi?.Dethiid != null)
            {
                Console.WriteLine($"DEBUG: [DapansController][Edit] - Lọc câu hỏi theo Dethiid: {dapan.Cauhoi.Dethiid}");
                cauhoisForDethi = cauhoisForDethi.Where(c => c.Dethiid == dapan.Cauhoi.Dethiid);
            }
            ViewData["Cauhoiid"] = new SelectList(cauhoisForDethi.OrderBy(c => c.Noidung), "Id", "Noidung", dapan.Cauhoi?.Id);

            return View(dapan);
        }

        // POST: Admin/Dapans/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Cauhoiid,Noidung,Dung")] Dapan dapanFromForm)
        {
            Console.WriteLine($"DEBUG: [DapansController][Edit] - Vào action Edit (POST). ID từ route: {id}. Dữ liệu nhận được: CauhoiID={dapanFromForm.Cauhoiid}, NoiDung='{dapanFromForm.Noidung}', Dung={dapanFromForm.Dung}");

            if (id != dapanFromForm.Id)
            {
                TempData["ErrorMessage"] = "ID đáp án không hợp lệ.";
                Console.WriteLine($"DEBUG: [DapansController][Edit] - ID không khớp: {id} != {dapanFromForm.Id}.");
                return NotFound();
            }

            var existingDapan = await _context.Dapans.FindAsync(id);

            if (existingDapan == null)
            {
                TempData["ErrorMessage"] = "Đáp án không tồn tại.";
                Console.WriteLine($"DEBUG: [DapansController][Edit] - Đáp án với ID {id} không tồn tại trong DB.");
                return NotFound();
            }

            if (!ModelState.IsValid)
            {
                Console.WriteLine("DEBUG: [DapansController][Edit] - ModelState.IsValid là FALSE. Có lỗi validation.");
                string validationErrors = string.Join("<br/>", ModelState.Values
                                                    .SelectMany(v => v.Errors)
                                                    .Select(e => e.ErrorMessage));
                TempData["ErrorMessage"] = "Có lỗi xác thực: <br/>" + validationErrors;
                Console.WriteLine($"DEBUG: [DapansController][Edit] - Lỗi validation: {validationErrors}");

                // Repopulate ViewDatas on error, ensuring related Dethi/Cauhoi are loaded
                await _context.Entry(existingDapan).Reference(d => d.Cauhoi).LoadAsync(); // Load Cauhoi để truy cập DethiId

                ViewData["Dethiid"] = new SelectList(_context.Dethis.OrderBy(d => d.Tendethi), "Id", "Tendethi", existingDapan.Cauhoi?.Dethiid);

                var cauhoisForDethi = _context.Cauhois.AsQueryable();
                if (existingDapan.Cauhoi?.Dethiid != null)
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
                Console.WriteLine($"DEBUG: [DapansController][Edit] - Đang lưu thay đổi cho đáp án ID: {existingDapan.Id}.");
                await _context.SaveChangesAsync();
                Console.WriteLine($"DEBUG: [DapansController][Edit] - Đã cập nhật đáp án ID: {existingDapan.Id} thành công.");
                TempData["SuccessMessage"] = "Đáp án đã được cập nhật thành công!";
            }
            catch (DbUpdateConcurrencyException)
            {
                Console.WriteLine($"ERROR: [DapansController][Edit] - Lỗi đồng thời khi cập nhật đáp án ID: {id}.");
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
                Console.WriteLine($"ERROR: [DapansController][Edit] - Lỗi hệ thống khi cập nhật đáp án ID: {id}: {ex.Message}");
                TempData["ErrorMessage"] = $"Đã xảy ra lỗi hệ thống: {ex.Message}";
            }

            return RedirectToAction(nameof(Index));
        }

        // GET: Admin/Dapans/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            Console.WriteLine($"DEBUG: [DapansController][Delete] - Vào action Delete (GET). ID: {id ?? null}");
            if (id == null)
            {
                TempData["ErrorMessage"] = "Không tìm thấy ID đáp án.";
                Console.WriteLine("DEBUG: [DapansController][Delete] - ID đáp án là NULL, trả về NotFound.");
                return NotFound();
            }

            var dapan = await _context.Dapans
                .Include(d => d.Cauhoi)
                    .ThenInclude(c => c.Dethi) // Include Dethi for display in Details/Delete view
                .FirstOrDefaultAsync(m => m.Id == id);

            if (dapan == null)
            {
                TempData["ErrorMessage"] = "Đáp án không tồn tại.";
                Console.WriteLine($"DEBUG: [DapansController][Delete] - Đáp án với ID {id} không tồn tại.");
                return RedirectToAction(nameof(Index));
            }
            Console.WriteLine($"DEBUG: [DapansController][Delete] - Đã tìm thấy đáp án ID: {dapan.Id}, Nội dung: {dapan.Noidung}");
            return View(dapan);
        }

        // POST: Admin/Dapans/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            Console.WriteLine($"DEBUG: [DapansController][DeleteConfirmed] - Vào action DeleteConfirmed (POST). ID: {id}");
            var dapan = await _context.Dapans.FindAsync(id);
            if (dapan == null)
            {
                TempData["ErrorMessage"] = "Đáp án không tồn tại hoặc đã bị xóa trước đó.";
                Console.WriteLine($"DEBUG: [DapansController][DeleteConfirmed] - Đáp án với ID {id} không tồn tại.");
                return RedirectToAction(nameof(Index));
            }

            try
            {
                _context.Dapans.Remove(dapan);
                Console.WriteLine($"DEBUG: [DapansController][DeleteConfirmed] - Đang xóa đáp án ID: {dapan.Id}.");
                await _context.SaveChangesAsync();
                Console.WriteLine($"DEBUG: [DapansController][DeleteConfirmed] - Đã xóa đáp án ID: {dapan.Id} thành công.");
                TempData["SuccessMessage"] = "Đáp án đã được xóa thành công!";
            }
            catch (Exception ex)
            {
                Console.WriteLine($"ERROR: [DapansController][DeleteConfirmed] - Lỗi khi xóa đáp án ID {id}: {ex.Message}");
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
