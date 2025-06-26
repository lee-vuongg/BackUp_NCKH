using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using TCN_NCKH.Models.DBModel;

namespace TCN_NCKH.Areas.HocSinh.Controllers
{
    [Area("HocSinh")]
    [Authorize(Roles = "Sinh viên")]
    public class HocSinhHomeController : Controller
    {
        private readonly NghienCuuKhoaHocContext _context;

        public HocSinhHomeController(NghienCuuKhoaHocContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
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

            // ************ ĐOẠN CODE ĐÃ ĐƯỢC TỐI ƯU LẠI ĐỂ KHẮC PHỤC LỖI 'WITH' ************

            var lichThisChuaNop = await _context.Lichthis
                .AsNoTracking() // Vẫn giữ AsNoTracking vì nó vẫn có thể cải thiện hiệu suất
                .Include(l => l.Dethi)
                    .ThenInclude(d => d.Monhoc)
                .Include(l => l.Lophoc)
                .Where(l => l.Ngaythi > DateTime.Now &&
                            l.Ngaythi.AddMinutes(l.Thoigian) > DateTime.Now &&
                            // Thay vì dùng .Contains() với danh sách đã lấy sẵn,
                            // chúng ta dùng Any() với điều kiện ! (NOT EXISTS)
                            // Điều này sẽ được dịch thành một subquery NOT EXISTS trong SQL
                            !_context.Ketquathis.Any(k => k.Sinhvienid == sinhVien.Sinhvienid && k.Lichthiid == l.Id))
                .OrderBy(l => l.Ngaythi)
                .ToListAsync();

            // ************ KẾT THÚC ĐOẠN CODE TỐI ƯU ************

            ViewBag.LichThiSapToi = lichThisChuaNop;

            return View();
        }
    }
}