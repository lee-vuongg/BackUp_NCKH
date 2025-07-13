using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims; // Required to get logged-in user information
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization; // For [Authorize] attribute
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using TCN_NCKH.Models.DBModel; // Ensure this namespace contains your DB Models (LichSuLamBai, Ketquathi, Cauhoi, TraloiSinhvien, CheatDetectionLog, etc.)
using TCN_NCKH.Models.ViewModels; // Ensure your ViewModels are imported here
using TCN_NCKH.Areas.HocSinh.Models; // Ensure your Area-specific ViewModels are imported here (e.g., ScoreAnalysisViewModel)
using TCN_NCKH.Services; // Ensure this namespace contains your services (PdfReportService, EmailService)

namespace TCN_NCKH.Areas.HocSinh.Controllers
{
    [Area("HocSinh")]
    public class LichSuLamBaisController : Controller
    {
        private readonly NghienCuuKhoaHocContext _context;
        private readonly PdfReportService _pdfReportService; // Declare PDF service
        // private readonly EmailService _emailService; // Will be used for email sending step later

        public LichSuLamBaisController(NghienCuuKhoaHocContext context, PdfReportService pdfReportService /*, EmailService emailService */)
        {
            _context = context;
            _pdfReportService = pdfReportService;
            // _emailService = emailService;
        }

        // Helper to get the MaSinhVien (Student ID) of the logged-in student
        // Assuming MaSinhVien (Sinhvienid) is stored in ClaimTypes.NameIdentifier as a string
        private string GetLoggedInStudentId()
        {
            return User.FindFirstValue(ClaimTypes.NameIdentifier);
        }

        // GET: HocSinh/LichSuLamBais
        // Displays a list of exam history for the logged-in student
        public async Task<IActionResult> Index()
        {
            var loggedInStudentId = GetLoggedInStudentId();

            if (string.IsNullOrEmpty(loggedInStudentId))
            {
                // Handle case where student ID cannot be retrieved (not logged in or error)
                return Unauthorized(); // Or RedirectToPage("/Account/Login")
            }

            // Retrieve exam history for the CURRENTLY logged-in student
            var lichSuLamBais = await _context.LichSuLamBais
                .Where(l => l.MaSinhVien == loggedInStudentId) // Filter by MaSinhVien of the logged-in student
                .Include(l => l.LichThi) // Include LichThi information
                    .ThenInclude(lt => lt.Dethi) // And include DeThi information from LichThi
                .Include(l => l.MaDeThiNavigation) // Include DeThi information directly linked to LichSuLamBai (if any)
                .Include(l => l.MaSinhVienNavigation) // Include NguoiDung (User) information
                    .ThenInclude(nd => nd.SinhvienNavigation) // Include SinhVien information from NguoiDung
                .Select(l => new LichSuLamBaiDetailsViewModel // Map to ViewModel for display
                {
                    Id = l.Id,
                    ThoiDiemBatDau = l.ThoiDiemBatDau,
                    ThoiDiemKetThuc = l.ThoiDiemKetThuc,
                    SoLanRoiManHinh = l.SoLanRoiManHinh,
                    TrangThaiNopBai = l.TrangThaiNopBai,
                    Ipaddress = l.Ipaddress,
                    TongDiemDatDuoc = l.TongDiemDatDuoc,
                    DaChamDiem = l.DaChamDiem,

                    // Handle CS0072 error: Check for null before calling ToString()
                    ThongTinLichThi = l.LichThi != null && l.LichThi.Dethi != null
                                             ? $"{l.LichThi.Dethi.Tendethi} ({(l.LichThi.ThoigianBatdau.HasValue ? l.LichThi.ThoigianBatdau.Value.ToString("dd/MM/yyyy HH:mm") : "N/A")})"
                                             : "Không rõ lịch thi",
                    // Handle CS1061 (if any): Ensure the property name is TenDeThi
                    TenDeThi = l.MaDeThiNavigation != null ? l.MaDeThiNavigation.Tendethi : "Không rõ đề thi",
                    // Handle CS1061: Assuming property name is Hoten in Sinhvien model
                    TenSinhVien = l.MaSinhVienNavigation != null ? l.MaSinhVienNavigation.SinhvienNavigation.Hoten : "Không rõ sinh viên"
                })
                .OrderByDescending(l => l.ThoiDiemBatDau) // Sort by most recent time
                .ToListAsync();

            return View(lichSuLamBais);
        }

