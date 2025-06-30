using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using TCN_NCKH.Models.DBModel;
using TCN_NCKH.Models.ViewModels; // Thêm dòng này để sử dụng ViewModel mới
using Microsoft.Data.SqlClient; // Thêm để bắt lỗi SQL Server cụ thể

namespace TCN_NCKH.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class LichthiSinhviensController : Controller
    {
        private readonly NghienCuuKhoaHocContext _context;

        public LichthiSinhviensController(NghienCuuKhoaHocContext context)
        {
            _context = context;
        }

        // GET: Admin/LichthiSinhviens
        // Cập nhật để hỗ trợ bộ lọc theo LichthiId và trả về ViewModel mới
        public async Task<IActionResult> Index(int? selectedLichthiId) // <-- ĐÃ CẬP NHẬT THAM SỐ
        {
            Console.WriteLine($"DEBUG: Vào action Index của LichthiSinhviensController. SelectedLichthiId: {selectedLichthiId ?? null}");

            // Khởi tạo truy vấn cơ sở cho LichthiSinhvien
            IQueryable<LichthiSinhvien> lichthiSinhviensQuery = _context.LichthiSinhviens
                                                                       .Include(ls => ls.Lichthi)
                                                                           .ThenInclude(lt => lt.Dethi) // Load Dethi của Lichthi
                                                                       .Include(ls => ls.Lichthi)
                                                                           .ThenInclude(lt => lt.Lophoc) // Load Lophoc của Lichthi
                                                                       .Include(ls => ls.Sinhvien)
                                                                           .ThenInclude(sv => sv.SinhvienNavigation); // Load Nguoidung của Sinhvien để lấy Hoten

            // Áp dụng bộ lọc nếu có selectedLichthiId
            if (selectedLichthiId.HasValue && selectedLichthiId.Value > 0)
            {
                lichthiSinhviensQuery = lichthiSinhviensQuery.Where(ls => ls.Lichthiid == selectedLichthiId.Value);
                Console.WriteLine($"DEBUG: Áp dụng bộ lọc theo LichthiId: {selectedLichthiId.Value}");
            }

            // Sắp xếp kết quả
            var filteredAssignments = await lichthiSinhviensQuery
                                                                .OrderBy(ls => ls.Lichthi.Ngaythi) // Sắp xếp theo ngày thi
                                                                .ThenBy(ls => ls.Sinhvien.Msv) // Sau đó theo mã sinh viên
                                                                .ToListAsync();
            Console.WriteLine($"DEBUG: Đã tải {filteredAssignments.Count} bản ghi LichthiSinhvien (sau lọc).");

            // Lấy tất cả các lịch thi để populate dropdown cho bộ lọc
            var allLichthis = await _context.Lichthis
                                            .Include(lt => lt.Dethi)
                                            .Include(lt => lt.Lophoc)
                                            .OrderByDescending(lt => lt.Ngaythi) // Sắp xếp để lịch thi mới hiện lên đầu
                                            .ToListAsync();

            // Tạo SelectList cho bộ lọc lịch thi
            var lichthiSelectListItems = allLichthis.Select(lt => new
            {
                Id = lt.Id,
                DisplayText = $"{lt.Dethi?.Tendethi} - {lt.Lophoc?.Tenlop} ({lt.Ngaythi.ToString("dd/MM/yyyy HH:mm") ?? "N/A"})"
            }).ToList();

            // Thêm tùy chọn "--- Tất cả lịch thi ---" vào đầu SelectList
            var listItems = new List<SelectListItem>();
            listItems.Add(new SelectListItem { Value = "", Text = "--- Tất cả lịch thi ---", Selected = !selectedLichthiId.HasValue || selectedLichthiId.Value == 0 });
            listItems.AddRange(lichthiSelectListItems.Select(item => new SelectListItem { Value = item.Id.ToString(), Text = item.DisplayText, Selected = item.Id == selectedLichthiId }));

            var finalSelectList = new SelectList(listItems, "Value", "Text", selectedLichthiId);


            var viewModel = new LichthiSinhvienIndexViewModel // <-- TRẢ VỀ VIEWMODEL MỚI
            {
                Assignments = filteredAssignments,
                AvailableLichthis = finalSelectList, // Sử dụng SelectList đã tạo
                SelectedLichthiId = selectedLichthiId
            };
            Console.WriteLine("DEBUG: ViewModel cho Index đã được chuẩn bị.");
            return View(viewModel); // <-- ĐÃ CẬP NHẬT: TRẢ VỀ VIEWMODEL MỚI
        }

        // GET: Admin/LichthiSinhviens/Details/5 (Giả định ID là ID của mối quan hệ LichthiSinhvien, nếu có)
        // Nếu đây là khóa chính ghép, bạn cần truyền cả Lichthiid và Sinhvienid
        public async Task<IActionResult> Details(int? lichthiId, string sinhvienId)
        {
            Console.WriteLine($"DEBUG: Vào action Details. LichthiId: {lichthiId ?? null}, SinhvienId: {sinhvienId ?? "null"}");
            if (lichthiId == null || string.IsNullOrEmpty(sinhvienId))
            {
                Console.WriteLine("DEBUG: Thông tin chi tiết không đầy đủ (thiếu ID lịch thi hoặc sinh viên).");
                TempData["ErrorMessage"] = "Thông tin chi tiết không đầy đủ (thiếu ID lịch thi hoặc sinh viên).";
                return NotFound();
            }

            var lichthiSinhvien = await _context.LichthiSinhviens
                .Include(l => l.Lichthi)
                    .ThenInclude(lt => lt.Dethi)
                .Include(l => l.Lichthi)
                    .ThenInclude(lt => lt.Lophoc)
                .Include(l => l.Sinhvien)
                    .ThenInclude(sv => sv.SinhvienNavigation)
                .FirstOrDefaultAsync(m => m.Lichthiid == lichthiId && m.Sinhvienid == sinhvienId); // Tìm kiếm theo khóa chính ghép

            if (lichthiSinhvien == null)
            {
                Console.WriteLine($"DEBUG: Không tìm thấy thông tin lịch thi của sinh viên với LichthiId={lichthiId}, SinhvienId={sinhvienId}.");
                TempData["ErrorMessage"] = "Không tìm thấy thông tin lịch thi của sinh viên này.";
                return RedirectToAction(nameof(Index));
            }
            Console.WriteLine($"DEBUG: Đã tìm thấy LichthiSinhvien: LichthiId={lichthiSinhvien.Lichthiid}, SinhvienId={lichthiSinhvien.Sinhvienid}, Được phép thi={lichthiSinhvien.Duocphepthi}.");
            return View(lichthiSinhvien);
        }

        // GET: Admin/LichthiSinhviens/Create
        // Sửa đổi để nhận LichthiId từ tham số (ví dụ: từ Lichthis/Index)
        public async Task<IActionResult> Create(int? lichthiId)
        {
            Console.WriteLine($"DEBUG: Vào action Create (GET). LichthiId nhận được: {lichthiId ?? null}.");

            if (lichthiId == null)
            {
                Console.WriteLine("DEBUG: Không có LichthiId được cung cấp. Chuyển hướng về trang Index của Lịch thi.");
                TempData["ErrorMessage"] = "Vui lòng chọn một Lịch thi để phân công sinh viên.";
                return RedirectToAction("Index", "Lichthis"); // Chuyển hướng về controller Lichthis
            }

            var lichthi = await _context.Lichthis
                                        .Include(lt => lt.Dethi)
                                        .Include(lt => lt.Lophoc)
                                        .FirstOrDefaultAsync(lt => lt.Id == lichthiId);

            if (lichthi == null)
            {
                Console.WriteLine($"DEBUG: Không tìm thấy Lịch thi với ID: {lichthiId}.");
                TempData["ErrorMessage"] = $"Không tìm thấy lịch thi với ID '{lichthiId}'.";
                return RedirectToAction("Index", "Lichthis");
            }

            // Lấy tất cả sinh viên trong lớp của lịch thi này
            var studentsInClass = await _context.Sinhviens
                                                .Include(s => s.SinhvienNavigation)
                                                .Where(s => s.Lophocid == lichthi.Lophocid) // Lọc theo Lophocid của Lichthi
                                                .OrderBy(s => s.Msv)
                                                .ToListAsync();
            Console.WriteLine($"DEBUG: Đã tìm thấy {studentsInClass.Count} sinh viên trong lớp '{lichthi.Lophoc?.Tenlop}'.");

            // Kiểm tra xem sinh viên nào đã được phân công cho lịch thi này rồi
            var assignedStudents = await _context.LichthiSinhviens
                                                 .Where(ls => ls.Lichthiid == lichthiId)
                                                 .Select(ls => ls.Sinhvienid)
                                                 .ToListAsync();
            Console.WriteLine($"DEBUG: Đã tìm thấy {assignedStudents.Count} sinh viên đã được phân công cho lịch thi này.");

            var viewModel = new AssignStudentsToLichthiViewModel
            {
                LichthiId = lichthi.Id,
                LichthiDisplayText = $"{lichthi.Dethi?.Tendethi} - {lichthi.Lophoc?.Tenlop} ({lichthi.Ngaythi.ToString("dd/MM/yyyy HH:mm") ?? "N/A"})",
                LophocId = lichthi.Lophocid,
                LophocTenlop = lichthi.Lophoc?.Tenlop,
                StudentsInClass = studentsInClass.Select(s => new SinhvienForAssignment
                {
                    Sinhvienid = s.Sinhvienid,
                    Msv = s.Msv,
                    Hoten = s.SinhvienNavigation?.Hoten ?? "(Không rõ tên)",
                    IsAssigned = assignedStudents.Contains(s.Sinhvienid), // Đánh dấu nếu đã được phân công
                    IsSelected = assignedStudents.Contains(s.Sinhvienid) // Mặc định chọn những SV đã được phân công
                }).ToList()
            };
            Console.WriteLine("DEBUG: ViewModel cho Create (GET) đã được chuẩn bị.");
            return View(viewModel);
        }


        // POST: Admin/LichthiSinhviens/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        // Nhận ViewModel chứa LichthiId và danh sách SelectedSinhvienIds
        public async Task<IActionResult> Create(AssignStudentsToLichthiViewModel model)
        {
            Console.WriteLine("DEBUG: Vào action Create (POST) với ViewModel.");

            // QUAN TRỌNG: Kiểm tra model có null không ngay từ đầu
            if (model == null)
            {
                Console.WriteLine("ERROR: AssignStudentsToLichthiViewModel model nhận được trong POST là NULL.");
                TempData["ErrorMessage"] = "Dữ liệu gửi lên không hợp lệ. Vui lòng thử lại.";
                return RedirectToAction("Index", "Lichthis");
            }

            Console.WriteLine($"DEBUG: LichthiId nhận được: {model.LichthiId}. Số lượng sinh viên được chọn: {model.SelectedSinhvienIds?.Count ?? 0}.");

            // Lấy lại thông tin lịch thi để hiển thị trên View nếu có lỗi
            var lichthi = await _context.Lichthis
                                        .Include(lt => lt.Dethi)
                                        .Include(lt => lt.Lophoc)
                                        .FirstOrDefaultAsync(lt => lt.Id == model.LichthiId);

            if (lichthi == null)
            {
                Console.WriteLine($"DEBUG: Không tìm thấy Lịch thi với ID: {model.LichthiId}. Không thể xử lý phân công.");
                TempData["ErrorMessage"] = $"Lịch thi với ID '{model.LichthiId}' không tồn tại. Vui lòng thử lại.";
                return RedirectToAction("Index", "Lichthis");
            }
            model.LichthiDisplayText = $"{lichthi.Dethi?.Tendethi} - {lichthi.Lophoc?.Tenlop} ({lichthi.Ngaythi.ToString("dd/MM/yyyy HH:mm") ?? "N/A"})";
            model.LophocId = lichthi.Lophocid;
            model.LophocTenlop = lichthi.Lophoc?.Tenlop;

            // Lấy lại danh sách sinh viên trong lớp để hiển thị lại form nếu có lỗi
            var studentsInClass = await _context.Sinhviens
                                                .Include(s => s.SinhvienNavigation)
                                                .Where(s => s.Lophocid == lichthi.Lophocid)
                                                .OrderBy(s => s.Msv)
                                                .ToListAsync();

            var existingAssignedStudents = await _context.LichthiSinhviens
                                                         .Where(ls => ls.Lichthiid == model.LichthiId)
                                                         .ToListAsync();

            // Xóa tất cả lỗi validation cho ViewModel vì chúng ta đang xử lý thủ công
            ModelState.Clear(); // Xóa tất cả lỗi validation để bắt đầu lại

            if (model.SelectedSinhvienIds == null || !model.SelectedSinhvienIds.Any())
            {
                ModelState.AddModelError(string.Empty, "Vui lòng chọn ít nhất một sinh viên để phân công.");
                Console.WriteLine("DEBUG: Lỗi: Không có sinh viên nào được chọn.");
            }

            if (ModelState.IsValid)
            {
                Console.WriteLine("DEBUG: ModelState.IsValid là TRUE (sau khi xử lý thủ công). Bắt đầu xử lý phân công.");
                int addedCount = 0;
                int skippedCount = 0;

                foreach (var sinhvienId in model.SelectedSinhvienIds ?? new List<string>())
                {
                    // Kiểm tra xem sinh viên đã được phân công chưa
                    var isAlreadyAssigned = existingAssignedStudents.Any(ls => ls.Lichthiid == model.LichthiId && ls.Sinhvienid == sinhvienId);

                    if (isAlreadyAssigned)
                    {
                        Console.WriteLine($"DEBUG: Sinh viên '{sinhvienId}' đã được phân công cho lịch thi này. Bỏ qua.");
                        skippedCount++;
                        continue;
                    }

                    var newAssignment = new LichthiSinhvien
                    {
                        Lichthiid = model.LichthiId,
                        Sinhvienid = sinhvienId,
                        Duocphepthi = model.DefaultDuocphepthi // Mặc định là true khi được chọn
                    };
                    _context.Add(newAssignment);
                    addedCount++;
                    Console.WriteLine($"DEBUG: Đã thêm phân công cho Sinhvienid: {sinhvienId}.");
                }

                try
                {
                    await _context.SaveChangesAsync();
                    Console.WriteLine($"DEBUG: Đã lưu {addedCount} phân công mới vào database.");
                    TempData["SuccessMessage"] = $"Đã phân công thành công {addedCount} sinh viên mới. ({skippedCount} sinh viên đã được bỏ qua vì đã tồn tại).";
                    return RedirectToAction(nameof(Index));
                }
                catch (DbUpdateException ex)
                {
                    Console.WriteLine($"ERROR: Lỗi DbUpdateException khi lưu phân công: {ex.Message}");
                    Console.WriteLine($"ERROR: Inner Exception: {ex.InnerException?.Message ?? "N/A"}.");
                    TempData["ErrorMessage"] = $"Đã xảy ra lỗi database khi phân công: {ex.Message}";
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"ERROR: Lỗi chung khi lưu phân công: {ex.Message}");
                    Console.WriteLine($"ERROR: Inner Exception: {ex.InnerException?.Message ?? "N/A"}.");
                    TempData["ErrorMessage"] = $"Đã xảy ra lỗi hệ thống: {ex.Message}";
                }
            }

            // Nếu có lỗi, cần chuẩn bị lại ViewModel để hiển thị trên View
            // Đảm bảo model.StudentsInClass được khởi tạo trước khi truy cập
            if (model.StudentsInClass == null)
            {
                model.StudentsInClass = new List<SinhvienForAssignment>(); // Khởi tạo nếu chưa có
            }
            model.StudentsInClass = studentsInClass.Select(s => new SinhvienForAssignment
            {
                Sinhvienid = s.Sinhvienid,
                Msv = s.Msv,
                Hoten = s.SinhvienNavigation?.Hoten ?? "(Không rõ tên)",
                IsAssigned = existingAssignedStudents.Any(ls => ls.Sinhvienid == s.Sinhvienid),
                IsSelected = model.SelectedSinhvienIds?.Contains(s.Sinhvienid) ?? false // Giữ lại trạng thái đã chọn của checkbox
            }).ToList();
            Console.WriteLine("DEBUG: Trả về View Create với lỗi và dữ liệu ViewModel đã được populate lại.");
            return View(model);
        }

        // GET: Admin/LichthiSinhviens/Edit/5 (Nếu Id là khóa chính tự tăng của LichthiSinhvien)
        // Nếu khóa chính là ghép (Lichthiid, Sinhvienid), cần truyền cả 2 ID
        public async Task<IActionResult> Edit(int? lichthiId, string sinhvienId)
        {
            Console.WriteLine($"DEBUG: Vào action Edit (GET). LichthiId: {lichthiId ?? null}, SinhvienId: {sinhvienId ?? "null"}.");
            if (lichthiId == null || string.IsNullOrEmpty(sinhvienId))
            {
                Console.WriteLine("DEBUG: Thông tin chỉnh sửa không đầy đủ (thiếu ID lịch thi hoặc sinh viên).");
                TempData["ErrorMessage"] = "Không tìm thấy ID lịch thi hoặc sinh viên.";
                return NotFound();
            }

            // Tìm kiếm đối tượng LichthiSinhvien bằng khóa chính ghép
            var lichthiSinhvien = await _context.LichthiSinhviens
                                                .Include(ls => ls.Lichthi) // Bao gồm Lichthi để hiển thị thông tin
                                                    .ThenInclude(lt => lt.Dethi)
                                                .Include(ls => ls.Lichthi)
                                                    .ThenInclude(lt => lt.Lophoc)
                                                .Include(ls => ls.Sinhvien) // Bao gồm Sinhvien để hiển thị thông tin
                                                    .ThenInclude(sv => sv.SinhvienNavigation)
                                                .FirstOrDefaultAsync(ls => ls.Lichthiid == lichthiId && ls.Sinhvienid == sinhvienId);

            if (lichthiSinhvien == null)
            {
                Console.WriteLine($"DEBUG: Không tìm thấy phân công lịch thi với LichthiId={lichthiId}, SinhvienId={sinhvienId}.");
                TempData["ErrorMessage"] = "Phân công lịch thi này không tồn tại.";
                return RedirectToAction(nameof(Index));
            }
            Console.WriteLine($"DEBUG: Đã tìm thấy LichthiSinhvien để chỉnh sửa: LichthiId={lichthiSinhvien.Lichthiid}, SinhvienId={lichthiSinhvien.Sinhvienid}.");

            // Populate ViewData cho Edit (nếu vẫn cần SelectList, nhưng trong trường hợp này có thể không cần)
            // Ví dụ: Nếu bạn chỉ chỉnh sửa Duocphepthi thì không cần SelectList
            // Nếu bạn vẫn muốn hiển thị dropdown lịch thi/sinh viên (nhưng là readonly)
            await PopulateLichthiAndSinhvienDataForDropdowns(lichthiSinhvien.Lichthiid, lichthiSinhvien.Sinhvienid);
            Console.WriteLine("DEBUG: ViewData đã được thiết lập cho Edit (GET).");

            return View(lichthiSinhvien);
        }

        // POST: Admin/LichthiSinhviens/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        // BIND CHỈ CÁC THUỘC TÍNH CÓ THỂ CHỈNH SỬA ĐƯỢC, KHÓA CHÍNH KHÔNG NÊN BIND TRỰC TIẾP
        public async Task<IActionResult> Edit(int lichthiId, string sinhvienId, [Bind("Duocphepthi")] LichthiSinhvien lichthiSinhvienFromForm) // Đổi tên tham số để rõ ràng hơn
        {
            Console.WriteLine($"DEBUG: Vào action Edit (POST). LichthiId từ route: {lichthiId}, SinhvienId từ route: {sinhvienId}.");
            Console.WriteLine($"DEBUG: Dữ liệu nhận từ form: Duocphepthi={lichthiSinhvienFromForm.Duocphepthi}.");

            // QUAN TRỌNG: Kiểm tra model nhận được có null không
            if (lichthiSinhvienFromForm == null)
            {
                Console.WriteLine("ERROR: LichthiSinhvien lichthiSinhvienFromForm nhận được trong POST là NULL.");
                TempData["ErrorMessage"] = "Dữ liệu gửi lên không hợp lệ khi chỉnh sửa. Vui lòng thử lại.";
                return NotFound(); // Hoặc chuyển hướng đến Index
            }

            // Gán lại các giá trị khóa chính từ route vào đối tượng được bind từ form
            lichthiSinhvienFromForm.Lichthiid = lichthiId;
            lichthiSinhvienFromForm.Sinhvienid = sinhvienId;
            Console.WriteLine($"DEBUG: Đã gán lại Lichthiid={lichthiSinhvienFromForm.Lichthiid} và Sinhvienid={lichthiSinhvienFromForm.Sinhvienid} vào model từ form.");

            // Lấy đối tượng gốc từ database để đảm bảo chỉ sửa các trường cho phép
            Console.WriteLine($"DEBUG: Đang tìm đối tượng LichthiSinhvien gốc với LichthiId={lichthiId} và SinhvienId={sinhvienId}...");
            var originalLichthiSinhvien = await _context.LichthiSinhviens
                                                        .FirstOrDefaultAsync(ls => ls.Lichthiid == lichthiId && ls.Sinhvienid == sinhvienId);

            if (originalLichthiSinhvien == null)
            {
                Console.WriteLine("DEBUG: Original LichthiSinhvien không tồn tại. Có thể đã bị xóa.");
                TempData["ErrorMessage"] = "Phân công lịch thi này không tồn tại hoặc đã bị xóa.";
                return NotFound();
            }
            Console.WriteLine($"DEBUG: Đã tìm thấy Original LichthiSinhvien. Trạng thái 'Duocphepthi' hiện tại: {originalLichthiSinhvien.Duocphepthi}.");


            // XÓA LỖI VALIDATION CHO CÁC THUỘC TÍNH ĐIỀU HƯỚNG NẾU CHÚNG NON-NULLABLE VÀ KHÔNG ĐƯỢC BIND TỪ FORM
            if (ModelState.ContainsKey("Lichthi"))
            {
                ModelState.Remove("Lichthi");
                Console.WriteLine("DEBUG: Đã xóa lỗi validation cho trường 'Lichthi' khỏi ModelState trong Edit.");
            }
            if (ModelState.ContainsKey("Sinhvien"))
            {
                ModelState.Remove("Sinhvien");
                Console.WriteLine("DEBUG: Đã xóa lỗi validation cho trường 'Sinhvien' khỏi ModelState trong Edit.");
            }
            // MỚI THÊM: Xóa lỗi validation cho các trường khóa chính nếu có
            if (ModelState.ContainsKey("Lichthiid"))
            {
                ModelState.Remove("Lichthiid");
                Console.WriteLine("DEBUG: Đã xóa lỗi validation cho trường 'Lichthiid' khỏi ModelState trong Edit.");
            }
            if (ModelState.ContainsKey("Sinhvienid"))
            {
                ModelState.Remove("Sinhvienid");
                Console.WriteLine("DEBUG: Đã xóa lỗi validation cho trường 'Sinhvienid' khỏi ModelState trong Edit.");
            }


            Console.WriteLine($"DEBUG: Kiểm tra ModelState.IsValid. Hiện tại: {ModelState.IsValid}.");
            if (ModelState.IsValid)
            {
                Console.WriteLine("DEBUG: ModelState.IsValid là TRUE. Tiến hành cập nhật dữ liệu.");
                try
                {
                    // Cập nhật chỉ các thuộc tính được phép từ form vào đối tượng gốc đã được theo dõi
                    originalLichthiSinhvien.Duocphepthi = lichthiSinhvienFromForm.Duocphepthi;
                    Console.WriteLine($"DEBUG: Đã cập nhật thuộc tính 'Duocphepthi' của originalLichthiSinhvien thành {originalLichthiSinhvien.Duocphepthi}.");

                    Console.WriteLine("DEBUG: Đang cố gắng lưu thay đổi vào database...");
                    await _context.SaveChangesAsync();
                    Console.WriteLine("DEBUG: Dữ liệu đã được cập nhật thành công vào database.");
                    TempData["SuccessMessage"] = "Phân công lịch thi đã được cập nhật thành công!";
                    return RedirectToAction(nameof(Index));
                }
                catch (DbUpdateConcurrencyException ex)
                {
                    Console.WriteLine($"ERROR: Lỗi đồng thời khi cập nhật: {ex.Message}");
                    if (!LichthiSinhvienExists(originalLichthiSinhvien.Lichthiid, originalLichthiSinhvien.Sinhvienid))
                    {
                        Console.WriteLine("DEBUG: Phân công lịch thi không tồn tại khi cập nhật đồng thời.");
                        TempData["ErrorMessage"] = "Phân công lịch thi không tồn tại hoặc đã bị xóa bởi người khác.";
                        return NotFound();
                    }
                    else
                    {
                        Console.WriteLine("DEBUG: Lỗi đồng thời khác. Ném ngoại lệ.");
                        TempData["ErrorMessage"] = "Đã xảy ra lỗi đồng thời khi cập nhật dữ liệu. Vui lòng thử lại.";
                        throw;
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"ERROR: Lỗi hệ thống khi cập nhật: {ex.Message}");
                    Console.WriteLine($"ERROR: Inner Exception: {ex.InnerException?.Message ?? "N/A"}.");
                    TempData["ErrorMessage"] = $"Đã xảy ra lỗi hệ thống: {ex.Message}";
                }
                return RedirectToAction(nameof(Index));
            }
            else
            {
                Console.WriteLine("DEBUG: ModelState.IsValid là FALSE. Có lỗi validation khi cập nhật.");
                var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList();
                if (errors.Any())
                {
                    Console.WriteLine("DEBUG: Các lỗi validation:");
                    foreach (var error in errors)
                    {
                        Console.WriteLine($"DEBUG: - {error}");
                    }
                    TempData["ErrorMessage"] = "Thông tin phân công không hợp lệ:<br/>" + string.Join("<br/>", errors);
                }
                else
                {
                    TempData["ErrorMessage"] = "Thông tin phân công không hợp lệ. Vui lòng kiểm tra lại.";
                }
            }

            // Nếu ModelState không hợp lệ hoặc có lỗi khi lưu, cần populate lại ViewData để giữ lại giá trị đã chọn
            Console.WriteLine("DEBUG: Đang populate lại ViewData cho View Edit (POST) do lỗi...");
            await PopulateLichthiAndSinhvienDataForDropdowns(lichthiSinhvienFromForm.Lichthiid, lichthiSinhvienFromForm.Sinhvienid);
            Console.WriteLine("DEBUG: ViewData đã được thiết lập lại cho Edit (POST) với lỗi.");

            return View(lichthiSinhvienFromForm);
        }

        // GET: Admin/LichthiSinhviens/Delete/5 (Giả định ID là ID của mối quan hệ LichthiSinhvien, nếu có)
        // Nếu khóa chính là ghép (Lichthiid, Sinhvienid), cần truyền cả 2 ID
        public async Task<IActionResult> Delete(int? lichthiId, string sinhvienId)
        {
            Console.WriteLine($"DEBUG: Vào action Delete (GET). LichthiId: {lichthiId ?? null}, SinhvienId: {sinhvienId ?? "null"}.");
            if (lichthiId == null || string.IsNullOrEmpty(sinhvienId))
            {
                Console.WriteLine("DEBUG: Thông tin xóa không đầy đủ (thiếu ID lịch thi hoặc sinh viên).");
                TempData["ErrorMessage"] = "Không tìm thấy ID lịch thi hoặc sinh viên.";
                return NotFound();
            }

            var lichthiSinhvien = await _context.LichthiSinhviens
                .Include(l => l.Lichthi)
                    .ThenInclude(lt => lt.Dethi)
                .Include(l => l.Lichthi)
                    .ThenInclude(lt => lt.Lophoc)
                .Include(l => l.Sinhvien)
                    .ThenInclude(sv => sv.SinhvienNavigation)
                .FirstOrDefaultAsync(m => m.Lichthiid == lichthiId && m.Sinhvienid == sinhvienId); // Tìm kiếm theo khóa chính ghép

            if (lichthiSinhvien == null)
            {
                Console.WriteLine($"DEBUG: Không tìm thấy phân công lịch thi với LichthiId={lichthiId}, SinhvienId={sinhvienId}.");
                TempData["ErrorMessage"] = "Phân công lịch thi này không tồn tại.";
                return RedirectToAction(nameof(Index));
            }
            Console.WriteLine($"DEBUG: Đã tìm thấy LichthiSinhvien để xóa: LichthiId={lichthiSinhvien.Lichthiid}, SinhvienId={lichthiSinhvien.Sinhvienid}.");
            return View(lichthiSinhvien);
        }

        // POST: Admin/LichthiSinhviens/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int lichthiId, string sinhvienId) // Nhận khóa chính ghép
        {
            Console.WriteLine($"DEBUG: Vào action DeleteConfirmed (POST). LichthiId: {lichthiId}, SinhvienId: {sinhvienId}.");
            var lichthiSinhvien = await _context.LichthiSinhviens.FirstOrDefaultAsync(ls => ls.Lichthiid == lichthiId && ls.Sinhvienid == sinhvienId);

            if (lichthiSinhvien == null)
            {
                Console.WriteLine($"DEBUG: Phân công lịch thi với LichthiId={lichthiId}, SinhvienId={sinhvienId} không tồn tại hoặc đã bị xóa.");
                TempData["ErrorMessage"] = "Phân công lịch thi không tồn tại hoặc đã bị xóa trước đó.";
                return RedirectToAction(nameof(Index));
            }

            try
            {
                _context.LichthiSinhviens.Remove(lichthiSinhvien);
                Console.WriteLine($"DEBUG: Phân công lịch thi với LichthiId={lichthiId}, SinhvienId={sinhvienId} đã được đánh dấu để xóa.");
                await _context.SaveChangesAsync();
                Console.WriteLine("DEBUG: Dữ liệu đã được xóa thành công từ database.");
                TempData["SuccessMessage"] = "Phân công lịch thi đã được xóa thành công!";
            }
            catch (Exception ex)
            {
                Console.WriteLine($"ERROR: Lỗi khi xóa phân công: {ex.Message}");
                Console.WriteLine($"ERROR: Inner Exception: {ex.InnerException?.Message ?? "N/A"}.");
                TempData["ErrorMessage"] = $"Đã xảy ra lỗi khi xóa phân công: {ex.Message}";
                // Xem xét xử lý chi tiết hơn nếu có các ràng buộc khóa ngoại
            }
            return RedirectToAction(nameof(Index));
        }

        // Helper method để populate ViewData cho các dropdown (dùng cho Edit hoặc trong trường hợp tạo ban đầu chưa có LichthiId)
        private async Task PopulateLichthiAndSinhvienDataForDropdowns(int? selectedLichthiId = null, string? selectedSinhvienId = null)
        {
            Console.WriteLine("DEBUG: PopulateLichthiAndSinhvienDataForDropdowns được gọi.");
            // Lichthis SelectList
            var lichthis = await _context.Lichthis
                                           .Include(lt => lt.Dethi)
                                           .Include(lt => lt.Lophoc)
                                           .OrderByDescending(lt => lt.Ngaythi) // Sắp xếp để lịch thi mới hiện lên đầu
                                           .ToListAsync();
            var lichthiItems = lichthis.Select(lt => new
            {
                Id = lt.Id,
                DisplayText = $"{lt.Dethi?.Tendethi} - {lt.Lophoc?.Tenlop} ({lt.Ngaythi.ToString("dd/MM/yyyy HH:mm") ?? "N/A"})"
            }).ToList();
            ViewData["Lichthiid"] = new SelectList(lichthiItems, "Id", "DisplayText", selectedLichthiId);
            Console.WriteLine($"DEBUG: ViewData[Lichthiid] đã được thiết lập với {lichthiItems.Count} mục.");

            // Sinhviens SelectList (tất cả sinh viên, không lọc theo lớp) - cho Edit hoặc tạo ban đầu
            var sinhviens = await _context.Sinhviens
                                            .Include(s => s.SinhvienNavigation)
                                            .OrderBy(s => s.Msv)
                                            .ToListAsync();
            var sinhvienItems = sinhviens.Select(s => new
            {
                Sinhvienid = s.Sinhvienid,
                DisplayText = $"{s.Msv} - {s.SinhvienNavigation?.Hoten ?? "(Không rõ tên)"}"
            }).ToList();
            ViewData["Sinhvienid"] = new SelectList(sinhvienItems, "Sinhvienid", "DisplayText", selectedSinhvienId);
            Console.WriteLine($"DEBUG: ViewData[Sinhvienid] đã được thiết lập với {sinhvienItems.Count} mục.");
            Console.WriteLine("DEBUG: PopulateLichthiAndSinhvienDataForDropdowns hoàn tất.");
        }

        // Hàm kiểm tra tồn tại cho khóa chính ghép
        private bool LichthiSinhvienExists(int lichthiId, string sinhvienId)
        {
            Console.WriteLine($"DEBUG: Kiểm tra tồn tại LichthiSinhvien với LichthiId={lichthiId}, SinhvienId={sinhvienId}.");
            return _context.LichthiSinhviens.Any(e => e.Lichthiid == lichthiId && e.Sinhvienid == sinhvienId);
        }
    }
}
