using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using TCN_NCKH.Models.DBModel;

namespace TCN_NCKH.Areas.HocSinh.Controllers
{
    [Area("HocSinh")]
    public class LamBaiController : Controller
    {
        private readonly NghienCuuKhoaHocContext _context;

        public LamBaiController(NghienCuuKhoaHocContext context)
        {
            _context = context;
        }

        // Hiển thị đề thi cho học sinh
        [HttpGet]
        public async Task<IActionResult> Index(string id)
        {
            if (string.IsNullOrWhiteSpace(id))
                return BadRequest("Mã đề thi không hợp lệ.");

            var lichThi = await _context.Lichthis
                .Include(l => l.Dethi)
                    .ThenInclude(d => d.Cauhois)
                        .ThenInclude(c => c.Dapans)
                .FirstOrDefaultAsync(l => l.Dethi.Id == id);

            if (lichThi == null || lichThi.Dethi == null)
                return NotFound("Không tìm thấy đề thi.");

            var msv = User.FindFirst("MaSinhVien")?.Value;
            if (string.IsNullOrWhiteSpace(msv))
            {
                return Unauthorized("Không xác định được sinh viên.");
            }

            var sinhVien = await _context.Sinhviens.FirstOrDefaultAsync(s => s.Msv == msv);
            if (sinhVien == null)
            {
                return Unauthorized("Sinh viên không tồn tại.");
            }

            // ❌ Nếu đã quá hạn thi
            // Kiểm tra đã hết hạn chưa
            if (DateTime.Now > lichThi.Ngaythi.AddMinutes(lichThi.Thoigian))
            {
                return BadRequest("Đã quá hạn làm bài.");
            }

            // ❌ Nếu đã nộp rồi thì không được làm lại
            var daNop = await _context.Ketquathis
                .AnyAsync(k => k.Sinhvienid == sinhVien.Sinhvienid && k.Lichthiid == lichThi.Id);
            if (daNop)
                return BadRequest("Bạn đã nộp bài thi này. Không thể làm lại.");

            return View(lichThi);
        }

