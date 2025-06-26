using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TCN_NCKH.Models.DBModel; // Đảm bảo namespace này trỏ đến DbContext và các model của bạn
using System.Linq; // Cần thiết cho các thao tác LINQ như Count(), Where()
using System; // Cần thiết cho DateTime.Today
using Microsoft.EntityFrameworkCore; // Cần thiết nếu dùng Include()

namespace TCN_NCKH.Areas.GiaoVien.Controllers
{
    [Area("GiaoVien")]
    [Authorize(Roles = "Giáo viên")]
    public class GiaoVienHomeController : Controller
    {
        private readonly NghienCuuKhoaHocContext _context; // Đảm bảo đây là tên DbContext của bạn

        // Constructor để inject DbContext
        public GiaoVienHomeController(NghienCuuKhoaHocContext context)
        {
            _context = context;
        }

        public IActionResult Index()
        {
            // 1. Lấy tổng số Đề Thi
            // Lọc theo người tạo nếu bạn muốn giáo viên chỉ thấy đề của họ
            // var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier); // Cần using System.Security.Claims;
            // ViewData["TotalDeThi"] = _context.Dethis.Where(d => d.Nguoitao == currentUserId).Count();

            ViewData["TotalDeThi"] = _context.Dethis.Count(); // Lấy tất cả đề thi

            // 2. Lấy số lượng Lịch Thi sắp tới (các lịch thi có Ngàythi từ hôm nay trở đi)
            ViewData["UpcomingLichThi"] = _context.Lichthis
                                                .Where(lt => lt.Ngaythi.Date >= DateTime.Today.Date) // So sánh chỉ phần ngày
                                                .Count();

            // 3. Lấy tổng số Kết Quả Thi (tổng số lượt thi)
            ViewData["TotalKetQuaThi"] = _context.Ketquathis.Count();

            // 4. Tính Tỷ lệ Đạt (Ví dụ: số bài thi có điểm >= 50 / tổng số lượt thi)
            // Bạn cần điều chỉnh ngưỡng điểm 50 cho phù hợp với quy tắc của bạn
            var totalResults = _context.Ketquathis.Count();
            var passedResults = _context.Ketquathis.Count(kq => kq.Diem >= 5); // Giả sử điểm tối thiểu để "đạt" là 5 (thang điểm 10)

            ViewData["PassRate"] = totalResults > 0 ? (int)((double)passedResults / totalResults * 100) : 0;

            // Lấy danh sách các Đề Thi gần đây (ví dụ: 5 đề thi mới nhất)
            // Lọc theo người tạo nếu cần
            var recentDeThis = _context.Dethis
                                      .OrderByDescending(d => d.Ngaytao) // Sắp xếp theo ngày tạo giảm dần
                                      .Take(5) // Lấy 5 đề thi đầu tiên
                                      .ToList();
            ViewBag.RecentDeThis = recentDeThis;

            // Lấy danh sách các Lịch Thi sắp tới (ví dụ: 5 lịch thi gần nhất từ hôm nay)
            // Bao gồm thông tin về đề thi và lớp học nếu cần hiển thị
            var upcomingLichThis = _context.Lichthis
                                          .Where(lt => lt.Ngaythi.Date >= DateTime.Today.Date)
                                          .OrderBy(lt => lt.Ngaythi) // Sắp xếp theo ngày thi tăng dần
                                          .Take(5)
                                          .Include(lt => lt.Dethi) // Nạp thông tin đề thi nếu muốn hiển thị Tendethi
                                          .Include(lt => lt.Lophoc) // Nạp thông tin lớp học nếu muốn hiển thị TenLophoc
                                          .ToList();
            ViewBag.UpcomingLichThis = upcomingLichThis;

            return View();
        }
    }
}