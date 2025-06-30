using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TCN_NCKH.Models.DBModel;
using System.Security.Claims; // Đảm bảo đã có

namespace TCN_NCKH.Areas.HocSinh.Controllers
{
    [Area("HocSinh")]
    public class KetquathisController : Controller
    {
        private readonly NghienCuuKhoaHocContext _context;

        public KetquathisController(NghienCuuKhoaHocContext context)
        {
            _context = context;
        }

        // GET: HocSinh/Ketquathis
        public async Task<IActionResult> Index(string dethiId) // Tham số dethiId
        {
            var msv = User.FindFirstValue(ClaimTypes.Name);
            if (string.IsNullOrWhiteSpace(msv))
            {
                TempData["ErrorMessage"] = "Không xác định được thông tin sinh viên đăng nhập.";
                return RedirectToAction("Login", "Account");
            }

            var sinhVien = await _context.Sinhviens.FirstOrDefaultAsync(s => s.Msv == msv);
            if (sinhVien == null)
            {
                TempData["ErrorMessage"] = "Thông tin sinh viên không tìm thấy trong hệ thống.";
                return RedirectToAction("Login", "Account");
            }

            // Nếu không có DethiId được cung cấp, có thể chuyển hướng hoặc hiển thị tất cả kết quả
            if (string.IsNullOrWhiteSpace(dethiId))
            {
                TempData["WarningMessage"] = "Không có mã đề thi được cung cấp. Hiển thị tất cả kết quả của bạn.";
                // Hoặc bạn có thể chọn:
                // return RedirectToAction("MyResults"); // Một action khác để xem tất cả kết quả
            }

            // Lấy kết quả thi của sinh viên hiện tại, liên quan đến đề thi được chọn
            var ketquaList = _context.Ketquathis
                                     .AsNoTracking()
                                     .Include(kq => kq.Lichthi) // Nạp thông tin lịch thi
                                         .ThenInclude(lt => lt.Dethi) // Nạp thông tin đề thi từ lịch thi
                                             .ThenInclude(dt => dt.Monhoc) // Nạp môn học từ đề thi
                                     .Where(kq => kq.Sinhvienid == sinhVien.Sinhvienid);

            // Lọc theo DethiId nếu có
            if (!string.IsNullOrWhiteSpace(dethiId))
            {
                ketquaList = ketquaList.Where(kq => kq.Lichthi != null && kq.Lichthi.Dethiid == dethiId);
            }
            else
            {
                // Nếu không có dethiId, chỉ hiển thị kết quả của sinh viên đó
                // (Có thể bạn muốn sắp xếp hoặc phân trang ở đây)
            }

            var ketquathis = await ketquaList.OrderByDescending(kq => kq.Ngaythi).ToListAsync();

            // Truyền dethiId hiện tại xuống View để có thể sử dụng cho tiêu đề hoặc logic khác
            ViewBag.CurrentDethiId = dethiId;

            return View(ketquathis);
        }

        // GET: HocSinh/Ketquathis/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
                return BadRequest("Không tìm thấy mã kết quả thi.");

            if (!User.Identity.IsAuthenticated || !User.IsInRole("Sinh viên"))
            {
                return RedirectToAction("Login", "Auth");
            }

            var sinhvienId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(sinhvienId))
            {
                return RedirectToAction("Login", "Auth");
            }

            var ketquathi = await _context.Ketquathis
                .Include(k => k.Lichthi)
                    .ThenInclude(l => l.Dethi)
                        .ThenInclude(d => d.Monhoc) // Load Monhoc
                .Include(k => k.Lichthi)
                    .ThenInclude(l => l.Lophoc) // Load Lophoc của Lichthi
                .Include(k => k.Sinhvien)
                    .ThenInclude(sv => sv.Lophoc) // Load Lophoc của Sinhvien
                .Include(k => k.Sinhvien)
                    .ThenInclude(sv => sv.SinhvienNavigation) // <-- Đảm bảo Include SinhvienNavigation
                .Include(k => k.TraloiSinhviens) // Load các câu trả lời
                    .ThenInclude(ts => ts.Dapan) // Load đáp án sinh viên chọn
                        .ThenInclude(da => da.Cauhoi) // Load câu hỏi của đáp án đó
                            .ThenInclude(ch => ch.Dapans) // <-- RẤT QUAN TRỌNG: Load TẤT CẢ các đáp án của câu hỏi để tìm đáp án đúng
                .FirstOrDefaultAsync(m => m.Id == id && m.Sinhvienid == sinhvienId);

            if (ketquathi == null)
                return NotFound("Không tìm thấy kết quả thi.");

            return View(ketquathi);
        }
    }
}