        // GET: HocSinh/LichSuLamBais/Details/5
        // Displays details of a specific exam history, ensuring the student has permission to view it
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var loggedInStudentId = GetLoggedInStudentId();

            if (string.IsNullOrEmpty(loggedInStudentId))
            {
                return Unauthorized();
            }

            // Retrieve exam history details by ID AND for the CURRENTLY logged-in student
            var lichSuLamBai = await _context.LichSuLamBais
                .Where(m => m.Id == id && m.MaSinhVien == loggedInStudentId) // Ensure only own record is viewed
                .Include(l => l.LichThi)
                    .ThenInclude(lt => lt.Dethi)
                .Include(l => l.MaDeThiNavigation)
                .Include(l => l.MaSinhVienNavigation)
                    .ThenInclude(nd => nd.SinhvienNavigation) // Include SinhVien information from NguoiDung
                .Select(l => new LichSuLamBaiDetailsViewModel // Map to ViewModel
                {
                    Id = l.Id,
                    ThoiDiemBatDau = l.ThoiDiemBatDau,
                    ThoiDiemKetThuc = l.ThoiDiemKetThuc,
                    SoLanRoiManHinh = l.SoLanRoiManHinh,
                    TrangThaiNopBai = l.TrangThaiNopBai,
                    Ipaddress = l.Ipaddress,
                    TongDiemDatDuoc = l.TongDiemDatDuoc,
                    DaChamDiem = l.DaChamDiem,
                    ThongTinLichThi = l.LichThi != null && l.LichThi.Dethi != null
                                             ? $"{l.LichThi.Dethi.Tendethi} ({(l.LichThi.ThoigianBatdau.HasValue ? l.LichThi.ThoigianBatdau.Value.ToString("dd/MM/yyyy HH:mm") : "N/A")})"
                                             : "Không rõ lịch thi",
                    TenDeThi = l.MaDeThiNavigation != null ? l.MaDeThiNavigation.Tendethi : "Không rõ đề thi",
                    TenSinhVien = l.MaSinhVienNavigation != null ? l.MaSinhVienNavigation.SinhvienNavigation.Hoten : "Không rõ sinh viên"
                })
                .FirstOrDefaultAsync();

            if (lichSuLamBai == null)
            {
                // If not found or ID does not belong to the logged-in student
                return NotFound();
            }

            return View(lichSuLamBai);
        }

