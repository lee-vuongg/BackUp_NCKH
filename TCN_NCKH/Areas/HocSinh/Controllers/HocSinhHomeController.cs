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
                // Sử dụng TempData để thông báo lỗi và chuyển hướng
                TempData["ErrorMessage"] = "Không xác định được sinh viên. Vui lòng đăng nhập lại.";
                return RedirectToAction("Login", "Auth", new { area = "" });
            }

            var sinhVien = await _context.Sinhviens.FirstOrDefaultAsync(s => s.Msv == msv);
            if (sinhVien == null)
            {
                // Sử dụng TempData để thông báo lỗi và chuyển hướng
                TempData["ErrorMessage"] = "Sinh viên không tồn tại trong hệ thống.";
                return RedirectToAction("Index", "Home", new { area = "HocSinh" }); // Hoặc một trang khác phù hợp
            }

            // ************ ĐOẠN CODE ĐÃ ĐƯỢC TỐI ƯU LẠI ĐỂ KHẮC PHỤC LỖI 'WITH' ************

            var lichThisChuaNop = await _context.Lichthis
                .AsNoTracking()
                .Include(l => l.Dethi)
                    .ThenInclude(d => d.Monhoc)
                .Include(l => l.Lophoc)
                .Where(l => l.Ngaythi > DateTime.Now &&
                             // SỬA LỖI TẠI ĐÂY: Sử dụng GetValueOrDefault(0)
                             l.Ngaythi.AddMinutes(l.Thoigian.GetValueOrDefault(0)) > DateTime.Now &&
                             !_context.Ketquathis.Any(k => k.Sinhvienid == sinhVien.Sinhvienid && k.Lichthiid == l.Id))
                .OrderBy(l => l.Ngaythi)
                .ToListAsync();

            // ************ KẾT THÚC ĐOẠN CODE TỐI ƯU ************

            ViewBag.LichThiSapToi = lichThisChuaNop;

            return View();
        }
    }
}