        [HttpPost]
        public async Task<IActionResult> NopBai(int lichThiId, Dictionary<int, List<int>> dapAnChon)
        {
            var msv = User.FindFirstValue(ClaimTypes.Name);
            if (string.IsNullOrWhiteSpace(msv))
                return Unauthorized("Không xác định được sinh viên.");

            var sinhVien = await _context.Sinhviens.FirstOrDefaultAsync(s => s.Msv == msv);
            if (sinhVien == null)
                return Unauthorized("Sinh viên không tồn tại.");

            var lichThi = await _context.Lichthis
                .Include(l => l.Dethi)
                    .ThenInclude(d => d.Cauhois)
                        .ThenInclude(c => c.Dapans)
                .FirstOrDefaultAsync(l => l.Id == lichThiId);

            if (lichThi == null || lichThi.Dethi == null)
                return NotFound("Không tìm thấy đề thi.");

            // ❌ Nếu quá hạn nộp bài
            if (DateTime.Now > lichThi.Ngaythi.AddMinutes(lichThi.Thoigian))
            {
                return BadRequest("Đã hết thời gian nộp bài.");
            }

            // ❌ Nếu đã nộp trước đó
            bool daNop = await _context.Ketquathis
                .AnyAsync(k => k.Sinhvienid == sinhVien.Sinhvienid && k.Lichthiid == lichThiId);
            if (daNop)
                return BadRequest("Bạn đã nộp bài rồi và không được nộp lại.");

            var now = DateTime.Now;

            // ************ CHỈNH SỬA TỪ ĐÂY ************

            // Tạo đối tượng Ketquathi trước
            var ketQuaMoi = new Ketquathi
            {
                Sinhvienid = sinhVien.Sinhvienid,
                Lichthiid = lichThiId,
                // Điểm sẽ được tính sau khi có tất cả câu trả lời và có thể set sau
                Thoigianlam = lichThi.Thoigian,
                Ngaythi = now
            };
            // Thêm Ketquathi vào DbContext ngay để nó được gán một ID tạm thời (nếu ID là tự tăng)
            // Hoặc nếu không tự tăng, nó sẽ nhận ID sau khi SaveChangesAsync()
            _context.Ketquathis.Add(ketQuaMoi); // Sử dụng Add thay vì AddAsync nếu không cần await ngay

            var newTraLois = new List<TraloiSinhvien>();
            foreach (var cauHoi in lichThi.Dethi.Cauhois)
            {
                if (dapAnChon.TryGetValue(cauHoi.Id, out List<int> dapAnIds))
                {
                    foreach (var dapanId in dapAnIds.Distinct())
                    {
                        newTraLois.Add(new TraloiSinhvien
                        {
                            Sinhvienid = sinhVien.Sinhvienid,
                            Cauhoiid = cauHoi.Id,
                            Dapanid = dapanId,
                            Ngaytraloi = now,
                            Ketquathi = ketQuaMoi // <--- Gán đối tượng Ketquathi vào TraloiSinhvien
                        });
                    }
                }
            }
            await _context.TraloiSinhviens.AddRangeAsync(newTraLois);

            // Bây giờ tính điểm và gán vào ketQuaMoi
            ketQuaMoi.Diem = TinhDiem(lichThi.Dethi.Cauhois, dapAnChon);

            // ************ KẾT THÚC CHỈNH SỬA ************

            await _context.SaveChangesAsync(); // Lưu tất cả thay đổi, bao gồm Ketquathi và TraloiSinhviens

            return RedirectToAction("KetQua", new { id = lichThiId });
        }
        // Hàm tính điểm
        private double TinhDiem(IEnumerable<Cauhoi> cauHois, Dictionary<int, List<int>> dapAnChon)
        {
            double diem = 0;
            foreach (var cauHoi in cauHois)
            {
                if (dapAnChon.TryGetValue(cauHoi.Id, out var dapAnIds))
                {
                    var dapAnDung = cauHoi.Dapans?.Where(d => d.Dung == true).Select(d => d.Id).ToList() ?? new List<int>();

                    bool chonDungHet = dapAnDung.All(id => dapAnIds.Contains(id));
                    bool chonThemSai = dapAnIds.Except(dapAnDung).Any();

                    if (chonDungHet && !chonThemSai)
                    {
                        diem += cauHoi.Diem ?? 1;
                    }
                }
            }
            return diem;
        }

        // Hiển thị kết quả thi
        [HttpGet]
        public async Task<IActionResult> KetQua(int id)
        {
            var msv = User.FindFirstValue(ClaimTypes.Name);
            if (string.IsNullOrWhiteSpace(msv))
                return Unauthorized("Không xác định được sinh viên.");

            var sinhVien = await _context.Sinhviens.FirstOrDefaultAsync(s => s.Msv == msv);
            if (sinhVien == null)
                return Unauthorized("Không tìm thấy sinh viên.");

            var ketQua = await _context.Ketquathis
                .Include(k => k.Lichthi)
                    .ThenInclude(l => l.Dethi)
                        .ThenInclude(d => d.Cauhois)
                            .ThenInclude(c => c.Dapans)
                .FirstOrDefaultAsync(k => k.Lichthiid == id && k.Sinhvienid == sinhVien.Sinhvienid);

            if (ketQua == null)
                return NotFound("Không tìm thấy kết quả thi.");

            var traLoiDict = await _context.TraloiSinhviens
                .Where(t => t.Sinhvienid == sinhVien.Sinhvienid && t.Cauhoiid != null && t.Dapanid != null)
                .GroupBy(t => t.Cauhoiid!.Value)
                .ToDictionaryAsync(
                    g => g.Key,
                    g => g.Select(t => t.Dapanid!.Value).ToList()
                );

            var viewModel = new KetQuaViewModel
            {
                Ketquathi = ketQua,
                DapAnChon = traLoiDict,
                Diem = ketQua.Diem
            };

            return View(viewModel);
        }
    }
}