        /// <summary>
        /// Displays score analysis for the logged-in student.
        /// Uses LichSuLamBai.TongDiemDatDuoc for calculations.
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> ScoreAnalysis()
        {
            var loggedInStudentId = GetLoggedInStudentId();

            if (string.IsNullOrEmpty(loggedInStudentId))
            {
                return Unauthorized();
            }

            // Retrieve all SUBMITTED and GRADED exam histories with scores for this student
            var studentSubmittedExams = await _context.LichSuLamBais
                   .Include(l => l.LichThi)
                       .ThenInclude(lt => lt.Dethi)
                   .Include(l => l.MaDeThiNavigation)
                   .Where(l => l.MaSinhVien == loggedInStudentId &&
                               (l.TrangThaiNopBai != null && l.TrangThaiNopBai.ToLower() == "true") &&
                               (l.DaChamDiem.HasValue && l.DaChamDiem.Value == true) &&
                               l.TongDiemDatDuoc.HasValue)
                   .OrderByDescending(l => l.ThoiDiemBatDau)
                   .ToListAsync();

            // Get student name for display (from NguoiDung or SinhVien)
            var studentName = await _context.Nguoidungs
                .Where(nd => nd.Id.Trim() == loggedInStudentId)
                .Select(nd => nd.Hoten) // Assuming Nguoidung has navigation to SinhVien and SinhVien has Hoten
                .FirstOrDefaultAsync() ?? loggedInStudentId; // If name not found, use ID

            // --- ĐOẠN CẬP NHẬT LẠI ĐÂY ---
            var viewModel = new ScoreAnalysisViewModel
            {
                StudentName = studentName,
                TotalExams = studentSubmittedExams.Count, // Tổng số bài thi đã nộp và chấm
                AverageScore = studentSubmittedExams.Any() ? studentSubmittedExams.Average(l => l.TongDiemDatDuoc ?? 0) : 0,
                PassedExams = studentSubmittedExams.Count(l => l.TongDiemDatDuoc >= 5), // Giả định điểm đậu là 5
                // SỬA LỖI CS0029: Lấy chỉ các giá trị điểm (double?) từ 10 bài gần nhất
                RecentScores = studentSubmittedExams.Take(10).Select(l => l.TongDiemDatDuoc).ToList(),
                // Gán cho TotalCompletedExams
                TotalCompletedExams = studentSubmittedExams.Count,
                // Gán ExamResults cho bảng chi tiết
                ExamResults = studentSubmittedExams.Select(l => new LichSuLamBaiDetailsViewModel
                {
                    Id = l.Id,
                    ThoiDiemBatDau = l.ThoiDiemBatDau,
                    ThoiDiemKetThuc = l.ThoiDiemKetThuc,
                    SoLanRoiManHinh = l.SoLanRoiManHinh,
                    TrangThaiNopBai = l.TrangThaiNopBai,
                    Ipaddress = l.Ipaddress,
                    TongDiemDatDuoc = l.TongDiemDatDuoc,
                    DaChamDiem = l.DaChamDiem,
                    TenDeThi = l.LichThi?.Dethi?.Tendethi ?? l.MaDeThiNavigation?.Tendethi ?? "Không rõ đề thi",
                    TenSinhVien = l.MaSinhVienNavigation?.SinhvienNavigation?.Hoten ?? "Không rõ sinh viên"
                   
                }).ToList(),
                // HighestScore và LowestScore cũng cần được gán từ studentSubmittedExams
                HighestScore = studentSubmittedExams.Any() ? studentSubmittedExams.Max(l => l.TongDiemDatDuoc ?? 0) : 0,
                LowestScore = studentSubmittedExams.Any() ? studentSubmittedExams.Min(l => l.TongDiemDatDuoc ?? 0) : 0
            };

            return View(viewModel);
        }

