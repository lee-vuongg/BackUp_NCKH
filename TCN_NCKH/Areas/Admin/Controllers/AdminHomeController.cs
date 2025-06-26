using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TCN_NCKH.Models.DBModel; // Đảm bảo bạn đã import namespace của DbContext và các Models
using System.Linq; // Cần thiết cho các phương thức LINQ như Where, Select, GroupBy
using Newtonsoft.Json; // Cần thiết để serialize đối tượng sang JSON

namespace TCN_NCKH.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class AdminHomeController : Controller
    {
        private readonly NghienCuuKhoaHocContext _context; // Thay TracNghiemOnlineDbContext bằng tên DbContext của bạn

        // Constructor để inject DbContext
        public AdminHomeController(NghienCuuKhoaHocContext context)
        {
            _context = context;
        }

        public IActionResult Index()
        {
            // --- Lấy dữ liệu lịch thi cho biểu đồ lịch ---
            var upcomingExams = _context.Lichthis
                                        .Where(lt => lt.Ngaythi >= DateTime.Today) // Lấy các lịch thi từ hôm nay trở đi
                                        .OrderBy(lt => lt.Ngaythi)
                                        .Select(lt => new
                                        {
                                            id = lt.Id,
                                            title = $"Thi {lt.Dethi.Tendethi ?? "Không rõ đề"} - Lớp {lt.Lophoc.Tenlop ?? "Không rõ lớp"}",
                                            start = lt.Ngaythi.ToString("yyyy-MM-dd HH:mm"), // Định dạng chuẩn cho FullCalendar
                                            duration = lt.Thoigian
                                        })
                                        .ToList();

            // Truyền dữ liệu lịch thi dưới dạng JSON string vào ViewBag
            ViewBag.UpcomingExams = JsonConvert.SerializeObject(upcomingExams);


            // --- Lấy dữ liệu điểm thi để hiển thị biểu đồ phân phối điểm ---
            var studentScores = _context.Ketquathis
                                        .GroupBy(kq => kq.Diem) // Nhóm kết quả thi theo điểm
                                        .Select(g => new
                                        {
                                            Diem = g.Key,       // Giá trị điểm
                                            SoLuong = g.Count() // Số lượng sinh viên đạt điểm đó
                                        })
                                        .OrderBy(s => s.Diem) // Sắp xếp theo điểm để biểu đồ có thứ tự
                                        .ToList();

            // Truyền dữ liệu điểm thi dưới dạng JSON string vào ViewBag
            ViewBag.StudentScores = JsonConvert.SerializeObject(studentScores);

            return View();
        }
    }
}