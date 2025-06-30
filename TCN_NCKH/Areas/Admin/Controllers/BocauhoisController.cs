using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering; // Cần thiết cho SelectListItem
using Microsoft.EntityFrameworkCore;
using TCN_NCKH.Models.DBModel;

namespace TCN_NCKH.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class BocauhoisController : Controller
    {
        private readonly NghienCuuKhoaHocContext _context;

        public BocauhoisController(NghienCuuKhoaHocContext context)
        {
            _context = context;
        }

        // GET: Admin/Bocauhois
        public async Task<IActionResult> Index()
        {
            Console.WriteLine("DEBUG: Vào action Index của BocauhoisController.");

            // Tải danh sách Bộ câu hỏi và bao gồm thông tin Môn học liên quan
            var bocauhois = await _context.Bocauhois
                                          .Include(b => b.Monhoc) // Eager load thông tin Môn học
                                          .OrderBy(b => b.Monhoc.Tenmon) // Sắp xếp theo tên môn học
                                          .ThenBy(b => b.Tenbocauhoi) // Sau đó sắp xếp theo tên bộ câu hỏi
                                          .ToListAsync();

            Console.WriteLine($"DEBUG: Đã tải {bocauhois.Count} bộ câu hỏi.");

            // Đảm bảo ViewBag.Monhocs được khởi tạo và gửi đến View cho các dropdown hoặc hiển thị thông tin
            ViewBag.Monhocs = await _context.Monhocs.OrderBy(m => m.Tenmon).ToListAsync();
            Console.WriteLine($"DEBUG: ViewBag.Monhocs đã được gán. Số lượng môn học: {(ViewBag.Monhocs as List<Monhoc>)?.Count ?? 0}");

            return View(bocauhois);
        }

        // GET: Admin/Bocauhois/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            Console.WriteLine($"DEBUG: Vào action Details. ID: {id}");
            if (id == null)
            {
                Console.WriteLine("DEBUG: ID chi tiết là null.");
                TempData["ErrorMessage"] = "Không tìm thấy ID bộ câu hỏi.";
                return NotFound();
            }

            var bocauhoi = await _context.Bocauhois
                .Include(b => b.Monhoc) // Eager load thông tin Môn học
                .FirstOrDefaultAsync(m => m.Id == id);

            if (bocauhoi == null)
            {
                Console.WriteLine($"DEBUG: Không tìm thấy bộ câu hỏi với ID: {id}");
                TempData["ErrorMessage"] = "Bộ câu hỏi không tồn tại.";
                return RedirectToAction(nameof(Index));
            }
            Console.WriteLine($"DEBUG: Đã tìm thấy bộ câu hỏi: {bocauhoi.Tenbocauhoi}");
            return View(bocauhoi);
        }

        // GET: Admin/Bocauhois/Create
        public IActionResult Create()
        {
            Console.WriteLine("DEBUG: Vào action Create (GET).");
            // Cải thiện SelectList cho Monhocid: Hiển thị Tenmon (tên môn học) thay vì Id
            // Truyền null cho Monhocid vì đây là trang tạo mới
            PopulateBocauhoiViewData(null, null); // Đã đổi tên PopulateDethiViewData thành PopulateBocauhoiViewData

            // Tạo SelectList cho Mucdokho (Mức độ khó)
            var mucDoKhoList = new List<SelectListItem>
            {
                new SelectListItem { Value = "0", Text = "Dễ" }, // Giá trị cần khớp với kiểu tinyint trong DB
                new SelectListItem { Value = "1", Text = "Trung bình" },
                new SelectListItem { Value = "2", Text = "Khó" }
            };
            ViewBag.MucDoKhoList = new SelectList(mucDoKhoList, "Value", "Text");
            Console.WriteLine("DEBUG: ViewBag.MucDoKhoList đã được thiết lập.");

            return View();
        }

        // POST: Admin/Bocauhois/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Tenbocauhoi,Monhocid,Mota,Mucdokho,DapanMacdinhA,DapanMacdinhB,DapanMacdinhC,DapanMacdinhD")] Bocauhoi bocauhoi)
        {
            Console.WriteLine("DEBUG: Vào action Create (POST).");
            Console.WriteLine($"DEBUG: Dữ liệu nhận từ form: Tenbocauhoi={bocauhoi.Tenbocauhoi}, Monhocid={bocauhoi.Monhocid}, Mucdokho={bocauhoi.Mucdokho}");

            // QUAN TRỌNG: XÓA LỖI VALIDATION CHO 'Monhoc' TRƯỚC KHI KIỂM TRA ModelState.IsValid
            if (ModelState.ContainsKey("Monhoc"))
            {
                ModelState.Remove("Monhoc");
                Console.WriteLine("DEBUG: Đã xóa lỗi validation cho trường 'Monhoc' khỏi ModelState.");
            }

            if (ModelState.IsValid)
            {
                Console.WriteLine("DEBUG: ModelState.IsValid là TRUE. Tiến hành lưu dữ liệu.");
                try
                {
                    _context.Add(bocauhoi);
                    Console.WriteLine("DEBUG: Đối tượng bocauhoi đã được thêm vào DbContext.");
                    await _context.SaveChangesAsync();
                    Console.WriteLine("DEBUG: Dữ liệu đã được lưu thành công vào database.");
                    TempData["SuccessMessage"] = "Bộ câu hỏi đã được tạo thành công!";
                    return RedirectToAction(nameof(Index));
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"ERROR: Lỗi khi lưu dữ liệu vào database: {ex.Message}");
                    if (ex.InnerException != null)
                    {
                        Console.WriteLine($"ERROR: Inner Exception: {ex.InnerException.Message}");
                    }
                    TempData["ErrorMessage"] = $"Đã xảy ra lỗi khi tạo bộ câu hỏi: {ex.Message}";
                }
            }
            else
            {
                Console.WriteLine("DEBUG: ModelState.IsValid là FALSE. Có lỗi validation.");
                foreach (var state in ModelState)
                {
                    if (state.Value.Errors.Count > 0)
                    {
                        Console.WriteLine($"DEBUG: Lỗi validation cho trường '{state.Key}':");
                        foreach (var error in state.Value.Errors)
                        {
                            Console.WriteLine($"DEBUG: - {error.ErrorMessage}");
                        }
                    }
                }
            }
            // Nếu ModelState không hợp lệ hoặc có lỗi khi lưu, cần khởi tạo lại SelectList và thông báo lỗi
            PopulateBocauhoiViewData(bocauhoi.Monhocid, bocauhoi.Mucdokho); // Đã đổi tên PopulateDethiViewData

            // Tạo SelectList cho Mucdokho (Mức độ khó) khi có lỗi validation để giữ lại giá trị đã chọn
            var mucDoKhoList = new List<SelectListItem>
            {
                new SelectListItem { Value = "0", Text = "Dễ" },
                new SelectListItem { Value = "1", Text = "Trung bình" },
                new SelectListItem { Value = "2", Text = "Khó" }
            };
            ViewBag.MucDoKhoList = new SelectList(mucDoKhoList, "Value", "Text", bocauhoi.Mucdokho?.ToString());
            Console.WriteLine("DEBUG: ViewBag.MucDoKhoList đã được thiết lập lại.");

            // Tổng hợp và hiển thị tất cả các lỗi validation
            string validationErrors = string.Join("<br/>", ModelState.Values
                                                            .SelectMany(v => v.Errors)
                                                            .Select(e => e.ErrorMessage));
            if (!string.IsNullOrEmpty(validationErrors))
            {
                TempData["ErrorMessage"] = (TempData["ErrorMessage"] as string ?? "") + "<br/>" + validationErrors;
                Console.WriteLine($"DEBUG: Đã thêm lỗi validation vào TempData: {validationErrors}");
            }
            else if (TempData["ErrorMessage"] == null)
            {
                TempData["ErrorMessage"] = "Thông tin bộ câu hỏi không hợp lệ. Vui lòng kiểm tra lại.";
                Console.WriteLine("DEBUG: TempData[ErrorMessage] được gán thông báo lỗi chung.");
            }
            Console.WriteLine("DEBUG: Trả về View với lỗi.");
            return View(bocauhoi);
        }

        // GET: Admin/Bocauhois/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            Console.WriteLine($"DEBUG: Vào action Edit (GET). ID: {id}");
            if (id == null)
            {
                Console.WriteLine("DEBUG: ID chỉnh sửa là null.");
                TempData["ErrorMessage"] = "Không tìm thấy ID bộ câu hỏi.";
                return NotFound();
            }

            var bocauhoi = await _context.Bocauhois.Include(b => b.Monhoc).FirstOrDefaultAsync(m => m.Id == id);
            if (bocauhoi == null)
            {
                Console.WriteLine($"DEBUG: Không tìm thấy bộ câu hỏi với ID: {id}");
                TempData["ErrorMessage"] = "Bộ câu hỏi không tồn tại.";
                return RedirectToAction(nameof(Index));
            }
            // Cải thiện SelectList cho Monhocid: Hiển thị Tenmon (tên môn học) thay vì Id
            // Sử dụng bocauhoi.Monhocid trực tiếp vì PopulateBocauhoiViewData giờ chấp nhận string?
            PopulateBocauhoiViewData(bocauhoi.Monhocid, bocauhoi.Mucdokho); // Đã đổi tên PopulateDethiViewData
            Console.WriteLine("DEBUG: ViewData[Monhocid] đã được thiết lập cho Edit.");

            // Tạo SelectList cho Mucdokho (Mức độ khó) và chọn giá trị hiện tại
            var mucDoKhoList = new List<SelectListItem>
            {
                new SelectListItem { Value = "0", Text = "Dễ" },
                new SelectListItem { Value = "1", Text = "Trung bình" },
                new SelectListItem { Value = "2", Text = "Khó" }
            };
            ViewBag.MucDoKhoList = new SelectList(mucDoKhoList, "Value", "Text", bocauhoi.Mucdokho?.ToString());
            Console.WriteLine("DEBUG: ViewBag.MucDoKhoList đã được thiết lập cho Edit.");

            Console.WriteLine($"DEBUG: Trả về View với dữ liệu bộ câu hỏi: {bocauhoi.Tenbocauhoi}");
            return View(bocauhoi);
        }

        // POST: Admin/Bocauhois/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        // Đã sửa [Bind] bằng cách loại bỏ trùng lặp DapanMacdinhC
        public async Task<IActionResult> Edit(int id, [Bind("Tenbocauhoi,Monhocid,Mota,Mucdokho,DapanMacdinhA,DapanMacdinhB,DapanMacdinhC,DapanMacdinhD")] Bocauhoi bocauhoiFromForm) // Bỏ Id khỏi bind
        {
            Console.WriteLine($"DEBUG: Vào action Edit (POST). ID: {id}");
            Console.WriteLine($"DEBUG: Dữ liệu nhận từ form: Tenbocauhoi={bocauhoiFromForm.Tenbocauhoi}, Monhocid={bocauhoiFromForm.Monhocid}, Mucdokho={bocauhoiFromForm.Mucdokho}");

            // QUAN TRỌNG: Gán lại ID từ route vào đối tượng model được bind từ form
            // Vì ID không nằm trong [Bind], nó sẽ không được tự động điền từ form.
            // Do đó, chúng ta phải gán thủ công từ tham số 'id' của route.
            bocauhoiFromForm.Id = id;
            Console.WriteLine($"DEBUG: Đã gán lại ID từ route ({id}) vào model từ form.");

            // XÓA LỖI VALIDATION CHO 'Id' VÀ 'Monhoc' TRƯỚC KHI KIỂM TRA ModelState.IsValid TRONG EDIT
            if (ModelState.ContainsKey("Id"))
            {
                ModelState.Remove("Id");
                Console.WriteLine("DEBUG: Đã xóa lỗi validation cho trường 'Id' khỏi ModelState trong Edit.");
            }
            if (ModelState.ContainsKey("Monhoc"))
            {
                ModelState.Remove("Monhoc");
                Console.WriteLine("DEBUG: Đã xóa lỗi validation cho trường 'Monhoc' khỏi ModelState trong Edit.");
            }

            if (ModelState.IsValid)
            {
                Console.WriteLine("DEBUG: ModelState.IsValid là TRUE. Tiến hành cập nhật dữ liệu.");
                try
                {
                    // Lấy lại đối tượng gốc từ database để đảm bảo Entity Framework theo dõi đúng trạng thái
                    var existingBocauhoi = await _context.Bocauhois.FindAsync(id);
                    if (existingBocauhoi == null)
                    {
                        TempData["ErrorMessage"] = "Bộ câu hỏi không tồn tại hoặc đã bị xóa.";
                        return NotFound();
                    }

                    // Cập nhật các thuộc tính từ bocauhoiFromForm vào existingBocauhoi
                    existingBocauhoi.Tenbocauhoi = bocauhoiFromForm.Tenbocauhoi;
                    existingBocauhoi.Monhocid = bocauhoiFromForm.Monhocid;
                    existingBocauhoi.Mota = bocauhoiFromForm.Mota;
                    existingBocauhoi.Mucdokho = bocauhoiFromForm.Mucdokho;
                    existingBocauhoi.DapanMacdinhA = bocauhoiFromForm.DapanMacdinhA;
                    existingBocauhoi.DapanMacdinhB = bocauhoiFromForm.DapanMacdinhB;
                    existingBocauhoi.DapanMacdinhC = bocauhoiFromForm.DapanMacdinhC;
                    existingBocauhoi.DapanMacdinhD = bocauhoiFromForm.DapanMacdinhD;

                    await _context.SaveChangesAsync();
                    Console.WriteLine("DEBUG: Dữ liệu đã được cập nhật thành công vào database.");
                    TempData["SuccessMessage"] = "Bộ câu hỏi đã được cập nhật thành công!";
                    return RedirectToAction(nameof(Index));
                }
                catch (DbUpdateConcurrencyException ex)
                {
                    Console.WriteLine($"ERROR: Lỗi đồng thời khi cập nhật: {ex.Message}");
                    if (!BocauhoiExists(bocauhoiFromForm.Id))
                    {
                        Console.WriteLine("DEBUG: Bộ câu hỏi không tồn tại khi cập nhật đồng thời.");
                        TempData["ErrorMessage"] = "Bộ câu hỏi không tồn tại hoặc đã bị xóa bởi người khác.";
                        return NotFound();
                    }
                    else
                    {
                        Console.WriteLine("DEBUG: Lỗi đồng thời khác. Ném ngoại lệ.");
                        TempData["ErrorMessage"] = "Đã xảy ra lỗi đồng thời khi cập nhật dữ liệu. Vui lòng thử lại.";
                        throw; // Ném lại ngoại lệ để ghi log chi tiết hơn
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"ERROR: Lỗi hệ thống khi cập nhật: {ex.Message}");
                    if (ex.InnerException != null)
                    {
                        Console.WriteLine($"ERROR: Inner Exception: {ex.InnerException.Message}");
                    }
                    TempData["ErrorMessage"] = $"Đã xảy ra lỗi hệ thống: {ex.Message}";
                }
                return RedirectToAction(nameof(Index));
            }
            else
            {
                Console.WriteLine("DEBUG: ModelState.IsValid là FALSE. Có lỗi validation khi cập nhật.");
                foreach (var state in ModelState)
                {
                    if (state.Value.Errors.Count > 0)
                    {
                        Console.WriteLine($"DEBUG: Lỗi validation cho trường '{state.Key}':");
                        foreach (var error in state.Value.Errors)
                        {
                            Console.WriteLine($"DEBUG: - {error.ErrorMessage}");
                        }
                    }
                }
            }
            // Nếu ModelState không hợp lệ hoặc có lỗi khi lưu, cần khởi tạo lại SelectList và thông báo lỗi
            PopulateBocauhoiViewData(bocauhoiFromForm.Monhocid, bocauhoiFromForm.Mucdokho); // Đã đổi tên PopulateDethiViewData
            Console.WriteLine("DEBUG: ViewData[Monhocid] đã được thiết lập lại cho Edit.");

            // Tạo SelectList cho Mucdokho (Mức độ khó) khi có lỗi validation để giữ lại giá trị đã chọn
            var mucDoKhoList = new List<SelectListItem>
            {
                new SelectListItem { Value = "0", Text = "Dễ" },
                new SelectListItem { Value = "1", Text = "Trung bình" },
                new SelectListItem { Value = "2", Text = "Khó" }
            };
            ViewBag.MucDoKhoList = new SelectList(mucDoKhoList, "Value", "Text", bocauhoiFromForm.Mucdokho?.ToString());
            Console.WriteLine("DEBUG: ViewBag.MucDoKhoList đã được thiết lập lại cho Edit.");

            // Tổng hợp và hiển thị tất cả các lỗi validation
            string validationErrors = string.Join("<br/>", ModelState.Values
                                                            .SelectMany(v => v.Errors)
                                                            .Select(e => e.ErrorMessage));
            if (!string.IsNullOrEmpty(validationErrors))
            {
                TempData["ErrorMessage"] = (TempData["ErrorMessage"] as string ?? "") + "<br/>" + validationErrors;
                Console.WriteLine($"DEBUG: Đã thêm lỗi validation vào TempData cho Edit: {validationErrors}");
            }
            else if (TempData["ErrorMessage"] == null)
            {
                TempData["ErrorMessage"] = "Thông tin bộ câu hỏi không hợp lệ. Vui lòng kiểm tra lại.";
                Console.WriteLine("DEBUG: TempData[ErrorMessage] được gán thông báo lỗi chung cho Edit.");
            }
            Console.WriteLine("DEBUG: Trả về View với lỗi khi cập nhật.");
            return View(bocauhoiFromForm);
        }

        // GET: Admin/Bocauhois/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            Console.WriteLine($"DEBUG: Vào action Delete (GET). ID: {id}");
            if (id == null)
            {
                Console.WriteLine("DEBUG: ID xóa là null.");
                TempData["ErrorMessage"] = "Không tìm thấy ID bộ câu hỏi.";
                return NotFound();
            }

            var bocauhoi = await _context.Bocauhois
                .Include(b => b.Monhoc) // Eager load thông tin Môn học
                .FirstOrDefaultAsync(m => m.Id == id);
            if (bocauhoi == null)
            {
                Console.WriteLine($"DEBUG: Không tìm thấy bộ câu hỏi với ID: {id}");
                TempData["ErrorMessage"] = "Bộ câu hỏi không tồn tại.";
                return RedirectToAction(nameof(Index));
            }
            Console.WriteLine($"DEBUG: Đã tìm thấy bộ câu hỏi để xóa: {bocauhoi.Tenbocauhoi}");
            return View(bocauhoi);
        }

        // POST: Admin/Bocauhois/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            Console.WriteLine($"DEBUG: Vào action DeleteConfirmed (POST). ID: {id}");
            var bocauhoi = await _context.Bocauhois.FindAsync(id);
            if (bocauhoi == null)
            {
                Console.WriteLine($"DEBUG: Bộ câu hỏi với ID {id} không tồn tại hoặc đã bị xóa.");
                TempData["ErrorMessage"] = "Bộ câu hỏi không tồn tại hoặc đã bị xóa trước đó.";
                return RedirectToAction(nameof(Index));
            }

            try
            {
                _context.Bocauhois.Remove(bocauhoi);
                Console.WriteLine($"DEBUG: Bộ câu hỏi với ID {id} đã được đánh dấu để xóa.");
                await _context.SaveChangesAsync();
                Console.WriteLine("DEBUG: Dữ liệu đã được xóa thành công từ database.");
                TempData["SuccessMessage"] = "Bộ câu hỏi đã được xóa thành công!";
            }
            catch (Exception ex)
            {
                Console.WriteLine($"ERROR: Lỗi khi xóa bộ câu hỏi: {ex.Message}");
                if (ex.InnerException != null)
                {
                    Console.WriteLine($"ERROR: Inner Exception: {ex.InnerException.Message}");
                }
                TempData["ErrorMessage"] = $"Đã xảy ra lỗi khi xóa bộ câu hỏi: {ex.Message}";
            }
            return RedirectToAction(nameof(Index));
        }

        private bool BocauhoiExists(int id)
        {
            Console.WriteLine($"DEBUG: Kiểm tra tồn tại bộ câu hỏi với ID: {id}");
            return _context.Bocauhois.Any(e => e.Id == id);
        }

        // Helper method để populate ViewData cho Monhoc và Mucdokho
        // selectedMonhocId là kiểu string? để khớp với Monhoc.Id trong DB
        private void PopulateBocauhoiViewData(string? selectedMonhocId = null, byte? selectedMucdokho = null)
        {
            Console.WriteLine("DEBUG: PopulateBocauhoiViewData được gọi.");
            ViewData["Monhocid"] = new SelectList(_context.Monhocs.OrderBy(m => m.Tenmon), "Id", "Tenmon", selectedMonhocId);

            var mucDoKhoList = new List<SelectListItem>
            {
                new SelectListItem { Value = "0", Text = "Dễ" },
                new SelectListItem { Value = "1", Text = "Trung bình" },
                new SelectListItem { Value = "2", Text = "Khó" }
            };
            ViewBag.MucDoKhoList = new SelectList(mucDoKhoList, "Value", "Text", selectedMucdokho?.ToString());
            Console.WriteLine("DEBUG: ViewData đã được PopulateBocauhoiViewData thiết lập xong.");
        }
    }
}