        /// <summary>
        /// Downloads a PDF file of the exam result for a specific exam history.
        /// </summary>
        /// <param name="id">ID of the LichSuLamBai (Exam History).</param>
        [HttpGet]
        public async Task<IActionResult> DownloadExamResultPdf(int id)
        {
            var loggedInStudentId = GetLoggedInStudentId();
            if (string.IsNullOrEmpty(loggedInStudentId))
            {
                return Unauthorized();
            }

            // Retrieve LichSuLamBai and related information, ensuring it belongs to the logged-in student
            var lichSuLamBai = await _context.LichSuLamBais
                .Include(l => l.LichThi)
                    .ThenInclude(lt => lt.Dethi)
                        .ThenInclude(dt => dt.Monhoc) // Assuming DeThi has MonHoc
                .Include(l => l.MaDeThiNavigation) // This is DeThi if it's directly linked to LichSuLamBai
                    .ThenInclude(dt => dt.Monhoc) // Assuming MaDeThiNavigation has MonHoc
                .Include(l => l.MaSinhVienNavigation) // Navigation to NguoiDung
                    .ThenInclude(nd => nd.SinhvienNavigation) // Navigation from NguoiDung to SinhVien to get name, email
                .Where(l => l.Id == id && l.MaSinhVien == loggedInStudentId)
                .FirstOrDefaultAsync();

            if (lichSuLamBai == null)
            {
                return NotFound("Không tìm thấy lịch sử làm bài hoặc mi không có quyền truy cập.");
            }

            // Retrieve the Ketquathi (Exam Result) associated with this LichSuLamBai
            // Assuming Ketquathi has Lichthiid and Sinhvienid matching LichSuLamBai
            var ketQuaThi = await _context.Ketquathis
                .FirstOrDefaultAsync(kq => kq.Lichthiid == lichSuLamBai.LichThiId && kq.Sinhvienid.Trim() == loggedInStudentId);

            if (ketQuaThi == null)
            {
                return NotFound("Không tìm thấy kết quả thi liên quan.");
            }

            // Retrieve student answers from TraloiSinhvien, linked via KetquathiId
            var traLoiSinhVien = await _context.TraloiSinhviens
                .Where(tl => tl.KetquathiId == ketQuaThi.Id) // Use KetquathiId to filter
                .ToListAsync();

            // Retrieve the actual questions for the exam
            List<Cauhoi> cauHois = new List<Cauhoi>();
            var deThiIdForQuestions = lichSuLamBai.MaDeThiNavigation?.Id ?? lichSuLamBai.LichThi?.Dethi?.Id;

            if (!string.IsNullOrEmpty(deThiIdForQuestions))
            {
                var deThi = await _context.Dethis
                    .Include(dt => dt.Cauhois) // Include direct questions if there's a direct relationship
                        .ThenInclude(ch => ch.Dapans) // Include answers for each question
                    .Include(dt => dt.Bocauhoi) // Include question set if the exam uses a question set
                        .ThenInclude(bc => bc.Cauhois)
                            .ThenInclude(ch => ch.Dapans)
                    .FirstOrDefaultAsync(dt => dt.Id == deThiIdForQuestions);

                if (deThi != null)
                {
                    if (deThi.Bocauhoiid.HasValue) // If the exam uses a question set
                    {
                        cauHois = await _context.Cauhois
                            .Include(c => c.Dapans)
                            .Where(c => c.Bocauhoiid == deThi.Bocauhoiid.Value)
                            .ToListAsync();
                    }
                    else if (deThi.Cauhois != null && deThi.Cauhois.Any()) // If questions are directly linked to the exam
                    {
                        cauHois = deThi.Cauhois.ToList();
                    }
                    else
                    {
                        // Special case: If Cauhoi has a Dethiid (string) that is not a navigation property
                        cauHois = await _context.Cauhois
                            .Include(c => c.Dapans)
                            .Where(c => c.Dethiid == deThi.Id)
                            .ToListAsync();
                    }
                }
            }

            // Generate the PDF
            // Pass LichSuLamBai, Ketquathi, traLoiSinhVien, and cauHois to the service
            var pdfBytes = _pdfReportService.GenerateExamResultPdf(lichSuLamBai, ketQuaThi, traLoiSinhVien, cauHois);

            // Return the PDF file
            var deThiTen = lichSuLamBai.MaDeThiNavigation?.Tendethi ?? lichSuLamBai.LichThi?.Dethi?.Tendethi ?? "Khong_ro_de_thi";
            var sinhVienTen = lichSuLamBai.MaSinhVienNavigation?.SinhvienNavigation?.Hoten ?? lichSuLamBai.MaSinhVien ?? "Khong_ro_sinh_vien";
            var fileName = $"KetQuaThi_{sinhVienTen}_{deThiTen}.pdf";

            return File(pdfBytes, "application/pdf", fileName);
        }

        // DTO to receive data from Python client for webcam detection logs
        // You can place this class at the end of the Controller file or in a separate DTOs folder
        public class WebcamDetectionLogDto
        {
            public int LichSuLamBaiId { get; set; }
            public string DetectionType { get; set; } // E.g., "HandGesture", "LookAway", "Stranger"
            public string Details { get; set; } // E.g., "Detected gesture: Fist", "Detected looking away for X seconds"
            // public string ImageData { get; set; } // Keep if you intend to send images from Python
        }

