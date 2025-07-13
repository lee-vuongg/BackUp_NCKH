using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using TCN_NCKH.Models.DBModel;
using System.ComponentModel.DataAnnotations; // Để dùng [Required] cho ViewModels

namespace TCN_NCKH.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class CheatDetectionLogsController : Controller
    {
        private readonly NghienCuuKhoaHocContext _context;

        public CheatDetectionLogsController(NghienCuuKhoaHocContext context)
        {
            _context = context;
        }

        // ================================================================
        // === CÁC ACTION CHO VIỆC HIỂN THỊ VÀ QUẢN LÝ DANH SÁCH LOG ===
        // ================================================================

        // GET: Admin/CheatDetectionLogs
        // Hiển thị danh sách các log gian lận, có hỗ trợ lọc và phân trang
        public async Task<IActionResult> Index(string? filterUserId, int page = 1, int pageSize = 10)
        {
            if (page < 1) page = 1;
            if (pageSize < 1) pageSize = 10;
            if (pageSize > 50) pageSize = 50; // Giới hạn pageSize để tránh tải quá nhiều dữ liệu

            IQueryable<CheatDetectionLog> logsQuery = _context.CheatDetectionLogs
                                                            .Include(l => l.User) // Bao gồm thông tin người dùng
                                                            .Include(l => l.LichSuLamBai) // Bao gồm thông tin lịch sử làm bài
                                                            .OrderByDescending(l => l.Timestamp); // Sắp xếp theo thời gian mới nhất

            // Áp dụng bộ lọc theo UserId nếu có
            if (!string.IsNullOrEmpty(filterUserId))
            {
                logsQuery = logsQuery.Where(l => l.UserId == filterUserId);
            }

            var totalItems = await logsQuery.CountAsync();
            var totalPages = (int)Math.Ceiling((double)totalItems / pageSize);

            var logsOnPage = await logsQuery
                                    .Skip((page - 1) * pageSize)
                                    .Take(pageSize)
                                    .ToListAsync(); // Thực hiện truy vấn DB tại đây

            // Tạo ViewModel để truyền dữ liệu sang View
            var viewModel = new CheatDetectionLogListViewModel
            {
                // Gọi constructor của ViewModel để map dữ liệu
                Logs = logsOnPage.Select(log => new CheatDetectionLogViewModel(log)).ToList(),
                PageNumber = page,
                PageSize = pageSize,
                TotalPages = totalPages,
                TotalItems = totalItems,
                FilterUserId = filterUserId // Giữ lại giá trị lọc
            };

            return View(viewModel);
        }

        // GET: Admin/CheatDetectionLogs/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var cheatDetectionLog = await _context.CheatDetectionLogs
                .Include(c => c.LichSuLamBai)
                .Include(c => c.User)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (cheatDetectionLog == null)
            {
                return NotFound();
            }

            // Tạo ViewModel bằng constructor
            var viewModel = new CheatDetectionLogViewModel(cheatDetectionLog);

            return View(viewModel);
        }

        // GET: Admin/CheatDetectionLogs/Create
        public IActionResult Create()
        {
            // Cung cấp danh sách ID và Tên cho SelectList
            ViewData["LichSuLamBaiId"] = new SelectList(_context.LichSuLamBais, "Id", "Id"); // Có thể muốn hiển thị thêm thông tin VD: "Id - MaDeThi"
            ViewData["UserId"] = new SelectList(_context.Nguoidungs, "Id", "Hoten"); // Hiển thị tên người dùng thay vì ID
            return View();
        }

        // POST: Admin/CheatDetectionLogs/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("UserId,DetectionType,Details,Timestamp,LichSuLamBaiId")] CheatDetectionLog cheatDetectionLog)
        {
            // Id được sinh tự động, Timestamp nên là DateTime.UtcNow nếu không được cung cấp từ form
            // Nếu Timestamp không được bind từ form, hãy gán giá trị mặc định ở đây hoặc trong model (hasDefaultValueSql)
            if (cheatDetectionLog.Timestamp == default(DateTime))
            {
                cheatDetectionLog.Timestamp = DateTime.UtcNow;
            }

            if (ModelState.IsValid)
            {
                _context.Add(cheatDetectionLog);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Log gian lận đã được tạo thành công!";
                return RedirectToAction(nameof(Index));
            }
            TempData["ErrorMessage"] = "Có lỗi xảy ra khi tạo log gian lận. Vui lòng kiểm tra lại dữ liệu.";
            ViewData["LichSuLamBaiId"] = new SelectList(_context.LichSuLamBais, "Id", "Id", cheatDetectionLog.LichSuLamBaiId);
            ViewData["UserId"] = new SelectList(_context.Nguoidungs, "Id", "Hoten", cheatDetectionLog.UserId);
            return View(cheatDetectionLog);
        }

        // GET: Admin/CheatDetectionLogs/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var cheatDetectionLog = await _context.CheatDetectionLogs.FindAsync(id);
            if (cheatDetectionLog == null)
            {
                return NotFound();
            }
            ViewData["LichSuLamBaiId"] = new SelectList(_context.LichSuLamBais, "Id", "Id", cheatDetectionLog.LichSuLamBaiId);
            ViewData["UserId"] = new SelectList(_context.Nguoidungs, "Id", "Hoten", cheatDetectionLog.UserId);
            return View(cheatDetectionLog);
        }

        // POST: Admin/CheatDetectionLogs/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,UserId,DetectionType,Details,Timestamp,LichSuLamBaiId")] CheatDetectionLog cheatDetectionLog)
        {
            if (id != cheatDetectionLog.Id)
            {
                return NotFound();
            }

            // Giữ lại Timestamp ban đầu nếu không được chỉnh sửa từ form
            // Hoặc có thể cập nhật nó nếu đây là yêu cầu nghiệp vụ
            var existingLog = await _context.CheatDetectionLogs.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id);
            if (existingLog != null)
            {
                if (cheatDetectionLog.Timestamp == default(DateTime)) // Nếu Timestamp không được gửi từ form
                {
                    cheatDetectionLog.Timestamp = existingLog.Timestamp; // Giữ lại giá trị cũ
                }
            }


            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(cheatDetectionLog);
                    await _context.SaveChangesAsync();
                    TempData["SuccessMessage"] = "Log gian lận đã được cập nhật thành công!";
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!CheatDetectionLogExists(cheatDetectionLog.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        TempData["ErrorMessage"] = "Lỗi đồng thời: Dữ liệu đã bị thay đổi bởi người khác.";
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            TempData["ErrorMessage"] = "Có lỗi xảy ra khi cập nhật log gian lận. Vui lòng kiểm tra lại dữ liệu.";
            ViewData["LichSuLamBaiId"] = new SelectList(_context.LichSuLamBais, "Id", "Id", cheatDetectionLog.LichSuLamBaiId);
            ViewData["UserId"] = new SelectList(_context.Nguoidungs, "Id", "Hoten", cheatDetectionLog.UserId);
            return View(cheatDetectionLog);
        }

        // GET: Admin/CheatDetectionLogs/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var cheatDetectionLog = await _context.CheatDetectionLogs
                .Include(c => c.LichSuLamBai)
                .Include(c => c.User)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (cheatDetectionLog == null)
            {
                return NotFound();
            }

            // Tạo ViewModel bằng constructor
            var viewModel = new CheatDetectionLogViewModel(cheatDetectionLog);

            return View(viewModel);
        }

        // POST: Admin/CheatDetectionLogs/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var cheatDetectionLog = await _context.CheatDetectionLogs.FindAsync(id);
            if (cheatDetectionLog != null)
            {
                _context.CheatDetectionLogs.Remove(cheatDetectionLog);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Log gian lận đã được xóa thành công!";
            }
            else
            {
                TempData["ErrorMessage"] = "Không tìm thấy log gian lận để xóa.";
            }
            return RedirectToAction(nameof(Index));
        }

        private bool CheatDetectionLogExists(int id)
        {
            return _context.CheatDetectionLogs.Any(e => e.Id == id);
        }

        // ================================================================
        // === CÁC ACTION API CHO VIỆC PHÁT HIỆN VÀ GHI LOG TỪ CLIENT ===
        // ================================================================

        // POST: Admin/CheatDetectionLogs/LogDetection
        // API endpoint để nhận dữ liệu phát hiện gian lận từ client (JavaScript)
        [HttpPost]
        public async Task<IActionResult> LogDetection([FromBody] CheatDetectionRequest request)
        {
            // Quan trọng: Trong môi trường thực tế, UserId nên được lấy từ thông tin xác thực
            // của người dùng đã đăng nhập (HttpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value)
            // thay vì gửi trực tiếp từ client để tránh giả mạo.
            // Ví dụ này vẫn lấy từ request để đơn giản hóa POC.
            if (string.IsNullOrEmpty(request.UserId))
            {
                return BadRequest("User ID là bắt buộc.");
            }
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var log = new CheatDetectionLog
            {
                UserId = request.UserId,
                DetectionType = request.DetectionType,
                Details = request.Details,
                Timestamp = DateTime.UtcNow, // Sử dụng thời gian server
                LichSuLamBaiId = request.LichSuLamBaiId
            };

            try
            {
                _context.CheatDetectionLogs.Add(log);
                await _context.SaveChangesAsync();
                return Ok(new { message = "Log gian lận đã được lưu thành công!", logId = log.Id });
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"Lỗi khi lưu log gian lận: {ex.Message}");
                return StatusCode(500, new { message = $"Lỗi server: {ex.Message}" });
            }
        }

        // GET: Admin/CheatDetectionLogs/GetDetectionLogs
        // API endpoint để lấy lịch sử phát hiện gian lận (dùng cho AJAX trên trang Index)
        [HttpGet]
        public async Task<IActionResult> GetDetectionLogs(string userId)
        {
            if (string.IsNullOrEmpty(userId))
            {
                return Ok(new List<CheatDetectionLogViewModel>());
            }

            var logs = await _context.CheatDetectionLogs
                                     .Where(l => l.UserId == userId)
                                     .Include(l => l.User)
                                     .Include(l => l.LichSuLamBai)
                                     .OrderByDescending(l => l.Timestamp)
                                     .Take(20) // Giới hạn số lượng log hiển thị trong modal/sidebar
                                     .ToListAsync(); // THỰC HIỆN TRUY VẤN DB TẠI ĐÂY TRƯỚC

            // SAU KHI DỮ LIỆU ĐÃ ĐƯỢC TẢI VỀ BỘ NHỚ, MỚI MAP SANG VIEWMODEL BẰNG CONSTRUCTOR
            var viewModels = logs.Select(log => new CheatDetectionLogViewModel(log)).ToList();

            return Ok(viewModels);
        }


        // ================================================================
        // === VIEWMODELS (Được định nghĩa bên trong Controller để tiện quản lý) ===
        // ================================================================

        // ViewModel cho dữ liệu gửi từ client khi ghi log
        public class CheatDetectionRequest
        {
            [Required]
            public string UserId { get; set; } = null!; // ID người dùng
            [Required]
            public string DetectionType { get; set; } = null!;
            public string? Details { get; set; }
            public int? LichSuLamBaiId { get; set; } // ID lịch sử làm bài (nullable)
        }

        // ViewModel cho một bản ghi log để hiển thị trên View
        public class CheatDetectionLogViewModel
        {
            public int Id { get; set; }
            public string UserId { get; set; } = null!;
            public string DetectionType { get; set; } = null!;
            public string? Details { get; set; }
            public DateTime Timestamp { get; set; }
            public int? LichSuLamBaiId { get; set; }
            public string? UserName { get; set; } // Tên người dùng liên quan
            public string? LichSuLamBaiMaDeThi { get; set; } // Mã đề thi của lịch sử làm bài
            public DateTime? LichSuLamBaiThoiDiemBatDau { get; set; } // Thời điểm bắt đầu của lịch sử làm bài

            // CONSTRUCTOR NÀY LÀ MẤU CHỐT ĐỂ KHẮC PHỤC LỖI CS8072
            public CheatDetectionLogViewModel(CheatDetectionLog log)
            {
                Id = log.Id;
                UserId = log.UserId;
                DetectionType = log.DetectionType;
                Details = log.Details;
                Timestamp = log.Timestamp;
                LichSuLamBaiId = log.LichSuLamBaiId;

                // Xử lý các giá trị null tại đây
                UserName = log.User?.Hoten ?? "N/A";
                LichSuLamBaiMaDeThi = log.LichSuLamBai?.MaDeThi ?? "N/A";
                LichSuLamBaiThoiDiemBatDau = log.LichSuLamBai?.ThoiDiemBatDau;
            }

            // Constructor mặc định (cần thiết cho deserialization JSON hoặc binding form)
            public CheatDetectionLogViewModel() { }
        }

        // ViewModel cho trang Index, chứa danh sách các log và thông tin phân trang/lọc
        public class CheatDetectionLogListViewModel
        {
            public IEnumerable<CheatDetectionLogViewModel> Logs { get; set; } = new List<CheatDetectionLogViewModel>();
            public int PageNumber { get; set; }
            public int PageSize { get; set; }
            public int TotalPages { get; set; }
            public int TotalItems { get; set; }
            public string? FilterUserId { get; set; } // Giá trị lọc theo UserId
        }
    }
}
