using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using TCN_NCKH.Models.DBModel;
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

        // GET: Admin/Dapans (Giờ đây hiển thị danh sách CÂU HỎI)
        public async Task<IActionResult> Index(int? selectedBoCauHoiId, string selectedDeThiId, int? selectedCauHoiId, int page = 1, int pageSize = 10) // Mặc định 10 câu hỏi mỗi trang
        {
            Console.WriteLine($"DEBUG: [DapansController][Index] - Vào action Index (hiển thị Câu hỏi). SelectedBoCauHoiId: {selectedBoCauHoiId}, SelectedDeThiId: {selectedDeThiId}, SelectedCauHoiId: {selectedCauHoiId}, Page: {page}, PageSize: {pageSize}");

            if (page < 1) page = 1;
            if (pageSize < 1) pageSize = 10; // Kích thước trang cho Câu hỏi

            // Bắt đầu query các Câu hỏi
            IQueryable<Cauhoi> cauhoisQuery = _context.Cauhois
                                                .Include(c => c.Bocauhoi) // Bao gồm Bộ câu hỏi
                                                .Include(c => c.Dapans)   // Bao gồm các Đáp án của câu hỏi
                                                .OrderBy(c => c.Id); // Sắp xếp để đảm bảo thứ tự nhất quán

            // Áp dụng bộ lọc Đề thi nếu có
            if (!string.IsNullOrEmpty(selectedDeThiId))
            {
                var dethi = await _context.Dethis.AsNoTracking().FirstOrDefaultAsync(d => d.Id == selectedDeThiId);
                if (dethi?.Bocauhoiid.HasValue == true)
                {
                    cauhoisQuery = cauhoisQuery.Where(c => c.Bocauhoiid == dethi.Bocauhoiid.Value);
                    Console.WriteLine($"DEBUG: [DapansController][Index] - Lọc theo Đề thi ID: {selectedDeThiId}, Bộ câu hỏi liên quan: {dethi.Bocauhoiid.Value}");
                    // Khi lọc theo Đề thi, ưu tiên bộ câu hỏi của đề thi đó
                    selectedBoCauHoiId = dethi.Bocauhoiid.Value;
                    selectedCauHoiId = null; // Reset câu hỏi khi lọc theo đề thi
                }
                else
                {
                    Console.WriteLine($"DEBUG: [DapansController][Index] - Không tìm thấy Đề thi với ID {selectedDeThiId} hoặc không có Bộ câu hỏi liên kết.");
                }
            }

            // Áp dụng bộ lọc Bộ câu hỏi nếu có
            if (selectedBoCauHoiId.HasValue)
            {
                cauhoisQuery = cauhoisQuery.Where(c => c.Bocauhoiid == selectedBoCauHoiId.Value);
                Console.WriteLine($"DEBUG: [DapansController][Index] - Lọc theo Bộ câu hỏi ID: {selectedBoCauHoiId.Value}");
            }

            // Áp dụng bộ lọc Câu hỏi nếu có (cho trường hợp tìm kiếm cụ thể 1 câu hỏi)
            if (selectedCauHoiId.HasValue)
            {
                cauhoisQuery = cauhoisQuery.Where(c => c.Id == selectedCauHoiId.Value);
                Console.WriteLine($"DEBUG: [DapansController][Index] - Lọc theo Câu hỏi ID: {selectedCauHoiId.Value}");
            }

            // Đếm tổng số câu hỏi sau khi lọc
            var totalItems = await cauhoisQuery.CountAsync();
            Console.WriteLine($"DEBUG: [DapansController][Index] - Tổng số câu hỏi (sau lọc): {totalItems}");

            // Tính tổng số trang
            var totalPages = (int)Math.Ceiling((double)totalItems / pageSize);
            Console.WriteLine($"DEBUG: [DapansController][Index] - Tổng số trang: {totalPages}");

            // Lấy danh sách câu hỏi cho trang hiện tại
            var cauhoisOnPage = await cauhoisQuery
                                        .Skip((page - 1) * pageSize)
                                        .Take(pageSize)
                                        .ToListAsync();

            Console.WriteLine($"DEBUG: [DapansController][Index] - Đã tải {cauhoisOnPage.Count} câu hỏi cho trang {page}.");

            // Tạo ViewModel cho từng Câu hỏi để bao gồm tên Đề thi liên quan
            var questionViewModels = new List<QuestionViewModelForIndex>();

            foreach (var cauhoi in cauhoisOnPage)
            {
                List<string> relatedDeThiNames = new List<string>();
                if (cauhoi.Bocauhoiid.HasValue)
                {
                    var relatedDethis = await _context.Dethis
                                                      .Where(d => d.Bocauhoiid == cauhoi.Bocauhoiid.Value)
                                                      .Select(d => d.Tendethi)
                                                      .ToListAsync();
                    relatedDeThiNames.AddRange(relatedDethis);
                }

                questionViewModels.Add(new QuestionViewModelForIndex
                {
                    Cauhoi = cauhoi,
                    RelatedDeThiNames = relatedDeThiNames
                });
            }

            // Populate dropdowns cho các bộ lọc trên View
            ViewData["BoCauHoiId"] = new SelectList(await _context.Bocauhois.OrderBy(b => b.Tenbocauhoi).ToListAsync(), "Id", "Tenbocauhoi", selectedBoCauHoiId);
            ViewData["DeThiId"] = new SelectList(await _context.Dethis.OrderBy(d => d.Tendethi).ToListAsync(), "Id", "Tendethi", selectedDeThiId);

            // Lọc danh sách câu hỏi cho dropdown "Câu hỏi" dựa trên selectedBoCauHoiId
            var cauhoisForFilterDropdown = _context.Cauhois.AsQueryable();
            if (selectedBoCauHoiId.HasValue)
            {
                cauhoisForFilterDropdown = cauhoisForFilterDropdown.Where(c => c.Bocauhoiid == selectedBoCauHoiId.Value);
            }
            ViewData["CauHoiId"] = new SelectList(await cauhoisForFilterDropdown.OrderBy(c => c.Noidung).ToListAsync(), "Id", "Noidung", selectedCauHoiId);


            // Tạo ViewModel để truyền dữ liệu và thông tin phân trang sang View
            var viewModel = new QuestionListViewModel
            {
                Questions = questionViewModels, // Giờ là danh sách Câu hỏi
                PageNumber = page,
                PageSize = pageSize,
                TotalPages = totalPages,
                TotalItems = totalItems,
                SelectedBoCauHoiId = selectedBoCauHoiId,
                SelectedCauHoiId = selectedCauHoiId,
                SelectedDeThiId = selectedDeThiId
            };

            return View(viewModel);
        }

        // GET: Admin/Dapans/GetAnswersForQuestion (Action mới cho AJAX để lấy đáp án cho modal)
        [HttpGet]
        public async Task<IActionResult> GetAnswersForQuestion(int cauhoiId)
        {
            var cauhoi = await _context.Cauhois
                                       .Include(c => c.Dapans.OrderBy(d => d.Id)) // Lấy các đáp án và sắp xếp
                                       .FirstOrDefaultAsync(c => c.Id == cauhoiId);

            if (cauhoi == null)
            {
                return NotFound();
            }
            // Trả về PartialView chứa danh sách đáp án
            return PartialView("_AnswersForQuestionPartial", cauhoi);
        }


        // GET: Admin/Dapans/Details/5 (Giữ nguyên)
        public async Task<IActionResult> Details(int? id)
        {
            Console.WriteLine($"DEBUG: [DapansController][Details] - Vào action Details. ID: {id ?? null}");

            if (id == null)
            {
                TempData["ErrorMessage"] = "Không tìm thấy ID đáp án.";
                Console.WriteLine("DEBUG: [DapansController][Details] - ID đáp án là NULL, trả về NotFound.");
                return NotFound();
            }

            // Load Dapan và Cauhoi, Bocauhoi liên quan
            var dapan = await _context.Dapans
                                     .Include(d => d.Cauhoi)
                                         .ThenInclude(c => c.Bocauhoi)
                                     .FirstOrDefaultAsync(d => d.Id == id);

            if (dapan == null)
            {
                TempData["ErrorMessage"] = "Đáp án không tồn tại.";
                Console.WriteLine($"DEBUG: [DapansController][Details] - Đáp án với ID {id} không tồn tại.");
                return RedirectToAction(nameof(Index));
            }

            Console.WriteLine($"DEBUG: [DapansController][Details] - Đã tìm thấy đáp án ID: {dapan.Id}, Nội dung: {dapan.Noidung}");
            Console.WriteLine($"DEBUG: [DapansController][Details] - Thuộc câu hỏi ID: {dapan.Cauhoiid}, Bộ câu hỏi ID: {dapan.Cauhoi?.Bocauhoiid}");

            // Populate Bocauhoi dropdown (sử dụng cho View)
            ViewData["Bocauhoiid"] = new SelectList(await _context.Bocauhois.OrderBy(b => b.Tenbocauhoi).ToListAsync(), "Id", "Tenbocauhoi", dapan.Cauhoi?.Bocauhoiid);

            // Populate Cauhoi dropdown, filtered by selected Bocauhoiid (sử dụng cho View)
            var cauhoisForBoCauHoi = _context.Cauhois.AsQueryable();
            if (dapan.Cauhoi?.Bocauhoiid != null)
            {
                Console.WriteLine($"DEBUG: [DapansController][Details] - Lọc câu hỏi theo Bocauhoiid: {dapan.Cauhoi.Bocauhoiid}");
                cauhoisForBoCauHoi = cauhoisForBoCauHoi.Where(c => c.Bocauhoiid == dapan.Cauhoi.Bocauhoiid);
            }
            ViewData["Cauhoiid"] = new SelectList(await cauhoisForBoCauHoi.OrderBy(c => c.Noidung).ToListAsync(), "Id", "Noidung", dapan.Cauhoiid);

            return View(dapan);
        }

        // GET: Admin/Dapans/Create (Giữ nguyên)
        public async Task<IActionResult> Create(int? cauhoiid)
        {
            Console.WriteLine($"DEBUG: [DapansController][Create] - Vào action Create (GET). Cauhoiid truyền vào: {cauhoiid ?? null}");

            // Populate Bocauhoi dropdown
            ViewData["Bocauhoiid"] = new SelectList(await _context.Bocauhois.OrderBy(b => b.Tenbocauhoi).ToListAsync(), "Id", "Tenbocauhoi");

            // Logic để chọn sẵn Câu hỏi nếu có cauhoiid truyền vào, và lọc dropdown Câu hỏi theo Bộ câu hỏi của nó
            List<SelectListItem> cauhoiListItems = new List<SelectListItem>();
            int? preselectedBoCauHoiId = null;

            if (cauhoiid.HasValue)
            {
                var cauhoi = await _context.Cauhois.Include(c => c.Bocauhoi).FirstOrDefaultAsync(c => c.Id == cauhoiid.Value);
                if (cauhoi != null)
                {
                    preselectedBoCauHoiId = cauhoi.Bocauhoiid;
                    // Lọc câu hỏi theo Bộ câu hỏi của câu hỏi được truyền vào nếu có
                    cauhoiListItems = (await _context.Cauhois
                                                    .Where(c => c.Bocauhoiid == preselectedBoCauHoiId)
                                                    .OrderBy(c => c.Noidung)
                                                    .ToListAsync())
                                                    .Select(c => new SelectListItem { Value = c.Id.ToString(), Text = $"{c.Noidung?.Substring(0, Math.Min(c.Noidung.Length, 100))}..." })
                                                    .ToList();
                }
            }
            else // Nếu không có cauhoiid truyền vào, hiển thị tất cả câu hỏi ban đầu
            {
                cauhoiListItems = (await _context.Cauhois.OrderBy(c => c.Noidung).ToListAsync())
                                                    .Select(c => new SelectListItem { Value = c.Id.ToString(), Text = $"{c.Noidung?.Substring(0, Math.Min(c.Noidung.Length, 100))}..." })
                                                    .ToList();
            }

            ViewData["Cauhoiid"] = new SelectList(cauhoiListItems, "Value", "Text", cauhoiid);
            ViewBag.PreselectedBoCauHoiId = preselectedBoCauHoiId;

            return View();
        }

        // POST: Admin/Dapans/Create (Giữ nguyên)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Cauhoiid,Noidung,Dung")] Dapan dapan)
        {
            Console.WriteLine($"DEBUG: [DapansController][Create] - Vào action Create (POST). Dữ liệu nhận được: CauhoiID={dapan.Cauhoiid}, NoiDung='{dapan.Noidung}', Dung={dapan.Dung}");

            // Loại bỏ validation cho các navigation properties (nếu chúng gây lỗi)
            ModelState.Remove("Cauhoi");

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

            // Repopulate ViewDatas again on error
            // Tải lại câu hỏi để biết nó thuộc bộ câu hỏi nào nếu Cauhoiid đã được chọn trên form
            int? preselectedBoCauHoiIdOnError = null;
            if (dapan.Cauhoiid.HasValue)
            {
                var cauhoi = await _context.Cauhois.AsNoTracking().FirstOrDefaultAsync(c => c.Id == dapan.Cauhoiid.Value);
                if (cauhoi != null)
                {
                    preselectedBoCauHoiIdOnError = cauhoi.Bocauhoiid;
                }
            }

            ViewData["Bocauhoiid"] = new SelectList(await _context.Bocauhois.OrderBy(b => b.Tenbocauhoi).ToListAsync(), "Id", "Tenbocauhoi", preselectedBoCauHoiIdOnError);

            // Lọc danh sách câu hỏi cho dropdown dựa trên Bộ câu hỏi đã chọn (hoặc tất cả nếu chưa chọn)
            List<SelectListItem> cauhoiListItemsOnError = new List<SelectListItem>();
            if (preselectedBoCauHoiIdOnError.HasValue)
            {
                cauhoiListItemsOnError = (await _context.Cauhois
                                                        .Where(c => c.Bocauhoiid == preselectedBoCauHoiIdOnError.Value)
                                                        .OrderBy(c => c.Noidung)
                                                        .ToListAsync())
                                                        .Select(c => new SelectListItem { Value = c.Id.ToString(), Text = $"{c.Noidung?.Substring(0, Math.Min(c.Noidung.Length, 100))}..." })
                                                        .ToList();
            }
            else
            {
                cauhoiListItemsOnError = (await _context.Cauhois.OrderBy(c => c.Noidung).ToListAsync())
                                                    .Select(c => new SelectListItem { Value = c.Id.ToString(), Text = $"{c.Noidung?.Substring(0, Math.Min(c.Noidung.Length, 100))}..." })
                                                    .ToList();
            }
            ViewData["Cauhoiid"] = new SelectList(cauhoiListItemsOnError, "Value", "Text", dapan.Cauhoiid);
            ViewBag.PreselectedBoCauHoiId = preselectedBoCauHoiIdOnError;

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

        // GET: Admin/Dapans/Edit/5 (Giữ nguyên)
        public async Task<IActionResult> Edit(int? id)
        {
            Console.WriteLine($"DEBUG: [DapansController][Edit] - Vào action Edit (GET). ID: {id ?? null}");
            if (id == null)
            {
                TempData["ErrorMessage"] = "Không tìm thấy ID đáp án.";
                Console.WriteLine("DEBUG: [DapansController][Edit] - ID đáp án là NULL, trả về NotFound.");
                return NotFound();
            }

            // Load Dapan và Cauhoi, Bocauhoi liên quan
            var dapan = await _context.Dapans
                                         .Include(d => d.Cauhoi)
                                             .ThenInclude(c => c.Bocauhoi)
                                         .FirstOrDefaultAsync(d => d.Id == id);

            if (dapan == null)
            {
                TempData["ErrorMessage"] = "Đáp án không tồn tại.";
                Console.WriteLine($"DEBUG: [DapansController][Edit] - Đáp án với ID {id} không tồn tại.");
                return RedirectToAction(nameof(Index));
            }

            Console.WriteLine($"DEBUG: [DapansController][Edit] - Đã tìm thấy đáp án ID: {dapan.Id}, Nội dung: {dapan.Noidung}");
            Console.WriteLine($"DEBUG: [DapansController][Edit] - Thuộc câu hỏi ID: {dapan.Cauhoiid}, Bộ câu hỏi ID: {dapan.Cauhoi?.Bocauhoiid}");

            // Populate Bocauhoi dropdown
            ViewData["Bocauhoiid"] = new SelectList(await _context.Bocauhois.OrderBy(b => b.Tenbocauhoi).ToListAsync(), "Id", "Tenbocauhoi", dapan.Cauhoi?.Bocauhoiid);

            // Populate Cauhoi dropdown, filtered by selected Bocauhoiid
            List<SelectListItem> cauhoiListItems = new List<SelectListItem>();
            if (dapan.Cauhoi?.Bocauhoiid != null)
            {
                Console.WriteLine($"DEBUG: [DapansController][Edit] - Lọc câu hỏi theo Bocauhoiid: {dapan.Cauhoi.Bocauhoiid}");
                cauhoiListItems = (await _context.Cauhois
                                                .Where(c => c.Bocauhoiid == dapan.Cauhoi.Bocauhoiid)
                                                .OrderBy(c => c.Noidung)
                                                .ToListAsync())
                                                .Select(c => new SelectListItem { Value = c.Id.ToString(), Text = $"{c.Noidung?.Substring(0, Math.Min(c.Noidung.Length, 100))}..." })
                                                .ToList();
            }
            else
            {
                cauhoiListItems = (await _context.Cauhois.OrderBy(c => c.Noidung).ToListAsync())
                                                    .Select(c => new SelectListItem { Value = c.Id.ToString(), Text = $"{c.Noidung?.Substring(0, Math.Min(c.Noidung.Length, 100))}..." })
                                                    .ToList();
            }
            ViewData["Cauhoiid"] = new SelectList(cauhoiListItems, "Value", "Text", dapan.Cauhoiid);
            ViewBag.PreselectedBoCauHoiId = dapan.Cauhoi?.Bocauhoiid;


            return View(dapan);
        }

        // POST: Admin/Dapans/Edit/5 (Giữ nguyên)
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

            ModelState.Remove("Cauhoi"); // Remove to avoid validation errors on navigation property

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

                // Repopulate ViewDatas on error, ensuring related BoCauHoi/Cauhoi are loaded
                await _context.Entry(existingDapan).Reference(d => d.Cauhoi).LoadAsync(); // Load Cauhoi để truy cập Bocauhoiid

                // Load the BoCauHoi from the existing Dapan's Cauhoi for pre-selecting the dropdown
                int? preselectedBoCauHoiIdOnError = existingDapan.Cauhoi?.Bocauhoiid;
                ViewData["Bocauhoiid"] = new SelectList(await _context.Bocauhois.OrderBy(b => b.Tenbocauhoi).ToListAsync(), "Id", "Tenbocauhoi", preselectedBoCauHoiIdOnError);

                List<SelectListItem> cauhoiListItemsOnError = new List<SelectListItem>();
                if (preselectedBoCauHoiIdOnError.HasValue)
                {
                    cauhoiListItemsOnError = (await _context.Cauhois
                                                            .Where(c => c.Bocauhoiid == preselectedBoCauHoiIdOnError.Value)
                                                            .OrderBy(c => c.Noidung)
                                                            .ToListAsync())
                                                            .Select(c => new SelectListItem { Value = c.Id.ToString(), Text = $"{c.Noidung?.Substring(0, Math.Min(c.Noidung.Length, 100))}..." })
                                                            .ToList();
                }
                else
                {
                    cauhoiListItemsOnError = (await _context.Cauhois.OrderBy(c => c.Noidung).ToListAsync())
                                                        .Select(c => new SelectListItem { Value = c.Id.ToString(), Text = $"{c.Noidung?.Substring(0, Math.Min(c.Noidung.Length, 100))}..." })
                                                        .ToList();
                }
                ViewData["Cauhoiid"] = new SelectList(cauhoiListItemsOnError, "Value", "Text", dapanFromForm.Cauhoiid); // Use dapanFromForm.Cauhoiid for current selection
                ViewBag.PreselectedBoCauHoiId = preselectedBoCauHoiIdOnError;

                return View(existingDapan); // Return existingDapan to retain its properties not bound by form
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

        // GET: Admin/Dapans/Delete/5 (Giữ nguyên)
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
                    .ThenInclude(c => c.Bocauhoi)
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

        // POST: Admin/Dapans/Delete/5 (Giữ nguyên)
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

        // GET: Admin/Dapans/GetQuestionsByBoCauHoiId (Ajax endpoint) (Giữ nguyên)
        [HttpGet]
        public async Task<IActionResult> GetQuestionsByBoCauHoiId(int boCauHoiId)
        {
            var questions = await _context.Cauhois
                                          .Where(c => c.Bocauhoiid == boCauHoiId)
                                          .OrderBy(c => c.Noidung)
                                          .Select(c => new { value = c.Id, text = c.Noidung })
                                          .ToListAsync();
            return Json(questions);
        }

        // ViewModel cho từng Câu hỏi hiển thị trên trang Index
        public class QuestionViewModelForIndex
        {
            public Cauhoi Cauhoi { get; set; }
            public List<string> RelatedDeThiNames { get; set; } = new List<string>(); // Danh sách tên các Đề thi liên quan
        }

        // ViewModel cho Index action, chứa danh sách các QuestionViewModelForIndex
        public class QuestionListViewModel
        {
            public IEnumerable<QuestionViewModelForIndex> Questions { get; set; } = new List<QuestionViewModelForIndex>();
            public int PageNumber { get; set; }
            public int PageSize { get; set; }
            public int TotalPages { get; set; }
            public int TotalItems { get; set; }
            public int? SelectedBoCauHoiId { get; set; }
            public int? SelectedCauHoiId { get; set; } // Giữ lại cho bộ lọc câu hỏi
            public string SelectedDeThiId { get; set; }
        }
    }
}