        /// <summary>
        /// Action method to receive and log webcam detection events (e.g., from a Python script).
        /// Will use the CheatDetectionLog model to save to DB.
        /// </summary>
        /// <param name="logDto">Log data received from the client.</param>
        /// <returns>HTTP status code Ok on success, BadRequest or StatusCode(500) on error.</returns>
        [HttpPost]
        // [ValidateAntiForgeryToken] // VERY IMPORTANT: Please read the comment below
        // You need to consider Anti-Forgery Token.
        // If your Python script cannot send this token, you will either need to:
        // 1. Temporarily remove [ValidateAntiForgeryToken] for this action (NOT RECOMMENDED IN PRODUCTION).
        // 2. Or implement another authentication mechanism (API Key, JWT token) for the Python client.
        public async Task<IActionResult> LogWebcamDetection([FromBody] WebcamDetectionLogDto logDto)
        {
            // Get the UserId of the logged-in student
            var loggedInUserId = User.FindFirstValue(ClaimTypes.NameIdentifier)?.Trim(); // Use ClaimTypes.NameIdentifier for UserId

            if (string.IsNullOrWhiteSpace(loggedInUserId))
            {
                Console.WriteLine("[SERVER DEBUG] User is not logged in or UserId not found for webcam detection log.");
                return Unauthorized(new { message = "Người dùng chưa đăng nhập hoặc không xác định." });
            }

            // Log received data from Python client
            Console.WriteLine($"[SERVER DEBUG] Received webcam detection log for UserId={loggedInUserId}: LichSuLamBaiId={logDto.LichSuLamBaiId}, DetectionType={logDto.DetectionType}, Details={logDto.Details}");

            // Check if LichSuLamBaiId is valid
            if (logDto.LichSuLamBaiId == 0)
            {
                Console.WriteLine("[SERVER DEBUG] LichSuLamBaiId is 0 for webcam detection, returning BadRequest.");
                return BadRequest(new { message = "LichSuLamBaiId không hợp lệ cho log phát hiện webcam." });
            }

            try
            {
                // Optional: Check if LichSuLamBai exists.
                // This ensures the log is linked to an existing exam history.
                var lichSuLamBai = await _context.LichSuLamBais.FindAsync(logDto.LichSuLamBaiId);
                if (lichSuLamBai == null)
                {
                    Console.WriteLine($"[SERVER DEBUG] LichSuLamBai with ID {logDto.LichSuLamBaiId} not found for webcam detection log. This log might be orphaned.");
                    // For cheat logs, sometimes you might still want to log even if the ID is invalid, depending on your business logic.
                    // return NotFound(new { message = "Lịch sử làm bài không tìm thấy cho log phát hiện webcam." });
                }

                // CREATE A NEW CheatDetectionLog RECORD
                var cheatLog = new CheatDetectionLog
                {
                    UserId = loggedInUserId, // Assign the UserId of the logged-in user
                    LichSuLamBaiId = logDto.LichSuLamBaiId,
                    DetectionType = logDto.DetectionType,
                    Details = logDto.Details,
                    Timestamp = DateTime.Now // Timestamp when the log is received on the server
                };

                // Ensure your DbContext has DbSet<CheatDetectionLog>
                // E.g.: public DbSet<CheatDetectionLog> CheatDetectionLogs { get; set; } in NghienCuuKhoaHocContext
                _context.CheatDetectionLogs.Add(cheatLog); // USE THIS NEW DbSet
                await _context.SaveChangesAsync(); // Save changes to the database

                Console.WriteLine("[SERVER DEBUG] CheatDetectionLog saved successfully to DB.");
                return Ok(new { message = "Log phát hiện gian lận webcam đã được ghi nhận." });
            }
            catch (Exception ex)
            {
                // Log errors if any issues occur during processing or DB save
                Console.WriteLine($"[SERVER ERROR] Exception in LogWebcamDetection: {ex.Message}");
                Console.WriteLine($"[SERVER ERROR] Stack Trace: {ex.StackTrace}");
                if (ex.InnerException != null)
                {
                    Console.WriteLine($"[SERVER ERROR] Inner Exception: {ex.InnerException.Message}");
                }
                return StatusCode(500, new { message = "Lỗi server khi ghi log phát hiện webcam.", error = ex.Message });
            }
        }
    }
}
