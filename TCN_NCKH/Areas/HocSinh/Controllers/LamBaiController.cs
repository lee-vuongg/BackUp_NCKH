using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using TCN_NCKH.Models.DBModel;
using System.Collections.Generic;
using System;
using System.Linq;
using System.Threading.Tasks;
using TCN_NCKH.Areas.HocSinh.Models; // Dòng này cần thiết cho LamBaiViewModel
using TCN_NCKH.Models; // <--- ĐẢM BẢO DÒNG NÀY ĐÚNG VỚI NAMESPACE CỦA SecurityViolationLog.cs

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
            Console.WriteLine("DEBUG: Vào action Index của LamBaiController.");

            string originalId = id;
            id = id?.Trim(); // Trim ID đề thi
            Console.WriteLine($"DEBUG: ID nhận được: '{originalId}', sau khi Trim: '{id ?? "NULL/Empty"}'");

            if (string.IsNullOrWhiteSpace(id))
            {
                Console.WriteLine("DEBUG: ID đề thi rỗng hoặc chỉ chứa khoảng trắng. Chuyển hướng về Home.");
                TempData["ErrorMessage"] = "Mã đề thi không hợp lệ.";
                return RedirectToAction("Index", "Home", new { area = "HocSinh" });
            }

            Console.WriteLine($"DEBUG: Đang tìm kiếm Lichthi và Dethi với ID: '{id}'");

            var lichThi = await _context.Lichthis
                .Include(l => l.Dethi)
                    .ThenInclude(d => d.Monhoc)
                .Include(l => l.Dethi)
                    .ThenInclude(d => d.Bocauhoi)
                .FirstOrDefaultAsync(l => l.Dethi != null && l.Dethi.Id != null && l.Dethi.Id.Trim() == id);

            Console.WriteLine($"DEBUG: Kết quả tìm kiếm Lichthi: {(lichThi != null ? $"ID: {lichThi.Id}, Tên đề: {lichThi.Dethi?.Tendethi ?? "N/A"}" : "NULL")}");

            if (lichThi == null || lichThi.Dethi == null)
            {
                Console.WriteLine("DEBUG: Không tìm thấy lịch thi hoặc đề thi liên quan. Chuyển hướng về Home.");
                TempData["ErrorMessage"] = "Không tìm thấy đề thi.";
                return RedirectToAction("Index", "Home", new { area = "HocSinh" });
            }

            List<Cauhoi> actualCauhois = new List<Cauhoi>();
            if (lichThi.Dethi.Bocauhoiid.HasValue)
            {
                Console.WriteLine($"DEBUG: Đề thi '{lichThi.Dethi.Tendethi}' được liên kết với Bộ câu hỏi ID: {lichThi.Dethi.Bocauhoiid.Value}.");
                var questionsQuery = _context.Cauhois
                                             .AsNoTracking() // Thêm AsNoTracking()
                                             .Include(c => c.Dapans)
                                             .Where(c => c.Bocauhoiid == lichThi.Dethi.Bocauhoiid.Value)
                                             .AsQueryable();

                int? soLuongCauHoiToTake = lichThi.Dethi.Soluongcauhoi;
                if (soLuongCauHoiToTake.HasValue && soLuongCauHoiToTake.Value > 0)
                {
                    var allQuestionsInBocauhoi = await questionsQuery.ToListAsync();
                    Random rng = new Random();
                    allQuestionsInBocauhoi = allQuestionsInBocauhoi.OrderBy(a => rng.Next()).ToList();
                    actualCauhois = allQuestionsInBocauhoi.Take(soLuongCauHoiToTake.Value).ToList();
                    Console.WriteLine($"DEBUG: Đã chọn ngẫu nhiên {actualCauhois.Count} câu hỏi từ Bộ câu hỏi (yêu cầu {soLuongCauHoiToTake.Value} câu).");
                }
                else
                {
                    actualCauhois = await questionsQuery.ToListAsync();
                    Console.WriteLine($"DEBUG: Đã lấy tất cả {actualCauhois.Count} câu hỏi từ Bộ câu hỏi.");
                }
            }
            else
            {
                Console.WriteLine("DEBUG: Đề thi không liên kết Bộ câu hỏi. Lấy câu hỏi trực tiếp từ đề thi (nếu có).");
                var dethiWithDirectQuestions = await _context.Dethis
                                                             .AsNoTracking() // Thêm AsNoTracking()
                                                             .Include(d => d.Cauhois)
                                                                 .ThenInclude(c => c.Dapans)
                                                             .FirstOrDefaultAsync(d => d.Id.Trim() == id);
                actualCauhois = dethiWithDirectQuestions?.Cauhois?.ToList() ?? new List<Cauhoi>();
                Console.WriteLine($"DEBUG: Số lượng câu hỏi trực tiếp từ đề thi: {actualCauhois.Count}");
            }

            if (lichThi.Dethi != null)
            {
                lichThi.Dethi.Cauhois = actualCauhois;
            }
            Console.WriteLine($"DEBUG: Tổng số lượng câu hỏi SẼ ĐƯỢC HIỂN THỊ: {lichThi.Dethi?.Cauhois?.Count ?? 0}");

            var msv = User.FindFirstValue("MaSinhVien")?.Trim(); // Trim MSV từ Claims
            Console.WriteLine($"DEBUG: Mã số sinh viên từ Claims (đã trim): '{msv ?? "NULL/Empty"}'");

            if (string.IsNullOrWhiteSpace(msv))
            {
                Console.WriteLine("DEBUG: Không xác định được sinh viên từ Claims. Chuyển hướng đến trang đăng nhập.");
                TempData["ErrorMessage"] = "Không xác định được sinh viên. Vui lòng đăng nhập lại.";
                return RedirectToAction("Login", "Account", new { area = "" });
            }

            // Trim MSV khi truy vấn DB và khi so sánh
            var sinhVien = await _context.Sinhviens.FirstOrDefaultAsync(s => s.Msv.Trim() == msv);
            Console.WriteLine($"DEBUG: Sinh viên tìm thấy trong DB: {(sinhVien != null ? sinhVien.Msv?.Trim() : "NULL")}");
            Console.WriteLine($"DEBUG: Sinhvien.Sinhvienid của sinh viên tìm thấy: {(sinhVien != null ? sinhVien.Sinhvienid?.Trim() : "NULL")}"); // Thêm Trim cho Sinhvienid nếu nó là string

            if (sinhVien == null)
            {
                Console.WriteLine("DEBUG: Sinh viên không tồn tại trong hệ thống. Chuyển hướng về Home.");
                TempData["ErrorMessage"] = "Sinh viên không tồn tại trong hệ thống.";
                return RedirectToAction("Index", "Home", new { area = "HocSinh" });
            }

            DateTime thoiGianKetThucThi = lichThi.Ngaythi.AddMinutes(lichThi.Thoigian.GetValueOrDefault(0));
            Console.WriteLine($"DEBUG: Thời gian hiện tại: {DateTime.Now}, Thời gian kết thúc thi: {thoiGianKetThucThi}");

            // Trim Sinhvienid khi so sánh
            bool daNop = await _context.Ketquathis
                .AnyAsync(k => k.Sinhvienid.Trim() == sinhVien.Sinhvienid.Trim() && k.Lichthiid == lichThi.Id);
            Console.WriteLine($"DEBUG: Đã nộp bài: {daNop}");

            bool isExpired = false;
            string expiryMessage = "";

            if (DateTime.Now > thoiGianKetThucThi)
            {
                Console.WriteLine("DEBUG: Đã quá hạn làm bài. Thiết lập thông báo.");
                isExpired = true;
                expiryMessage = "Bài thi đã hết thời gian làm bài. Bạn không thể bắt đầu hoặc nộp bài thi này.";
            }
            else if (daNop)
            {
                Console.WriteLine("DEBUG: Sinh viên đã nộp bài này. Thiết lập thông báo.");
                isExpired = true;
                expiryMessage = "Bạn đã nộp bài thi này rồi và không thể làm lại.";
            }

            // --- LÔ-GIC: Xử lý LichSuLamBai ---
            LichSuLamBai lichSuLamBai;

            // Luôn cố gắng tìm LichSuLamBai để có ID đúng, ngay cả khi bài thi đã hết hạn/đã nộp
            lichSuLamBai = await _context.LichSuLamBais
                .FirstOrDefaultAsync(ls => ls.MaSinhVien.Trim() == sinhVien.Sinhvienid.Trim() && ls.LichThiId == lichThi.Id); // SỬ DỤNG SINHVIENID.TRIM() Ở ĐÂY!

            if (!isExpired) // Chỉ tạo/cập nhật nếu bài thi chưa hết hạn và chưa nộp
            {
                if (lichSuLamBai == null)
                {
                    // Tạo bản ghi LichSuLamBai mới nếu chưa có
                    lichSuLamBai = new LichSuLamBai
                    {
                        // THAY ĐỔI DÒNG NÀY:
                        MaSinhVien = sinhVien.Sinhvienid.Trim(), // SỬ DỤNG SINHVIENID thay vì MSV
                        MaDeThi = lichThi.Dethi.Id.Trim(), // Trim MaDeThi
                        LichThiId = lichThi.Id,
                        ThoiDiemBatDau = DateTime.Now,
                        ThoiDiemKetThuc = null,
                        SoLanRoiManHinh = 0,
                        TrangThaiNopBai = false.ToString(),
                        Ipaddress = HttpContext.Connection.RemoteIpAddress?.ToString(),
                        TongDiemDatDuoc = 0,
                        DaChamDiem = false,
                    };
                    _context.LichSuLamBais.Add(lichSuLamBai);
                    Console.WriteLine("DEBUG: Đã tạo mới bản ghi LichSuLamBai.");
                }
                else
                {
                    // Cập nhật ThoiDiemBatDau nếu sinh viên quay lại làm bài đang dở
                    lichSuLamBai.ThoiDiemBatDau = DateTime.Now;
                    lichSuLamBai.TrangThaiNopBai = false.ToString();
                    _context.LichSuLamBais.Update(lichSuLamBai); // Đảm bảo entity được đánh dấu là Modified
                    Console.WriteLine("DEBUG: Đã cập nhật ThoiDiemBatDau cho bản ghi LichSuLamBai hiện có.");
                }

                try
                {
                    await _context.SaveChangesAsync();
                    Console.WriteLine($"DEBUG: LichSuLamBai ID sau SaveChanges: {lichSuLamBai.Id}");
                }
                catch (DbUpdateException ex)
                {
                    Console.WriteLine($"ERROR: Lỗi khi lưu LichSuLamBai: {ex.Message}");
                    Console.WriteLine($"ERROR: Inner Exception: {ex.InnerException?.Message ?? "N/A"}");
                    TempData["ErrorMessage"] = "Có lỗi xảy ra khi bắt đầu làm bài. Vui lòng thử lại. Chi tiết: " + (ex.InnerException?.Message ?? ex.Message);
                    return RedirectToAction("Index", "Home", new { area = "HocSinh" });
                }
            }
            else
            {
                if (lichSuLamBai == null)
                {
                    lichSuLamBai = new LichSuLamBai { Id = 0 };
                    Console.WriteLine("DEBUG: LichSuLamBai không tìm thấy và bài thi đã hết hạn/đã nộp. Tạo LichSuLamBai ảo với ID 0.");
                }
                else
                {
                    Console.WriteLine($"DEBUG: Bài thi đã hết hạn/đã nộp. LichSuLamBai đã được tải với ID: {lichSuLamBai.Id}.");
                }
            }
            // --- KẾT THÚC LÔ-GIC: Xử lý LichSuLamBai ---

            var viewModel = new LamBaiViewModel
            {
                LichThi = lichThi,
                LichSuLamBaiId = lichSuLamBai.Id,
                SinhVienMsv = msv,
                ThoiGianKetThucThi = thoiGianKetThucThi,
                IsExpired = isExpired,
                ExpiryMessage = expiryMessage
            };

            if (!isExpired)
            {
                TempData["SuccessMessage"] = "Bạn đã sẵn sàng làm bài thi. Chúc bạn may mắn!";
            }

            return View(viewModel);
        }

        [HttpPost]
        public async Task<IActionResult> NopBai(int lichThiId, int lichSuLamBaiId, Dictionary<int, List<int>> dapAnChon)
        {
            Console.WriteLine($"DEBUG: Vào action NopBai (POST). LichThiId: {lichThiId}, LichSuLamBaiId: {lichSuLamBaiId}");

            var msv = User.FindFirstValue("MaSinhVien")?.Trim(); // Trim MSV từ Claims
            Console.WriteLine($"DEBUG: MSV từ Claims trong NopBai (đã trim): '{msv ?? "NULL/Empty"}'");

            if (string.IsNullOrWhiteSpace(msv))
            {
                Console.WriteLine("DEBUG: Không xác định được sinh viên trong NopBai. Chuyển hướng đến trang đăng nhập.");
                TempData["ErrorMessage"] = "Phiên đăng nhập đã hết hạn hoặc không xác định được sinh viên.";
                return RedirectToAction("Login", "Account", new { area = "" });
            }

            // Trim MSV khi truy vấn DB
            var sinhVien = await _context.Sinhviens.FirstOrDefaultAsync(s => s.Msv.Trim() == msv);
            if (sinhVien == null)
            {
                Console.WriteLine("DEBUG: Sinh viên không tồn tại trong NopBai. Chuyển hướng về Home.");
                TempData["ErrorMessage"] = "Sinh viên không tồn tại trong hệ thống.";
                return RedirectToAction("Index", "Home", new { area = "HocSinh" });
            }

            Console.WriteLine($"DEBUG: Đang tìm kiếm Lichthi với ID: {lichThiId} cho NopBai.");
            var lichThi = await _context.Lichthis
                .Include(l => l.Dethi)
                    .ThenInclude(d => d.Bocauhoi)
                .FirstOrDefaultAsync(l => l.Id == lichThiId);

            if (lichThi == null || lichThi.Dethi == null)
            {
                Console.WriteLine("DEBUG: Không tìm thấy đề thi để nộp bài. Chuyển hướng về Home.");
                TempData["ErrorMessage"] = "Không tìm thấy đề thi để nộp bài.";
                return RedirectToAction("Index", "Home", new { area = "HocSinh" });
            }

            List<Cauhoi> questionsForScoring = new List<Cauhoi>();
            if (lichThi.Dethi.Bocauhoiid.HasValue)
            {
                questionsForScoring = await _context.Cauhois
                                                   .AsNoTracking() // Thêm AsNoTracking()
                                                   .Include(c => c.Dapans)
                                                   .Where(c => c.Bocauhoiid == lichThi.Dethi.Bocauhoiid.Value)
                                                   .ToListAsync();
            }
            else
            {
                var dethiWithDirectQuestions = await _context.Dethis
                                                             .AsNoTracking() // Thêm AsNoTracking()
                                                             .Include(d => d.Cauhois)
                                                                 .ThenInclude(c => c.Dapans)
                                                             .FirstOrDefaultAsync(d => d.Id.Trim() == lichThi.Dethi.Id.Trim());
                questionsForScoring = dethiWithDirectQuestions?.Cauhois?.ToList() ?? new List<Cauhoi>();
            }
            Console.WriteLine($"DEBUG: NopBai: Đã tải {questionsForScoring.Count} câu hỏi để tính điểm.");

            DateTime thoiGianKetThucThi = lichThi.Ngaythi.AddMinutes(lichThi.Thoigian.GetValueOrDefault(0));
            Console.WriteLine($"DEBUG: Thời gian hiện tại trong NopBai: {DateTime.Now}, Thời gian kết thúc thi: {thoiGianKetThucThi}");
            if (DateTime.Now > thoiGianKetThucThi)
            {
                Console.WriteLine("DEBUG: Đã hết thời gian nộp bài. Chuyển hướng về trang Kết Quả.");
                TempData["ErrorMessage"] = "Đã hết thời gian nộp bài. Bài của bạn không được chấp nhận.";
                return RedirectToAction("KetQua", new { id = lichThiId });
            }

            // Trim Sinhvienid khi so sánh
            Console.WriteLine($"DEBUG: Kiểm tra đã nộp bài trong NopBai cho sinh viên '{sinhVien.Sinhvienid?.Trim()}' và lịch thi '{lichThiId}'.");
            bool daNop = await _context.Ketquathis
                .AnyAsync(k => k.Sinhvienid.Trim() == sinhVien.Sinhvienid.Trim() && k.Lichthiid == lichThiId);
            if (daNop)
            {
                Console.WriteLine("DEBUG: Sinh viên đã nộp bài này. Chuyển hướng về trang Kết Quả.");
                TempData["ErrorMessage"] = "Bạn đã nộp bài rồi và không được nộp lại.";
                return RedirectToAction("KetQua", new { id = lichThiId });
            }

            var now = DateTime.Now;
            Console.WriteLine("DEBUG: Tạo Ketquathi mới.");
            var ketQuaMoi = new Ketquathi
            {
                Sinhvienid = sinhVien.Sinhvienid.Trim(), // Lưu Sinhvienid đã trim
                Lichthiid = lichThiId,
                Thoigianlam = lichThi.Thoigian.GetValueOrDefault(0),
                Ngaythi = now,
                Diem = TinhDiem(questionsForScoring, dapAnChon)
            };
            _context.Ketquathis.Add(ketQuaMoi);

            var newTraLois = new List<TraloiSinhvien>();
            Console.WriteLine($"DEBUG: Xử lý {questionsForScoring.Count} câu hỏi để lưu câu trả lời.");
            foreach (var cauHoi in questionsForScoring)
            {
                if (dapAnChon.TryGetValue(cauHoi.Id, out List<int> dapAnIds))
                {
                    foreach (var dapanId in dapAnIds.Distinct())
                    {
                        newTraLois.Add(new TraloiSinhvien
                        {
                            Sinhvienid = sinhVien.Sinhvienid.Trim(), // Lưu Sinhvienid đã trim
                            Cauhoiid = cauHoi.Id,
                            Dapanid = dapanId,
                            Ngaytraloi = now,
                            Ketquathi = ketQuaMoi
                        });
                    }
                }
            }
            await _context.TraloiSinhviens.AddRangeAsync(newTraLois);

            // --- LÔ-GIC: Cập nhật LichSuLamBai khi nộp bài ---
            Console.WriteLine($"DEBUG: Đang tìm kiếm LichSuLamBai để cập nhật (ID từ form: {lichSuLamBaiId}, MSV: '{sinhVien.Msv.Trim()}', LichThiId: {lichThiId}).");
            var lichSuLamBai = await _context.LichSuLamBais
                .FirstOrDefaultAsync(ls => ls.Id == lichSuLamBaiId && ls.MaSinhVien.Trim() == sinhVien.Sinhvienid.Trim() && ls.LichThiId == lichThiId); // SỬ DỤNG SINHVIENID.TRIM() Ở ĐÂY!

            if (lichSuLamBai != null)
            {
                Console.WriteLine($"DEBUG: Đã tìm thấy LichSuLamBai để cập nhật. ID: {lichSuLamBai.Id}");
                lichSuLamBai.ThoiDiemKetThuc = now;
                lichSuLamBai.TrangThaiNopBai = true.ToString();
                lichSuLamBai.TongDiemDatDuoc = ketQuaMoi.Diem;
                lichSuLamBai.DaChamDiem = true;
                lichSuLamBai.Ipaddress = HttpContext.Connection.RemoteIpAddress?.ToString();

                Console.WriteLine("DEBUG: Các giá trị LichSuLamBai trước khi cập nhật:");
                Console.WriteLine($"  ThoiDiemKetThuc: {lichSuLamBai.ThoiDiemKetThuc}");
                Console.WriteLine($"  TrangThaiNopBai: {lichSuLamBai.TrangThaiNopBai}");
                Console.WriteLine($"  TongDiemDatDuoc: {lichSuLamBai.TongDiemDatDuoc}");
                Console.WriteLine($"  DaChamDiem: {lichSuLamBai.DaChamDiem}");
                Console.WriteLine($"  Ipaddress: {lichSuLamBai.Ipaddress}");
                Console.WriteLine($"  LichSuLamBai.MaSinhVien (from DB): '{lichSuLamBai.MaSinhVien?.Trim()}'"); // Show trimmed MSV from DB
                Console.WriteLine($"  LichSuLamBai.MaDeThi (from DB): '{lichSuLamBai.MaDeThi?.Trim()}'");    // Show trimmed MaDeThi from DB
                Console.WriteLine($"  LichSuLamBai.LichThiId (from DB): {lichSuLamBai.LichThiId}");

                _context.LichSuLamBais.Update(lichSuLamBai);
                Console.WriteLine("DEBUG: Đã cập nhật bản ghi LichSuLamBai trong bộ nhớ.");
            }
            else
            {
                Console.WriteLine($"WARNING: KHÔNG tìm thấy bản ghi LichSuLamBai để cập nhật khi nộp bài (ID: {lichSuLamBaiId}, MSV: '{sinhVien.Msv.Trim()}', LichThiId: {lichThiId}).");
                Console.WriteLine("WARNING: Dữ liệu thời gian kết thúc, IP, và trạng thái chấm điểm CÓ THỂ KHÔNG ĐƯỢC LƯU.");
                // Log all existing LichSuLamBai entries for this student and lichThiId to debug
                var existingEntries = await _context.LichSuLamBais
                    .Where(ls => ls.MaSinhVien.Trim() == sinhVien.Sinhvienid.Trim() && ls.LichThiId == lichThiId) // SỬ DỤNG SINHVIENID.TRIM() Ở ĐÂY!
                    .ToListAsync();
                if (existingEntries.Any())
                {
                    Console.WriteLine("WARNING: Các bản ghi LichSuLamBai hiện có cho sinh viên và lịch thi này:");
                    foreach (var entry in existingEntries)
                    {
                        Console.WriteLine($"  - ID: {entry.Id}, MSV: '{entry.MaSinhVien?.Trim()}', LichThiId: {entry.LichThiId}, ThoiDiemBatDau: {entry.ThoiDiemBatDau}");
                    }
                }
                else
                {
                    Console.WriteLine("WARNING: Không có bản ghi LichSuLamBai nào tồn tại cho sinh viên và lịch thi này.");
                }
            }
            // --- KẾT THÚC LÔ-GIC: Xử lý LichSuLamBai ---

            try
            {
                await _context.SaveChangesAsync();
                Console.WriteLine("DEBUG: Đã lưu dữ liệu nộp bài và LichSuLamBai thành công.");
                TempData["SuccessMessage"] = $"Bạn đã nộp bài thành công! Điểm của bạn là: {ketQuaMoi.Diem}";
                return RedirectToAction("KetQua", new { id = lichThiId });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"ERROR: Lỗi khi lưu dữ liệu nộp bài: {ex.Message}");
                Console.WriteLine($"ERROR: Inner Exception: {ex.InnerException?.Message ?? "N/A"}");
                TempData["ErrorMessage"] = "Đã xảy ra lỗi khi nộp bài. Vui lòng thử lại.";
                return RedirectToAction("Index", "Home", new { area = "HocSinh" });
            }
        }


        // --- NEW ACTION: LogSecurityViolation ---
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> LogSecurityViolation([FromBody] SecurityViolationLog log)
        {
            Console.WriteLine($"DEBUG: LogSecurityViolation called. LichSuLamBaiId: {log.LichSuLamBaiId}, Type: {log.ViolationType}, Message: {log.Message}");

            var msvTuClaim = User.FindFirstValue("MaSinhVien")?.Trim();
            if (string.IsNullOrWhiteSpace(msvTuClaim))
            {
                Console.WriteLine("ERROR: LogSecurityViolation: MSV not found in claims.");
                return Unauthorized(new { message = "Unauthorized: MSV not found." });
            }

            var sinhVienHienTai = await _context.Sinhviens.FirstOrDefaultAsync(s => s.Msv.Trim() == msvTuClaim);
            if (sinhVienHienTai == null)
            {
                Console.WriteLine($"ERROR: LogSecurityViolation: Sinhvien with MSV '{msvTuClaim}' not found in DB.");
                return NotFound(new { message = "Sinhvien not found in database." });
            }
            string sinhVienIdDeSoSanh = sinhVienHienTai.Sinhvienid.Trim();

            try
            {
                // Quan trọng: Chỉ tìm và cập nhật LichSuLamBai, không liên quan đến các entity khác
                var lichSuLamBai = await _context.LichSuLamBais
                    .FirstOrDefaultAsync(ls => ls.Id == log.LichSuLamBaiId && ls.MaSinhVien.Trim() == sinhVienIdDeSoSanh);

                if (lichSuLamBai == null)
                {
                    Console.WriteLine($"WARNING: LogSecurityViolation: LichSuLamBai with ID {log.LichSuLamBaiId} not found for SinhvienID '{sinhVienIdDeSoSanh}'.");
                    return NotFound(new { message = "LichSuLamBai record not found or does not match user." });
                }

                bool updated = false;
                if (log.ViolationType == "TabSwitch" || log.ViolationType == "WindowSwitch" || log.ViolationType == "FullscreenExit")
                {
                    lichSuLamBai.SoLanRoiManHinh = (lichSuLamBai.SoLanRoiManHinh ?? 0) + 1;
                    updated = true;
                    Console.WriteLine($"DEBUG: Incrementing SoLanRoiManHinh for LichSuLamBaiId {log.LichSuLamBaiId}. New count: {lichSuLamBai.SoLanRoiManHinh}");
                }
                // Thêm các loại vi phạm khác nếu cần
                // else if (log.ViolationType == "CopyPaste") { /* ... */ }
                // else if (log.ViolationType == "DeveloperTools") { /* ... */ }

                if (updated)
                {
                    // Không cần _context.LichSuLamBais.Update(lichSuLamBai) nếu đối tượng đã được lấy từ context
                    // và bạn chỉ sửa đổi thuộc tính của nó. EF Core sẽ tự động theo dõi thay đổi.
                    // Tuy nhiên, nếu bạn muốn chắc chắn, việc gọi Update cũng không sai.
                    // _context.LichSuLamBais.Update(lichSuLamBai); 
                    await _context.SaveChangesAsync(); // Chỉ lưu các thay đổi của lichSuLamBai
                    Console.WriteLine($"DEBUG: LichSuLamBaiId {log.LichSuLamBaiId} updated successfully.");
                }

                return Json(new { success = true, newSoLanRoiManHinh = lichSuLamBai.SoLanRoiManHinh });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"ERROR: LogSecurityViolation: Exception: {ex.Message}");
                Console.WriteLine($"ERROR: LogSecurityViolation: Inner Exception: {ex.InnerException?.Message ?? "N/A"}");
                return StatusCode(500, new { message = "An error occurred while logging the violation.", error = ex.Message });
            }
        }
        // --- END NEW ACTION ---

        // Hàm tính điểm (giữ nguyên)
        private double TinhDiem(IEnumerable<Cauhoi> cauHois, Dictionary<int, List<int>> dapAnChon)
        {
            Console.WriteLine("DEBUG: Bắt đầu tính điểm.");
            double diem = 0;
            foreach (var cauHoi in cauHois)
            {
                if (dapAnChon.TryGetValue(cauHoi.Id, out var dapAnIds))
                {
                    var dapAnDung = cauHoi.Dapans?.Where(d => d.Dung == true).Select(d => d.Id).ToList() ?? new List<int>();
                    Console.WriteLine($"DEBUG: Câu hỏi {cauHoi.Id}. Đáp án đúng: {string.Join(",", dapAnDung)}. Đáp án chọn: {string.Join(",", dapAnIds)}.");

                    bool chonDungHet = dapAnDung.All(id => dapAnIds.Contains(id));
                    bool chonThemSai = dapAnIds.Except(dapAnDung).Any();

                    if (chonDungHet && !chonThemSai)
                    {
                        diem += cauHoi.Diem ?? 1;
                        Console.WriteLine($"DEBUG: Câu hỏi {cauHoi.Id} đúng. Cộng {cauHoi.Diem ?? 1} điểm. Tổng điểm hiện tại: {diem}");
                    }
                    else
                    {
                        Console.WriteLine($"DEBUG: Câu hỏi {cauHoi.Id} sai.");
                    }
                }
                else
                {
                    Console.WriteLine($"DEBUG: Câu hỏi {cauHoi.Id} không được trả lời.");
                }
            }
            Console.WriteLine($"DEBUG: Kết thúc tính điểm. Tổng điểm: {diem}");
            return diem;
        }

        // Hiển thị kết quả thi (giữ nguyên)
        [HttpGet]
        public async Task<IActionResult> KetQua(int id)
        {
            Console.WriteLine($"DEBUG: Vào action KetQua (GET). LichThiId: {id}");

            var msv = User.FindFirstValue("MaSinhVien")?.Trim(); // Trim MSV từ Claims
            Console.WriteLine($"DEBUG: MSV từ Claims trong KetQua (đã trim): '{msv ?? "NULL/Empty"}'");

            if (string.IsNullOrWhiteSpace(msv))
            {
                Console.WriteLine("DEBUG: Không xác định được sinh viên trong KetQua. Chuyển hướng đến trang đăng nhập.");
                TempData["ErrorMessage"] = "Phiên đăng nhập đã hết hạn hoặc không xác định được sinh viên.";
                return RedirectToAction("Login", "Account", new { area = "" });
            }

            // Trim MSV khi truy vấn DB
            var sinhVien = await _context.Sinhviens.FirstOrDefaultAsync(s => s.Msv.Trim() == msv);
            if (sinhVien == null)
            {
                Console.WriteLine("DEBUG: Sinh viên không tồn tại trong KetQua. Chuyển hướng về Home.");
                TempData["ErrorMessage"] = "Sinh viên không tồn tại trong hệ thống.";
                return RedirectToAction("Index", "Home", new { area = "HocSinh" });
            }

            Console.WriteLine($"DEBUG: Đang tìm kiếm Ketquathi cho sinh viên '{sinhVien.Sinhvienid?.Trim()}' và lịch thi '{id}'."); // Trim Sinhvienid
            var ketQua = await _context.Ketquathis
                .Include(k => k.Lichthi)
                    .ThenInclude(l => l.Dethi)
                        .ThenInclude(d => d.Monhoc)
                .Include(k => k.Lichthi.Dethi)
                    .ThenInclude(d => d.Cauhois)
                        .ThenInclude(c => c.Dapans)
                .FirstOrDefaultAsync(k => k.Lichthiid == id && k.Sinhvienid.Trim() == sinhVien.Sinhvienid.Trim()); // Trim Sinhvienid khi so sánh

            if (ketQua == null)
            {
                Console.WriteLine("DEBUG: Không tìm thấy kết quả thi của sinh viên này. Chuyển hướng về Home.");
                TempData["ErrorMessage"] = "Không tìm thấy kết quả thi của bạn.";
                return RedirectToAction("Index", "Home", new { area = "HocSinh" });
            }
            Console.WriteLine($"DEBUG: Đã tìm thấy kết quả thi. Điểm: {ketQua.Diem}");

            Console.WriteLine($"DEBUG: Đang tải các câu trả lời của sinh viên cho lịch thi '{id}'.");
            var traLoiDict = await _context.TraloiSinhviens
                .Where(t => t.Sinhvienid.Trim() == sinhVien.Sinhvienid.Trim() && t.Cauhoiid != null && t.Dapanid != null) // Trim Sinhvienid
                .GroupBy(t => t.Cauhoiid!.Value)
                .ToDictionaryAsync(
                    g => g.Key,
                    g => g.Select(t => t.Dapanid!.Value).ToList()
                );
            Console.WriteLine($"DEBUG: Đã tải {traLoiDict.Count} câu trả lời của sinh viên.");

            var viewModel = new KetQuaViewModel
            {
                Ketquathi = ketQua,
                DapAnChon = traLoiDict,
                Diem = ketQua.Diem
            };
            Console.WriteLine("DEBUG: ViewModel cho KetQua đã được chuẩn bị. Trả về View.");
            return View(viewModel);
        }
    }
}
