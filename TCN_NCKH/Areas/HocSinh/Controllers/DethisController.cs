using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Security.Claims;
using TCN_NCKH.Models.DBModel;
using TCN_NCKH.Models.ViewModels; // Cần thêm dòng này để sử dụng DeThiViewModel
using System.Collections.Generic; // Cần thiết cho List<T>
using System.Threading.Tasks; // Cần thiết cho Task
using System; // Cần thiết cho Console.WriteLine

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
            Console.WriteLine("DEBUG: Đã vào action Index của DeThisController.");

            // Lấy mã số sinh viên của người dùng hiện tại
            var msv = User.FindFirstValue(ClaimTypes.Name);
            Console.WriteLine($"DEBUG: Mã số sinh viên (MSV) từ Claims: {msv ?? "NULL/Empty"}");

            if (string.IsNullOrWhiteSpace(msv))
            {
                Console.WriteLine("DEBUG: MSV không xác định. Trả về Unauthorized.");
                TempData["ErrorMessage"] = "Không xác định được sinh viên. Vui lòng đăng nhập lại.";
                return RedirectToAction("Login", "Account", new { area = "" }); // Giả định có Controller Account bên ngoài Area
            }

            var sinhVien = await _context.Sinhviens.FirstOrDefaultAsync(s => s.Msv == msv);
            Console.WriteLine($"DEBUG: Sinh viên tìm thấy trong DB: {(sinhVien != null ? sinhVien.Msv : "NULL")}");

            if (sinhVien == null)
            {
                Console.WriteLine("DEBUG: Không tìm thấy sinh viên trong DB. Trả về Unauthorized.");
                TempData["ErrorMessage"] = "Thông tin sinh viên không tồn tại trong hệ thống.";
                return RedirectToAction("Login", "Account", new { area = "" }); // Chuyển hướng đến trang đăng nhập
            }

            // Lấy tất cả đề thi
            Console.WriteLine("DEBUG: Đang truy vấn danh sách đề thi...");
            var deThis = await _context.Dethis
                .AsNoTracking()
                .Include(d => d.Monhoc) // Nạp thông tin môn học
                .Include(d => d.NguoitaoNavigation) // Nạp thông tin người tạo (Giáo viên)
                .OrderBy(d => d.Tendethi)
                .ToListAsync();
            Console.WriteLine($"DEBUG: Đã tải {deThis.Count} đề thi từ DB.");

            // Tạo danh sách ViewModel để truyền sang View
            var deThiViewModels = new List<DethiViewModel>();
            Console.WriteLine("DEBUG: Bắt đầu xử lý từng đề thi để tạo ViewModel.");

            foreach (var deThi in deThis)
            {
                Console.WriteLine($"DEBUG: Đang xử lý đề thi: {deThi.Id} - {deThi.Tendethi}");

                // Kiểm tra xem sinh viên đã nộp bài thi nào liên quan đến đề thi này chưa.
                bool daNopBai = await _context.Ketquathis
                    .AsNoTracking()
                    .Include(k => k.Lichthi) // Quan trọng: Cần Include Lichthi để có thể truy cập DethiId từ Lichthi
                    .AnyAsync(k => k.Sinhvienid == sinhVien.Sinhvienid &&
                                   k.Lichthi != null && k.Lichthi.Dethiid != null && k.Lichthi.Dethiid.Trim() == deThi.Id.Trim());

                Console.WriteLine($"DEBUG: Đề thi {deThi.Id} - Đã nộp bài: {daNopBai}");

                deThiViewModels.Add(new DethiViewModel
                {
                    DeThi = deThi, // Gán đối tượng Dethi gốc
                    DaNopBai = daNopBai, // Gán trạng thái đã nộp bài
                    // Các thuộc tính khác của DethiViewModel không cần gán ở đây vì đã có DeThi = deThi
                    // Nếu mi muốn hiển thị trực tiếp các thuộc tính như Tendethi, Monhocid, v.v., trên view
                    // mà không cần truy cập qua DeThi.Tendethi, thì mi có thể ánh xạ chúng ở đây:
                    Id = deThi.Id,
                    Tendethi = deThi.Tendethi,
                    Monhocid = deThi.Monhocid,
                    Bocauhoiid = deThi.Bocauhoiid,
                    Soluongcauhoi = deThi.Soluongcauhoi,
                    Mucdokho = null, // Hoặc gán giá trị mặc định nếu cần
                    DethikhacnhauCa = deThi.DethikhacnhauCa
                });
            }

            Console.WriteLine($"DEBUG: Đã tạo {deThiViewModels.Count} DethiViewModel. Chuẩn bị trả về View.");
            return View(deThiViewModels); // Truyền danh sách ViewModel sang View
        }
    }
}
