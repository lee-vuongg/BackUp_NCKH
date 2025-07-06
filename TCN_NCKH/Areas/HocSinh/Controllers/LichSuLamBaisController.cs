using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims; // Cần thiết để lấy thông tin user đang đăng nhập
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization; // Để sử dụng [Authorize]
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using TCN_NCKH.Models.DBModel;
using TCN_NCKH.Models.ViewModels; // Đảm bảo đã import ViewModel của mi vào đây

namespace TCN_NCKH.Areas.HocSinh.Controllers
{
    [Area("HocSinh")]
    // Chỉ cho phép người dùng có vai trò "SinhVien" (hoặc tên vai trò tương ứng) truy cập
    [Authorize(Roles = "SinhVien")] // Giả định role của học sinh là "SinhVien"
    public class LichSuLamBaisController : Controller
    {
        private readonly NghienCuuKhoaHocContext _context;

        public LichSuLamBaisController(NghienCuuKhoaHocContext context)
        {
            _context = context;
        }

        // Helper để lấy MaSinhVien của học sinh đang đăng nhập
        // Giả định MaSinhVien (Sinhvienid) của học sinh được lưu trữ trong ClaimTypes.NameIdentifier là string
        private string GetLoggedInStudentId()
        {
            return User.FindFirstValue(ClaimTypes.NameIdentifier);
        }

        // GET: HocSinh/LichSuLamBais
        // Hiển thị danh sách lịch sử làm bài của học sinh đang đăng nhập
        public async Task<IActionResult> Index()
        {
            var loggedInStudentId = GetLoggedInStudentId();

            if (string.IsNullOrEmpty(loggedInStudentId))
            {
                // Xử lý trường hợp không lấy được ID học sinh (chưa đăng nhập hoặc lỗi)
                return Unauthorized(); // Hoặc RedirectToPage("/Account/Login")
            }

            // Lấy lịch sử làm bài của CHÍNH HỌC SINH đang đăng nhập
            var lichSuLamBais = await _context.LichSuLamBais
                .Where(l => l.MaSinhVien == loggedInStudentId) // Lọc theo MaSinhVien của học sinh đang đăng nhập
                .Include(l => l.LichThi) // Bao gồm thông tin LichThi
                    .ThenInclude(lt => lt.Dethi) // Và bao gồm thông tin DeThi từ LichThi
                .Include(l => l.MaDeThiNavigation) // Bao gồm thông tin DeThi liên quan trực tiếp đến LichSuLamBai (nếu có)
                .Include(l => l.MaSinhVienNavigation) // Bao gồm thông tin SinhVien
                .Select(l => new LichSuLamBaiDetailsViewModel // Ánh xạ sang ViewModel để hiển thị
                {
                    Id = l.Id,
                    ThoiDiemBatDau = l.ThoiDiemBatDau,
                    ThoiDiemKetThuc = l.ThoiDiemKetThuc,
                    SoLanRoiManHinh = l.SoLanRoiManHinh, // Đã khớp kiểu dữ liệu int?
                    TrangThaiNopBai = l.TrangThaiNopBai, // Đã khớp kiểu dữ liệu bool?
                    Ipaddress = l.Ipaddress,
                    TongDiemDatDuoc = l.TongDiemDatDuoc, // Đã khớp kiểu dữ liệu double?
                    DaChamDiem = l.DaChamDiem, // Đã khớp kiểu dữ liệu bool?

                    // Sửa lỗi CS0072: Xử lý null trước khi gọi ToString()
                    ThongTinLichThi = l.LichThi != null && l.LichThi.Dethi != null
                                      ? $"{l.LichThi.Dethi.Tendethi} ({(l.LichThi.ThoigianBatdau.HasValue ? l.LichThi.ThoigianBatdau.Value.ToString("dd/MM/yyyy HH:mm") : "N/A")})"
                                      : "Không rõ lịch thi",
                    // Sửa lỗi CS1061 (nếu có): Đảm bảo thuộc tính đúng là TenDeThi
                    TenDeThi = l.MaDeThiNavigation != null ? l.MaDeThiNavigation.Tendethi : "Không rõ đề thi",
                    // Sửa lỗi CS1061: Giả định tên thuộc tính là HoTen trong model Sinhvien
                    TenSinhVien = l.MaSinhVienNavigation != null ? l.MaSinhVienNavigation.SinhvienNavigation.Hoten : "Không rõ sinh viên"
                })
                .OrderByDescending(l => l.ThoiDiemBatDau) // Sắp xếp theo thời gian mới nhất
                .ToListAsync();

            return View(lichSuLamBais);
        }

        // GET: HocSinh/LichSuLamBais/Details/5
        // Hiển thị chi tiết một lịch sử làm bài cụ thể, đảm bảo học sinh có quyền xem
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

            // Lấy chi tiết lịch sử làm bài theo ID VÀ của CHÍNH HỌC SINH đang đăng nhập
            var lichSuLamBai = await _context.LichSuLamBais
                .Where(m => m.Id == id && m.MaSinhVien == loggedInStudentId) // Đảm bảo chỉ xem của mình
                .Include(l => l.LichThi)
                    .ThenInclude(lt => lt.Dethi)
                .Include(l => l.MaDeThiNavigation)
                .Include(l => l.MaSinhVienNavigation)
                .Select(l => new LichSuLamBaiDetailsViewModel // Ánh xạ sang ViewModel
                {
                    Id = l.Id,
                    ThoiDiemBatDau = l.ThoiDiemBatDau,
                    ThoiDiemKetThuc = l.ThoiDiemKetThuc,
                    SoLanRoiManHinh = l.SoLanRoiManHinh,
                    TrangThaiNopBai = l.TrangThaiNopBai,
                    Ipaddress = l.Ipaddress,
                    TongDiemDatDuoc = l.TongDiemDatDuoc,
                    DaChamDiem = l.DaChamDiem,

                    // Sửa lỗi CS0072: Xử lý null trước khi gọi ToString()
                    ThongTinLichThi = l.LichThi != null && l.LichThi.Dethi != null
                                      ? $"{l.LichThi.Dethi.Tendethi} ({(l.LichThi.ThoigianBatdau.HasValue ? l.LichThi.ThoigianBatdau.Value.ToString("dd/MM/yyyy HH:mm") : "N/A")})"
                                      : "Không rõ lịch thi",
                    // Sửa lỗi CS1061 (nếu có): Đảm bảo thuộc tính đúng là TenDeThi
                    TenDeThi = l.MaDeThiNavigation != null ? l.MaDeThiNavigation.Tendethi : "Không rõ đề thi",
                    // Sửa lỗi CS1061: Giả định tên thuộc tính là HoTen trong model Sinhvien
                    TenSinhVien = l.MaSinhVienNavigation != null ? l.MaSinhVienNavigation.SinhvienNavigation.Hoten : "Không rõ sinh viên"
                })
                .FirstOrDefaultAsync();

            if (lichSuLamBai == null)
            {
                // Nếu không tìm thấy hoặc ID không thuộc về học sinh đang đăng nhập
                return NotFound();
            }

            return View(lichSuLamBai);
        }

       
    }
}
