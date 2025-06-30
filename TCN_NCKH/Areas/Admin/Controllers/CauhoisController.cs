using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using TCN_NCKH.Models.DBModel;
using TCN_NCKH.Models.ViewModels; // Thêm để sử dụng CauhoiViewModel và DapanViewModel
using Microsoft.Data.SqlClient; // Thêm để bắt lỗi SQL Server cụ thể

namespace TCN_NCKH.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class CauhoisController : Controller
    {
        private readonly NghienCuuKhoaHocContext _context;

        public CauhoisController(NghienCuuKhoaHocContext context)
        {
            _context = context;
        }

        // GET: Admin/Cauhois
        // Hiển thị danh sách câu hỏi cho một bộ câu hỏi cụ thể
        public async Task<IActionResult> Index(int? bocauhoiId)
        {
            Console.WriteLine($"DEBUG: [CauhoisController][Index] - Vào action Index. BocauhoiId: {bocauhoiId ?? null}");

            // Kiểm tra nếu không có bocauhoiId được truyền vào
            if (bocauhoiId == null)
            {
                TempData["ErrorMessage"] = "Không tìm thấy bộ câu hỏi để hiển thị câu hỏi.";
                Console.WriteLine("DEBUG: [CauhoisController][Index] - bocauhoiId là NULL, chuyển hướng về Bocauhois/Index.");
                return RedirectToAction("Index", "Bocauhois"); // Chuyển hướng về trang danh sách bộ câu hỏi
            }

            // Tìm bộ câu hỏi dựa trên bocauhoiId
            var bocauhoi = await _context.Bocauhois.FindAsync(bocauhoiId);
            if (bocauhoi == null)
            {
                TempData["ErrorMessage"] = "Bộ câu hỏi không tồn tại.";
                Console.WriteLine($"DEBUG: [CauhoisController][Index] - Bộ câu hỏi với ID {bocauhoiId} không tồn tại, chuyển hướng về Bocauhois/Index.");
                return RedirectToAction("Index", "Bocauhois"); // Chuyển hướng nếu bộ câu hỏi không tìm thấy
            }

            ViewBag.Bocauhoi = bocauhoi; // Truyền đối tượng bộ câu hỏi sang View để hiển thị thông tin
            Console.WriteLine($"DEBUG: [CauhoisController][Index] - Đã tìm thấy Bộ câu hỏi: ID={bocauhoi.Id}, Tên='{bocauhoi.Tenbocauhoi}'");

            // Lấy danh sách câu hỏi thuộc bộ câu hỏi này, bao gồm cả thông tin bộ câu hỏi và đáp án
            var cauhois = await _context.Cauhois
                                        .Include(c => c.Bocauhoi) // Đảm bảo load thông tin Bocauhoi
                                        .Include(c => c.Dapans.OrderBy(d => d.Id)) // Bao gồm đáp án và sắp xếp theo ID
                                        .Where(c => c.Bocauhoiid == bocauhoiId) // Lọc theo BocauhoiId
                                        .OrderBy(c => c.Id) // Sắp xếp câu hỏi theo ID
                                        .ToListAsync();

            Console.WriteLine($"DEBUG: [CauhoisController][Index] - Đã tải {cauhois.Count} câu hỏi cho bộ câu hỏi ID {bocauhoiId}.");
            foreach (var cauhoi in cauhois)
            {
                Console.WriteLine($"  - Câu hỏi ID: {cauhoi.Id}, Nội dung: {cauhoi.Noidung?.Substring(0, Math.Min(cauhoi.Noidung.Length, 50))}...");
                Console.WriteLine($"    - Số đáp án: {cauhoi.Dapans?.Count ?? 0}, Đáp án đúng: {cauhoi.DapandungKeys ?? "N/A"}");
            }
            return View(cauhois);
        }

        // GET: Admin/Cauhois/Details/5
        // Hiển thị chi tiết một câu hỏi
        public async Task<IActionResult> Details(int? id)
        {
            Console.WriteLine($"DEBUG: [CauhoisController][Details] - Vào action Details. ID: {id ?? null}");
            if (id == null)
            {
                TempData["ErrorMessage"] = "Không tìm thấy ID câu hỏi.";
                Console.WriteLine("DEBUG: [CauhoisController][Details] - ID câu hỏi là NULL, trả về NotFound.");
                return NotFound();
            }

            // Tìm câu hỏi theo ID, bao gồm thông tin liên quan
            var cauhoi = await _context.Cauhois
                .Include(c => c.Bocauhoi)
                .Include(c => c.Dethi) // Bao gồm Dethi nếu cần (kiểm tra xem Dethi có còn được dùng không)
                .Include(c => c.Dapans.OrderBy(d => d.Id))
                .FirstOrDefaultAsync(m => m.Id == id);

            if (cauhoi == null)
            {
                TempData["ErrorMessage"] = "Câu hỏi không tồn tại.";
                Console.WriteLine($"DEBUG: [CauhoisController][Details] - Câu hỏi với ID {id} không tồn tại.");
                return RedirectToAction("Index", "Bocauhois"); // Chuyển hướng nếu câu hỏi không tìm thấy
            }

            ViewBag.BocauhoiId = cauhoi.Bocauhoiid; // Truyền BocauhoiId để nút "Quay lại" đúng hướng
            Console.WriteLine($"DEBUG: [CauhoisController][Details] - Đã tìm thấy câu hỏi: ID={cauhoi.Id}, Nội dung='{cauhoi.Noidung?.Substring(0, Math.Min(cauhoi.Noidung.Length, 50))}...', Thuộc BocauhoiID={cauhoi.Bocauhoiid}");
            return View(cauhoi);
        }

        // GET: Admin/Cauhois/Create
        // Hiển thị form tạo câu hỏi mới
        public async Task<IActionResult> Create(int? bocauhoiId)
        {
            Console.WriteLine($"DEBUG: [CauhoisController][Create] - Vào action Create (GET). BocauhoiId: {bocauhoiId ?? null}");

            if (bocauhoiId == null)
            {
                TempData["ErrorMessage"] = "Không xác định được bộ câu hỏi để tạo câu hỏi mới.";
                Console.WriteLine("DEBUG: [CauhoisController][Create] - bocauhoiId là NULL, chuyển hướng về Bocauhois/Index.");
                return RedirectToAction("Index", "Bocauhois");
            }

            var bocauhoi = await _context.Bocauhois.FindAsync(bocauhoiId);
            if (bocauhoi == null)
            {
                TempData["ErrorMessage"] = "Bộ câu hỏi không tồn tại.";
                Console.WriteLine($"DEBUG: [CauhoisController][Create] - Bộ câu hỏi với ID {bocauhoiId} không tồn tại.");
                return RedirectToAction("Index", "Bocauhois");
            }
            Console.WriteLine($"DEBUG: [CauhoisController][Create] - Đã tìm thấy Bộ câu hỏi: ID={bocauhoi.Id}, Tên='{bocauhoi.Tenbocauhoi}'");

            // Khởi tạo ViewModel với dữ liệu mặc định
            var viewModel = new CauhoiViewModel
            {
                Bocauhoiid = bocauhoiId,
                Tenbocauhoi = bocauhoi.Tenbocauhoi,
                Dapans = new List<DapanViewModel> // Khởi tạo 4 đáp án mặc định
                {
                    new DapanViewModel { Noidung = bocauhoi.DapanMacdinhA ?? "", Dung = false },
                    new DapanViewModel { Noidung = bocauhoi.DapanMacdinhB ?? "", Dung = false },
                    new DapanViewModel { Noidung = bocauhoi.DapanMacdinhC ?? "", Dung = false },
                    new DapanViewModel { Noidung = bocauhoi.DapanMacdinhD ?? "", Dung = false }
                },
                Diem = 1.0 // Mặc định điểm khi tạo mới để tránh lỗi validation "số dương" nếu [Required]
            };
            Console.WriteLine($"DEBUG: [CauhoisController][Create] - ViewModel được tạo với Bocauhoiid: {viewModel.Bocauhoiid}, Tenbocauhoi: '{viewModel.Tenbocauhoi}'");
            return View(viewModel);
        }

        // POST: Admin/Cauhois/Create
        // Xử lý tạo câu hỏi mới sau khi submit form
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Noidung,Diem,Bocauhoiid,Dapans")] CauhoiViewModel viewModel)
        {
            Console.WriteLine("DEBUG: [CauhoisController][Create] - Vào action Create (POST).");
            Console.WriteLine($"DEBUG: Dữ liệu nhận từ form: Noidung='{viewModel.Noidung}', Diem={viewModel.Diem}, Bocauhoiid={viewModel.Bocauhoiid}");
            if (viewModel.Dapans != null)
            {
                for (int i = 0; i < viewModel.Dapans.Count; i++)
                {
                    Console.WriteLine($"DEBUG:   - Đáp án {i + 1}: Nội dung='{viewModel.Dapans[i].Noidung}', Đúng={viewModel.Dapans[i].Dung}");
                }
            }


            // Kiểm tra ít nhất một đáp án đúng
            if (viewModel.Dapans.All(d => !d.Dung))
            {
                ModelState.AddModelError("Dapans", "Phải có ít nhất một đáp án đúng.");
                TempData["ErrorMessage"] = (TempData["ErrorMessage"] as string ?? "") + "<br/>Phải có ít nhất một đáp án đúng.";
                Console.WriteLine("DEBUG: [CauhoisController][Create] - Lỗi: Không có đáp án đúng nào được chọn.");
            }

            // Xóa lỗi validation cho các thuộc tính điều hướng (navigation properties) nếu có,
            // để tránh lỗi ModelState không cần thiết.
            // Dethi đã bị xóa khỏi Cauhoi, nên chỉ cần kiểm tra Bocauhoi.
            if (ModelState.ContainsKey("Bocauhoi")) ModelState.Remove("Bocauhoi");
            // if (ModelState.ContainsKey("Dethi")) ModelState.Remove("Dethi"); // Có thể bỏ dòng này nếu Dethi đã hoàn toàn bị loại bỏ khỏi Cauhoi

            if (ModelState.IsValid)
            {
                Console.WriteLine("DEBUG: [CauhoisController][Create] - ModelState.IsValid là TRUE. Tiến hành lưu dữ liệu.");
                try
                {
                    // Lấy thông tin bộ câu hỏi để kiểm tra tồn tại.
                    var bocauhoi = await _context.Bocauhois.FindAsync(viewModel.Bocauhoiid);
                    if (bocauhoi == null)
                    {
                        ModelState.AddModelError("Bocauhoiid", "Bộ câu hỏi không tồn tại.");
                        TempData["ErrorMessage"] = "Bộ câu hỏi được chọn không tồn tại.";
                        Console.WriteLine($"DEBUG: [CauhoisController][Create] - Lỗi: Bộ câu hỏi ID {viewModel.Bocauhoiid} không tồn tại.");
                        return View(viewModel);
                    }

                    // Tạo đối tượng Cauhoi từ ViewModel
                    var cauhoi = new Cauhoi
                    {
                        Noidung = viewModel.Noidung,
                        Diem = viewModel.Diem,
                        Bocauhoiid = viewModel.Bocauhoiid,
                        Loaicauhoi = 1 // Giả định là Trắc nghiệm (dựa trên dữ liệu trước đó của mi)
                    };

                    _context.Add(cauhoi);
                    Console.WriteLine($"DEBUG: [CauhoisController][Create] - Đang thêm câu hỏi vào context. Noidung: '{cauhoi.Noidung}', Bocauhoiid: {cauhoi.Bocauhoiid}");
                    await _context.SaveChangesAsync(); // Lưu để có ID câu hỏi mới tạo
                    Console.WriteLine($"DEBUG: [CauhoisController][Create] - Đã lưu câu hỏi ID: {cauhoi.Id} thành công.");

                    // Xử lý và lưu các đáp án
                    string correctKeys = "";
                    char optionChar = 'A';
                    foreach (var dapanViewModel in viewModel.Dapans)
                    {
                        var dapan = new Dapan
                        {
                            Cauhoiid = cauhoi.Id, // Gán ID câu hỏi vừa tạo
                            Noidung = dapanViewModel.Noidung,
                            Dung = dapanViewModel.Dung
                        };
                        _context.Add(dapan);
                        Console.WriteLine($"DEBUG:   - Đang thêm đáp án vào context: Nội dung='{dapan.Noidung}', Đúng={dapan.Dung}, Thuộc CauhoiID={dapan.Cauhoiid}");
                        if (dapan.Dung == true)
                        {
                            correctKeys += optionChar; // Ghi nhận đáp án đúng
                        }
                        optionChar++;
                    }
                    cauhoi.DapandungKeys = correctKeys; // Cập nhật lại câu hỏi với chuỗi đáp án đúng
                    Console.WriteLine($"DEBUG: [CauhoisController][Create] - Cập nhật DapandungKeys cho câu hỏi ID {cauhoi.Id}: '{correctKeys}'");
                    await _context.SaveChangesAsync(); // Lưu đáp án và cập nhật DapandungKeys

                    TempData["SuccessMessage"] = "Câu hỏi đã được tạo thành công!";
                    Console.WriteLine("DEBUG: [CauhoisController][Create] - Câu hỏi và đáp án đã được tạo thành công.");
                    return RedirectToAction(nameof(Index), new { bocauhoiId = viewModel.Bocauhoiid }); // Chuyển hướng về danh sách câu hỏi của bộ đó
                }
                catch (DbUpdateException dbEx)
                {
                    Console.WriteLine($"ERROR: [CauhoisController][Create] - Lỗi DbUpdateException khi lưu dữ liệu vào database: {dbEx.Message}");
                    string errorMessage = "Đã xảy ra lỗi database khi tạo câu hỏi.";
                    if (dbEx.InnerException != null)
                    {
                        Console.WriteLine($"ERROR: Inner Exception: {dbEx.InnerException.Message}");
                        if (dbEx.InnerException is SqlException sqlEx)
                        {
                            errorMessage += $" Chi tiết: {sqlEx.Message}"; // Hiển thị chi tiết lỗi SQL
                        }
                    }
                    TempData["ErrorMessage"] = errorMessage;
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"ERROR: [CauhoisController][Create] - Lỗi chung khi lưu dữ liệu vào database: {ex.Message}");
                    TempData["ErrorMessage"] = $"Đã xảy ra lỗi không xác định khi tạo câu hỏi: {ex.Message}";
                }
            }
            else
            {
                Console.WriteLine("DEBUG: [CauhoisController][Create] - ModelState.IsValid là FALSE. Có lỗi validation.");
                // Ghép nối tất cả các lỗi validation để hiển thị
                string validationErrors = string.Join("<br/>", ModelState.Values
                                                            .SelectMany(v => v.Errors)
                                                            .Select(e => e.ErrorMessage));
                TempData["ErrorMessage"] = (TempData["ErrorMessage"] as string ?? "") + "<br/>" + validationErrors;
                Console.WriteLine($"DEBUG: [CauhoisController][Create] - Chi tiết lỗi validation: {validationErrors}");
            }

            // Repopulate ViewModel nếu có lỗi để hiển thị lại form
            if (viewModel.Bocauhoiid.HasValue)
            {
                var bocauhoi = await _context.Bocauhois.FindAsync(viewModel.Bocauhoiid.Value);
                if (bocauhoi != null)
                {
                    viewModel.Tenbocauhoi = bocauhoi.Tenbocauhoi;
                }
            }
            return View(viewModel);
        }

        // GET: Admin/Cauhois/Edit/5
        // Hiển thị form chỉnh sửa câu hỏi
        public async Task<IActionResult> Edit(int? id)
        {
            Console.WriteLine($"DEBUG: [CauhoisController][Edit] - Vào action Edit (GET). ID: {id ?? null}");
            if (id == null)
            {
                TempData["ErrorMessage"] = "Không tìm thấy ID câu hỏi để chỉnh sửa.";
                Console.WriteLine("DEBUG: [CauhoisController][Edit] - ID câu hỏi là NULL, trả về NotFound.");
                return NotFound();
            }

            // Tìm câu hỏi và bao gồm các đáp án cùng bộ câu hỏi liên quan
            var cauhoi = await _context.Cauhois
                .Include(c => c.Dapans.OrderBy(d => d.Id))
                .Include(c => c.Bocauhoi)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (cauhoi == null)
            {
                TempData["ErrorMessage"] = "Câu hỏi không tồn tại.";
                Console.WriteLine($"DEBUG: [CauhoisController][Edit] - Câu hỏi với ID {id} không tồn tại.");
                return RedirectToAction("Index", "Bocauhois");
            }
            Console.WriteLine($"DEBUG: [CauhoisController][Edit] - Đã tìm thấy câu hỏi: ID={cauhoi.Id}, Nội dung='{cauhoi.Noidung?.Substring(0, Math.Min(cauhoi.Noidung.Length, 50))}...', Thuộc BocauhoiID={cauhoi.Bocauhoiid}");


            // Ánh xạ từ Cauhoi Model sang CauhoiViewModel để hiển thị trên form
            var viewModel = new CauhoiViewModel
            {
                Id = cauhoi.Id,
                Noidung = cauhoi.Noidung,
                Diem = cauhoi.Diem ?? 0.0, // Gán giá trị mặc định nếu null
                Bocauhoiid = cauhoi.Bocauhoiid,
                Tenbocauhoi = cauhoi.Bocauhoi?.Tenbocauhoi, // Lấy tên bộ câu hỏi
                Dapans = cauhoi.Dapans.Select(d => new DapanViewModel
                {
                    Id = d.Id,
                    Noidung = d.Noidung,
                    Dung = d.Dung ?? false // Gán giá trị mặc định nếu null
                }).ToList()
            };

            // Đảm bảo có ít nhất 4 đáp án trong ViewModel để tránh lỗi trên View khi render form
            while (viewModel.Dapans.Count < 4)
            {
                viewModel.Dapans.Add(new DapanViewModel());
            }
            Console.WriteLine($"DEBUG: [CauhoisController][Edit] - ViewModel được tạo để chỉnh sửa. Số đáp án: {viewModel.Dapans.Count}");
            return View(viewModel);
        }

        // POST: Admin/Cauhois/Edit/5
        // Xử lý cập nhật câu hỏi sau khi submit form
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Noidung,Diem,Bocauhoiid,Dapans")] CauhoiViewModel viewModel)
        {
            Console.WriteLine($"DEBUG: [CauhoisController][Edit] - Vào action Edit (POST). ID từ route: {id}");
            Console.WriteLine($"DEBUG: Dữ liệu nhận từ form: Id={viewModel.Id}, Noidung='{viewModel.Noidung}', Diem={viewModel.Diem}, Bocauhoiid={viewModel.Bocauhoiid}");
            if (viewModel.Dapans != null)
            {
                for (int i = 0; i < viewModel.Dapans.Count; i++)
                {
                    Console.WriteLine($"DEBUG:   - Đáp án {i + 1}: ID={viewModel.Dapans[i].Id}, Nội dung='{viewModel.Dapans[i].Noidung}', Đúng={viewModel.Dapans[i].Dung}");
                }
            }

            if (id != viewModel.Id)
            {
                TempData["ErrorMessage"] = "ID câu hỏi không hợp lệ.";
                Console.WriteLine($"DEBUG: [CauhoisController][Edit] - ID không khớp: {id} != {viewModel.Id}.");
                return View(viewModel);
            }

            // Kiểm tra ít nhất một đáp án đúng
            if (viewModel.Dapans.All(d => !d.Dung))
            {
                ModelState.AddModelError("Dapans", "Phải có ít nhất một đáp án đúng.");
                TempData["ErrorMessage"] = (TempData["ErrorMessage"] as string ?? "") + "<br/>Phải có ít nhất một đáp án đúng.";
                Console.WriteLine("DEBUG: [CauhoisController][Edit] - Lỗi: Không có đáp án đúng nào được chọn.");
            }

            // Xóa lỗi validation cho các thuộc tính điều hướng nếu có
            if (ModelState.ContainsKey("Bocauhoi")) ModelState.Remove("Bocauhoi");
            // if (ModelState.ContainsKey("Dethi")) ModelState.Remove("Dethi"); // Có thể bỏ dòng này nếu Dethi đã hoàn toàn bị loại bỏ khỏi Cauhoi

            if (ModelState.IsValid)
            {
                Console.WriteLine("DEBUG: [CauhoisController][Edit] - ModelState.IsValid là TRUE. Tiến hành cập nhật dữ liệu.");
                try
                {
                    // Lấy câu hỏi hiện có và các đáp án của nó
                    var cauhoiToUpdate = await _context.Cauhois
                                                        .Include(c => c.Dapans)
                                                        .Include(c => c.Bocauhoi) // Vẫn include để truy cập Bocauhoi nếu cần (ví dụ: Tenbocauhoi)
                                                        .FirstOrDefaultAsync(c => c.Id == id);
                    if (cauhoiToUpdate == null)
                    {
                        TempData["ErrorMessage"] = "Câu hỏi không tồn tại hoặc đã bị xóa.";
                        Console.WriteLine($"DEBUG: [CauhoisController][Edit] - Câu hỏi với ID {id} không tồn tại trong DB khi cập nhật.");
                        return NotFound();
                    }

                    // Cập nhật thông tin chính của câu hỏi
                    cauhoiToUpdate.Noidung = viewModel.Noidung;
                    cauhoiToUpdate.Diem = viewModel.Diem;
                    cauhoiToUpdate.Bocauhoiid = viewModel.Bocauhoiid;
                    Console.WriteLine($"DEBUG: [CauhoisController][Edit] - Cập nhật Cauhoi: Nội dung='{cauhoiToUpdate.Noidung}', Diem={cauhoiToUpdate.Diem}, Bocauhoiid={cauhoiToUpdate.Bocauhoiid}");

                    // Cập nhật/Thêm mới đáp án
                    string correctKeys = "";
                    char optionChar = 'A';

                    // Duyệt qua các đáp án từ ViewModel
                    foreach (var dapanViewModel in viewModel.Dapans)
                    {
                        var existingDapan = cauhoiToUpdate.Dapans.FirstOrDefault(d => d.Id == dapanViewModel.Id);
                        if (existingDapan != null)
                        {
                            // Cập nhật đáp án hiện có
                            existingDapan.Noidung = dapanViewModel.Noidung;
                            existingDapan.Dung = dapanViewModel.Dung;
                            Console.WriteLine($"DEBUG:   - Cập nhật đáp án ID {existingDapan.Id}: Nội dung='{existingDapan.Noidung}', Đúng={existingDapan.Dung}");
                        }
                        else if (!string.IsNullOrWhiteSpace(dapanViewModel.Noidung)) // Thêm mới nếu có nội dung và chưa tồn tại
                        {
                            var newDapan = new Dapan
                            {
                                Cauhoiid = cauhoiToUpdate.Id,
                                Noidung = dapanViewModel.Noidung,
                                Dung = dapanViewModel.Dung
                            };
                            _context.Dapans.Add(newDapan);
                            Console.WriteLine($"DEBUG:   - Thêm mới đáp án: Nội dung='{newDapan.Noidung}', Đúng={newDapan.Dung}");
                        }

                        if (dapanViewModel.Dung == true)
                        {
                            correctKeys += optionChar; // Ghi nhận đáp án đúng
                        }
                        optionChar++;
                    }

                    // Xóa các đáp án không còn tồn tại trong ViewModel (nếu có)
                    var dapansToRemove = cauhoiToUpdate.Dapans
                                            .Where(d => !viewModel.Dapans.Any(v => v.Id == d.Id))
                                            .ToList();
                    foreach (var dapan in dapansToRemove)
                    {
                        _context.Dapans.Remove(dapan);
                        Console.WriteLine($"DEBUG:   - Xóa đáp án ID: {dapan.Id}, Nội dung: '{dapan.Noidung}'");
                    }

                    cauhoiToUpdate.DapandungKeys = correctKeys; // Cập nhật lại câu hỏi với chuỗi đáp án đúng
                    Console.WriteLine($"DEBUG: [CauhoisController][Edit] - Cập nhật DapandungKeys cho câu hỏi ID {cauhoiToUpdate.Id}: '{correctKeys}'");
                    await _context.SaveChangesAsync(); // Lưu các thay đổi vào database

                    TempData["SuccessMessage"] = "Câu hỏi đã được cập nhật thành công!";
                    Console.WriteLine("DEBUG: [CauhoisController][Edit] - Câu hỏi và đáp án đã được cập nhật thành công.");
                    return RedirectToAction(nameof(Index), new { bocauhoiId = viewModel.Bocauhoiid }); // Chuyển hướng về danh sách câu hỏi của bộ đó
                }
                catch (DbUpdateConcurrencyException)
                {
                    // Xử lý lỗi đồng thời (nếu câu hỏi bị xóa hoặc thay đổi bởi người khác)
                    Console.WriteLine($"ERROR: [CauhoisController][Edit] - Lỗi DbUpdateConcurrencyException khi cập nhật câu hỏi ID: {id}.");
                    if (!CauhoiExists(viewModel.Id))
                    {
                        TempData["ErrorMessage"] = "Câu hỏi không tồn tại hoặc đã bị xóa bởi người khác.";
                        return NotFound();
                    }
                    else
                    {
                        TempData["ErrorMessage"] = "Đã xảy ra lỗi đồng thời khi cập nhật dữ liệu. Vui lòng thử lại.";
                        throw; // Ném lại lỗi nếu không phải là lỗi không tồn tại
                    }
                }
                catch (DbUpdateException dbEx)
                {
                    Console.WriteLine($"ERROR: [CauhoisController][Edit] - Lỗi DbUpdateException khi cập nhật dữ liệu vào database: {dbEx.Message}");
                    string errorMessage = "Đã xảy ra lỗi database khi cập nhật câu hỏi.";
                    if (dbEx.InnerException != null)
                    {
                        Console.WriteLine($"ERROR: Inner Exception: {dbEx.InnerException.Message}");
                        if (dbEx.InnerException is SqlException sqlEx)
                        {
                            errorMessage += $" Chi tiết: {sqlEx.Message}";
                        }
                    }
                    TempData["ErrorMessage"] = errorMessage;
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"ERROR: [CauhoisController][Edit] - Lỗi chung khi cập nhật dữ liệu vào database: {ex.Message}");
                    TempData["ErrorMessage"] = $"Đã xảy ra lỗi không xác định khi cập nhật câu hỏi: {ex.Message}";
                }
            }
            else
            {
                Console.WriteLine("DEBUG: [CauhoisController][Edit] - ModelState.IsValid là FALSE. Có lỗi validation.");
                string validationErrors = string.Join("<br/>", ModelState.Values
                                                            .SelectMany(v => v.Errors)
                                                            .Select(e => e.ErrorMessage));
                TempData["ErrorMessage"] = (TempData["ErrorMessage"] as string ?? "") + "<br/>" + validationErrors;
                Console.WriteLine($"DEBUG: [CauhoisController][Edit] - Chi tiết lỗi validation: {validationErrors}");
            }

            // Repopulate ViewModel nếu có lỗi để hiển thị lại form
            if (viewModel.Bocauhoiid.HasValue)
            {
                var bocauhoi = await _context.Bocauhois.FindAsync(viewModel.Bocauhoiid.Value);
                if (bocauhoi != null)
                {
                    viewModel.Tenbocauhoi = bocauhoi.Tenbocauhoi;
                }
            }
            return View(viewModel);
        }

        // GET: Admin/Cauhois/Delete/5
        // Hiển thị xác nhận xóa câu hỏi
        public async Task<IActionResult> Delete(int? id)
        {
            Console.WriteLine($"DEBUG: [CauhoisController][Delete] - Vào action Delete (GET). ID: {id ?? null}");
            if (id == null)
            {
                TempData["ErrorMessage"] = "Không tìm thấy ID câu hỏi để xóa.";
                Console.WriteLine("DEBUG: [CauhoisController][Delete] - ID câu hỏi là NULL, trả về NotFound.");
                return NotFound();
            }

            // Tìm câu hỏi và bao gồm bộ câu hỏi, đáp án để hiển thị thông tin
            var cauhoi = await _context.Cauhois
                .Include(c => c.Bocauhoi)
                .Include(c => c.Dapans.OrderBy(d => d.Id))
                .FirstOrDefaultAsync(m => m.Id == id);
            if (cauhoi == null)
            {
                TempData["ErrorMessage"] = "Câu hỏi không tồn tại.";
                Console.WriteLine($"DEBUG: [CauhoisController][Delete] - Câu hỏi với ID {id} không tồn tại.");
                return RedirectToAction("Index", "Bocauhois");
            }
            Console.WriteLine($"DEBUG: [CauhoisController][Delete] - Đã tìm thấy câu hỏi: ID={cauhoi.Id}, Nội dung='{cauhoi.Noidung?.Substring(0, Math.Min(cauhoi.Noidung.Length, 50))}...', Thuộc BocauhoiID={cauhoi.Bocauhoiid}");
            ViewBag.BocauhoiId = cauhoi.Bocauhoiid; // Truyền BocauhoiId để nút "Quay lại" đúng
            return View(cauhoi);
        }

        // POST: Admin/Cauhois/Delete/5
        // Xử lý xóa câu hỏi sau khi xác nhận
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            Console.WriteLine($"DEBUG: [CauhoisController][DeleteConfirmed] - Vào action DeleteConfirmed (POST). ID: {id}");
            // Tìm câu hỏi theo ID để xóa
            var cauhoi = await _context.Cauhois.FindAsync(id);
            if (cauhoi == null)
            {
                TempData["ErrorMessage"] = "Câu hỏi không tồn tại hoặc đã bị xóa.";
                Console.WriteLine($"DEBUG: [CauhoisController][DeleteConfirmed] - Câu hỏi với ID {id} không tồn tại.");
                return RedirectToAction("Index", "Bocauhois"); // Chuyển hướng nếu không tìm thấy
            }
            var bocauhoiId = cauhoi.Bocauhoiid; // Lấy ID bộ câu hỏi trước khi xóa để có thể redirect về đúng trang
            Console.WriteLine($"DEBUG: [CauhoisController][DeleteConfirmed] - Đã tìm thấy câu hỏi để xóa: ID={cauhoi.Id}, Thuộc BocauhoiID={bocauhoiId}");

            try
            {
                _context.Cauhois.Remove(cauhoi); // Xóa câu hỏi khỏi ngữ cảnh
                Console.WriteLine($"DEBUG: [CauhoisController][DeleteConfirmed] - Đang xóa câu hỏi ID: {cauhoi.Id}.");
                await _context.SaveChangesAsync(); // Lưu thay đổi vào database
                Console.WriteLine($"DEBUG: [CauhoisController][DeleteConfirmed] - Đã xóa câu hỏi ID: {cauhoi.Id} thành công.");
                TempData["SuccessMessage"] = "Câu hỏi đã được xóa thành công!";
            }
            catch (Exception ex)
            {
                Console.WriteLine($"ERROR: [CauhoisController][DeleteConfirmed] - Lỗi khi xóa câu hỏi ID {id}: {ex.Message}");
                TempData["ErrorMessage"] = $"Đã xảy ra lỗi khi xóa câu hỏi: {ex.Message}";
            }
            // Chuyển hướng về trang Index của Cauhois, truyền theo bocauhoiId để hiển thị đúng danh sách
            return RedirectToAction(nameof(Index), new { bocauhoiId = bocauhoiId });
        }

        // Phương thức kiểm tra sự tồn tại của câu hỏi
        private bool CauhoiExists(int id)
        {
            return _context.Cauhois.Any(e => e.Id == id);
        }
    }
}
