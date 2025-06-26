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
        public async Task<IActionResult> Index()
        {
            // Kiểm tra đăng nhập và phân quyền
            if (!User.Identity.IsAuthenticated || !User.IsInRole("Sinh viên"))
            {
                return RedirectToAction("Login", "Auth");
            }

            var sinhvienId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(sinhvienId))
            {
                return RedirectToAction("Login", "Auth");
            }

            // Lấy danh sách kết quả thi và bao gồm đầy đủ thông tin liên quan
            var ketquaList = await _context.Ketquathis
                .Include(k => k.Lichthi)
                    .ThenInclude(l => l.Dethi)
                        .ThenInclude(d => d.Monhoc)
                .Include(k => k.Sinhvien)
                    .ThenInclude(sv => sv.SinhvienNavigation) // <-- Đảm bảo Include SinhvienNavigation để lấy thông tin Người dùng
                .Where(k => k.Sinhvienid == sinhvienId)
                .OrderByDescending(k => k.Ngaythi)
                .ToListAsync();

            return View(ketquaList);
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