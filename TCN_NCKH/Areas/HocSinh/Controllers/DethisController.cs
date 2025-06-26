// File: TCN_NCKH/Areas/HocSinh/Controllers/DeThisController.cs

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Security.Claims;
using TCN_NCKH.Models.DBModel;
using TCN_NCKH.Models.ViewModels; // <-- Cần thêm dòng này để sử dụng DeThiViewModel
using System.Collections.Generic; // Cần thiết cho List<T>
using System.Threading.Tasks; // Cần thiết cho Task

namespace TCN_NCKH.Areas.HocSinh.Controllers
{
    [Area("HocSinh")]
    [Authorize(Roles = "Sinh viên")]
    public class DeThisController : Controller
    {
        private readonly NghienCuuKhoaHocContext _context;

        public DeThisController(NghienCuuKhoaHocContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            // Lấy mã số sinh viên của người dùng hiện tại
            var msv = User.FindFirstValue(ClaimTypes.Name);
            if (string.IsNullOrWhiteSpace(msv))
            {
                return Unauthorized("Không xác định được sinh viên.");
            }

            var sinhVien = await _context.Sinhviens.FirstOrDefaultAsync(s => s.Msv == msv);
            if (sinhVien == null)
            {
                return Unauthorized("Sinh viên không tồn tại.");
            }

            // Lấy tất cả đề thi
            var deThis = await _context.Dethis
                .AsNoTracking()
                .Include(d => d.Monhoc) // Nạp thông tin môn học
                .Include(d => d.NguoitaoNavigation) // Nạp thông tin người tạo (Giáo viên)
                .OrderBy(d => d.Tendethi)
                .ToListAsync();

            // Tạo danh sách ViewModel để truyền sang View
            var deThiViewModels = new List<DeThiViewModel>();

            foreach (var deThi in deThis)
            {
                // Kiểm tra xem sinh viên đã nộp bài thi nào liên quan đến đề thi này chưa.
                // Logic: Tìm xem có kết quả thi nào của sinh viên này mà
                // lịch thi của kết quả đó có DethiId trùng với Id của đề thi hiện tại không.
                bool daNopBai = await _context.Ketquathis
                    .AsNoTracking()
                    .Include(k => k.Lichthi) // Quan trọng: Cần Include Lichthi để có thể truy cập DethiId từ Lichthi
                    .AnyAsync(k => k.Sinhvienid == sinhVien.Sinhvienid &&
                                   k.Lichthi != null && k.Lichthi.Dethiid == deThi.Id);

                deThiViewModels.Add(new DeThiViewModel
                {
                    DeThi = deThi, // Gán đối tượng Dethi gốc
                    DaNopBai = daNopBai // Gán trạng thái đã nộp bài
                });
            }

            return View(deThiViewModels); // Truyền danh sách ViewModel sang View
        }
    }
}