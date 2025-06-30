using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using TCN_NCKH.Models.DBModel;
using System.Collections.Generic;
using System;
using System.Linq; // Thêm dòng này để sử dụng LINQ (OrderBy, Take)
using System.Threading.Tasks; // Cần thiết cho Task

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
            id = id?.Trim();
            Console.WriteLine($"DEBUG: ID nhận được: '{originalId}', sau khi Trim: '{id ?? "NULL/Empty"}'");

            if (string.IsNullOrWhiteSpace(id))
            {
                Console.WriteLine("DEBUG: ID đề thi rỗng hoặc chỉ chứa khoảng trắng. Chuyển hướng về Home.");
                TempData["ErrorMessage"] = "Mã đề thi không hợp lệ.";
                return RedirectToAction("Index", "Home", new { area = "HocSinh" });
            }

            Console.WriteLine($"DEBUG: Đang tìm kiếm Lichthi và Dethi với ID: '{id}'");

            // Bắt đầu truy vấn Lichthi
            var lichThi = await _context.Lichthis
                .Include(l => l.Dethi) // Bao gồm đối tượng Dethi liên quan
                    .ThenInclude(d => d.Monhoc) // <--- QUAN TRỌNG: Bao gồm Monhoc từ Dethi
                .Include(l => l.Dethi) // Lặp lại include Dethi để thêm ThenInclude khác
                    .ThenInclude(d => d.Bocauhoi) // Bao gồm Bocauhoi từ Dethi (để dùng cho logic lấy câu hỏi)
                .FirstOrDefaultAsync(l => l.Dethi != null && l.Dethi.Id != null && l.Dethi.Id.Trim() == id);

            Console.WriteLine($"DEBUG: Kết quả tìm kiếm Lichthi: {(lichThi != null ? $"ID: {lichThi.Id}, Tên đề: {lichThi.Dethi?.Tendethi ?? "N/A"}" : "NULL")}");
            Console.WriteLine($"DEBUG: Tên môn học của đề thi: {lichThi?.Dethi?.Monhoc?.Tenmon ?? "N/A"}"); // Debug thêm để kiểm tra

            if (lichThi == null || lichThi.Dethi == null)
            {
                Console.WriteLine("DEBUG: Không tìm thấy lịch thi hoặc đề thi liên quan. Chuyển hướng về Home.");
                TempData["ErrorMessage"] = "Không tìm thấy đề thi.";
                return RedirectToAction("Index", "Home", new { area = "HocSinh" });
            }

            // --- LÔ-GIC: Lấy câu hỏi từ BỘ CÂU HỎI liên kết với ĐỀ THI ---
            List<Cauhoi> actualCauhois = new List<Cauhoi>();

            if (lichThi.Dethi.Bocauhoiid.HasValue)
            {
                Console.WriteLine($"DEBUG: Đề thi '{lichThi.Dethi.Tendethi}' được liên kết với Bộ câu hỏi ID: {lichThi.Dethi.Bocauhoiid.Value}.");

                // Truy vấn câu hỏi từ bảng CAUHOI có Bocauhoiid tương ứng
                var questionsQuery = _context.Cauhois
                                            .Include(c => c.Dapans) // Đảm bảo tải cả đáp án của câu hỏi
                                            .Where(c => c.Bocauhoiid == lichThi.Dethi.Bocauhoiid.Value)
                                            .AsQueryable();

                int? soLuongCauHoiToTake = lichThi.Dethi.Soluongcauhoi;

                if (soLuongCauHoiToTake.HasValue && soLuongCauHoiToTake.Value > 0)
                {
                    // Lấy tất cả câu hỏi trong bộ câu hỏi, sau đó chọn ngẫu nhiên số lượng yêu cầu
                    var allQuestionsInBocauhoi = await questionsQuery.ToListAsync();
                    Random rng = new Random();
                    allQuestionsInBocauhoi = allQuestionsInBocauhoi.OrderBy(a => rng.Next()).ToList();

                    actualCauhois = allQuestionsInBocauhoi.Take(soLuongCauHoiToTake.Value).ToList();
                    Console.WriteLine($"DEBUG: Đã chọn ngẫu nhiên {actualCauhois.Count} câu hỏi từ Bộ câu hỏi (yêu cầu {soLuongCauHoiToTake.Value} câu).");
                }
                else
                {
                    // Nếu Soluongcauhoi không được chỉ định hoặc bằng 0, lấy tất cả câu hỏi từ Bộ câu hỏi
                    actualCauhois = await questionsQuery.ToListAsync();
                    Console.WriteLine($"DEBUG: Đã lấy tất cả {actualCauhois.Count} câu hỏi từ Bộ câu hỏi.");
                }
            }
            else
            {
                // Trường hợp Đề thi KHÔNG liên kết với Bộ câu hỏi (lấy câu hỏi trực tiếp từ đề thi, nếu có)
                Console.WriteLine("DEBUG: Đề thi không liên kết Bộ câu hỏi. Lấy câu hỏi trực tiếp từ đề thi (nếu có).");
                // Cần tải lại Dethi với Cauhois nếu đi theo nhánh này
                var dethiWithDirectQuestions = await _context.Dethis
                                                             .Include(d => d.Cauhois)
                                                                .ThenInclude(c => c.Dapans)
                                                             .FirstOrDefaultAsync(d => d.Id.Trim() == id);
                actualCauhois = dethiWithDirectQuestions?.Cauhois?.ToList() ?? new List<Cauhoi>();
                Console.WriteLine($"DEBUG: Số lượng câu hỏi trực tiếp từ đề thi: {actualCauhois.Count}");
            }

            // Gán danh sách câu hỏi thực tế vào thuộc tính Cauhois của đối tượng Dethi trong Lichthi
            // Điều này đảm bảo View vẫn có thể truy cập câu hỏi thông qua Model.Dethi.Cauhois
            if (lichThi.Dethi != null)
            {
                lichThi.Dethi.Cauhois = actualCauhois;
            }
            Console.WriteLine($"DEBUG: Tổng số lượng câu hỏi SẼ ĐƯỢC HIỂN THỊ: {lichThi.Dethi?.Cauhois?.Count ?? 0}");
            // --- KẾT THÚC LÔ-GIC LẤY CÂU HỎI ---

            var msv = User.FindFirst("MaSinhVien")?.Value; // Lấy MSV từ Claims (có thể cần điều chỉnh tên Claim)
            Console.WriteLine($"DEBUG: Mã số sinh viên từ Claims: '{msv ?? "NULL/Empty"}'");

            if (string.IsNullOrWhiteSpace(msv))
            {
                Console.WriteLine("DEBUG: Không xác định được sinh viên từ Claims. Chuyển hướng đến trang đăng nhập.");
                TempData["ErrorMessage"] = "Không xác định được sinh viên. Vui lòng đăng nhập lại.";
                return RedirectToAction("Login", "Account", new { area = "" });
            }

            var sinhVien = await _context.Sinhviens.FirstOrDefaultAsync(s => s.Msv == msv);
            Console.WriteLine($"DEBUG: Sinh viên tìm thấy trong DB: {(sinhVien != null ? sinhVien.Msv : "NULL")}");

            if (sinhVien == null)
            {
                Console.WriteLine("DEBUG: Sinh viên không tồn tại trong hệ thống. Chuyển hướng về Home.");
                TempData["ErrorMessage"] = "Sinh viên không tồn tại trong hệ thống.";
                return RedirectToAction("Index", "Home", new { area = "HocSinh" });
            }

            // Kiểm tra đã quá hạn thi HOẶC ĐÃ NỘP BÀI TỪ TRƯỚC
            DateTime thoiGianKetThucThi = lichThi.Ngaythi.AddMinutes(lichThi.Thoigian.GetValueOrDefault(0));
            Console.WriteLine($"DEBUG: Thời gian hiện tại: {DateTime.Now}, Thời gian kết thúc thi: {thoiGianKetThucThi}");

            bool daNop = await _context.Ketquathis
                .AnyAsync(k => k.Sinhvienid == sinhVien.Sinhvienid && k.Lichthiid == lichThi.Id);
            Console.WriteLine($"DEBUG: Đã nộp bài: {daNop}");

            if (DateTime.Now > thoiGianKetThucThi)
            {
                Console.WriteLine("DEBUG: Đã quá hạn làm bài. Không chuyển hướng, chỉ thiết lập thông báo.");
                ViewBag.IsExpired = true; // Đặt cờ để View biết đã hết hạn
                ViewBag.ExpiryMessage = "Bài thi đã hết thời gian làm bài. Bạn không thể bắt đầu hoặc nộp bài thi này.";
            }
            else if (daNop)
            {
                Console.WriteLine("DEBUG: Sinh viên đã nộp bài này. Không chuyển hướng, chỉ thiết lập thông báo.");
                ViewBag.IsExpired = true; // Cờ này cũng có thể dùng cho trường hợp đã nộp
                ViewBag.ExpiryMessage = "Bạn đã nộp bài thi này rồi và không thể làm lại.";
            }
            else
            {
                Console.WriteLine("DEBUG: Tất cả các kiểm tra đều hợp lệ. Hiển thị View làm bài.");
                TempData["SuccessMessage"] = "Bạn đã sẵn sàng làm bài thi. Chúc bạn may mắn!";
            }

            return View(lichThi); // Luôn trả về View lichThi để hiển thị thông báo tùy chỉnh
        }

        [HttpPost]
        public async Task<IActionResult> NopBai(int lichThiId, Dictionary<int, List<int>> dapAnChon)
        {
            Console.WriteLine($"DEBUG: Vào action NopBai (POST). LichThiId: {lichThiId}");

            var msv = User.FindFirstValue(ClaimTypes.Name);
            Console.WriteLine($"DEBUG: MSV từ Claims trong NopBai: '{msv ?? "NULL/Empty"}'");

            if (string.IsNullOrWhiteSpace(msv))
            {
                Console.WriteLine("DEBUG: Không xác định được sinh viên trong NopBai. Chuyển hướng đến trang đăng nhập.");
                TempData["ErrorMessage"] = "Phiên đăng nhập đã hết hạn hoặc không xác định được sinh viên.";
                return RedirectToAction("Login", "Account", new { area = "" });
            }

            var sinhVien = await _context.Sinhviens.FirstOrDefaultAsync(s => s.Msv == msv);
            if (sinhVien == null)
            {
                Console.WriteLine("DEBUG: Sinh viên không tồn tại trong NopBai. Chuyển hướng về Home.");
                TempData["ErrorMessage"] = "Sinh viên không tồn tại trong hệ thống.";
                return RedirectToAction("Index", "Home", new { area = "HocSinh" });
            }

            Console.WriteLine($"DEBUG: Đang tìm kiếm Lichthi với ID: {lichThiId} cho NopBai.");
            var lichThi = await _context.Lichthis
                .Include(l => l.Dethi)
                    .ThenInclude(d => d.Bocauhoi) // Đảm bảo bao gồm Bocauhoi
                .FirstOrDefaultAsync(l => l.Id == lichThiId);

            if (lichThi == null || lichThi.Dethi == null)
            {
                Console.WriteLine("DEBUG: Không tìm thấy đề thi để nộp bài. Chuyển hướng về Home.");
                TempData["ErrorMessage"] = "Không tìm thấy đề thi để nộp bài.";
                return RedirectToAction("Index", "Home", new { area = "HocSinh" });
            }

            // --- LÔ-GIC: Tải câu hỏi và đáp án để tính điểm ---
            List<Cauhoi> questionsForScoring = new List<Cauhoi>();
            if (lichThi.Dethi.Bocauhoiid.HasValue)
            {
                questionsForScoring = await _context.Cauhois
                                                    .Include(c => c.Dapans)
                                                    .Where(c => c.Bocauhoiid == lichThi.Dethi.Bocauhoiid.Value)
                                                    .ToListAsync();
                Console.WriteLine($"DEBUG: NopBai: Đã tải {questionsForScoring.Count} câu hỏi từ Bộ câu hỏi để tính điểm.");
            }
            else
            {
                var dethiWithDirectQuestions = await _context.Dethis
                                                             .Include(d => d.Cauhois)
                                                                .ThenInclude(c => c.Dapans)
                                                             .FirstOrDefaultAsync(d => d.Id.Trim() == lichThi.Dethi.Id.Trim());
                questionsForScoring = dethiWithDirectQuestions?.Cauhois?.ToList() ?? new List<Cauhoi>();
                Console.WriteLine($"DEBUG: NopBai: Đã tải {questionsForScoring.Count} câu hỏi trực tiếp từ đề thi để tính điểm.");
            }
            // --- KẾT THÚC LÔ-GIC TẢI CÂU HỎI ---

            // Nếu quá hạn nộp bài
            DateTime thoiGianKetThucThi = lichThi.Ngaythi.AddMinutes(lichThi.Thoigian.GetValueOrDefault(0));
            Console.WriteLine($"DEBUG: Thời gian hiện tại trong NopBai: {DateTime.Now}, Thời gian kết thúc thi: {thoiGianKetThucThi}");
            if (DateTime.Now > thoiGianKetThucThi)
            {
                Console.WriteLine("DEBUG: Đã hết thời gian nộp bài. Chuyển hướng về trang Kết Quả.");
                TempData["ErrorMessage"] = "Đã hết thời gian nộp bài. Bài của bạn không được chấp nhận.";
                return RedirectToAction("KetQua", new { id = lichThiId });
            }

            // Nếu đã nộp trước đó
            Console.WriteLine($"DEBUG: Kiểm tra đã nộp bài trong NopBai cho sinh viên '{sinhVien.Sinhvienid}' và lịch thi '{lichThiId}'.");
            bool daNop = await _context.Ketquathis
                .AnyAsync(k => k.Sinhvienid == sinhVien.Sinhvienid && k.Lichthiid == lichThiId);
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
                Sinhvienid = sinhVien.Sinhvienid,
                Lichthiid = lichThiId,
                Thoigianlam = lichThi.Thoigian.GetValueOrDefault(0),
                Ngaythi = now
            };
            _context.Ketquathis.Add(ketQuaMoi);

            var newTraLois = new List<TraloiSinhvien>();
            Console.WriteLine($"DEBUG: Xử lý {questionsForScoring.Count} câu hỏi từ đề thi để lưu câu trả lời.");
            foreach (var cauHoi in questionsForScoring) // Lặp qua các câu hỏi đã tải từ Bocauhoi
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
                            Ketquathi = ketQuaMoi
                        });
                        Console.WriteLine($"DEBUG: - Thêm câu trả lời cho Câu hỏi {cauHoi.Id}, Đáp án {dapanId}");
                    }
                }
            }
            await _context.TraloiSinhviens.AddRangeAsync(newTraLois);

            ketQuaMoi.Diem = TinhDiem(questionsForScoring, dapAnChon); // Tính điểm dựa trên các câu hỏi đã tải
            Console.WriteLine($"DEBUG: Điểm tính được: {ketQuaMoi.Diem}");

            try
            {
                await _context.SaveChangesAsync();
                Console.WriteLine("DEBUG: Đã lưu dữ liệu nộp bài thành công.");
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

            var msv = User.FindFirstValue(ClaimTypes.Name);
            Console.WriteLine($"DEBUG: MSV từ Claims trong KetQua: '{msv ?? "NULL/Empty"}'");

            if (string.IsNullOrWhiteSpace(msv))
            {
                Console.WriteLine("DEBUG: Không xác định được sinh viên trong KetQua. Chuyển hướng đến trang đăng nhập.");
                TempData["ErrorMessage"] = "Phiên đăng nhập đã hết hạn hoặc không xác định được sinh viên.";
                return RedirectToAction("Login", "Account", new { area = "" });
            }

            var sinhVien = await _context.Sinhviens.FirstOrDefaultAsync(s => s.Msv == msv);
            if (sinhVien == null)
            {
                Console.WriteLine("DEBUG: Sinh viên không tồn tại trong KetQua. Chuyển hướng về Home.");
                TempData["ErrorMessage"] = "Sinh viên không tồn tại trong hệ thống.";
                return RedirectToAction("Index", "Home", new { area = "HocSinh" });
            }

            Console.WriteLine($"DEBUG: Đang tìm kiếm Ketquathi cho sinh viên '{sinhVien.Sinhvienid}' và lịch thi '{id}'.");
            var ketQua = await _context.Ketquathis
                .Include(k => k.Lichthi)
                    .ThenInclude(l => l.Dethi)
                        .ThenInclude(d => d.Monhoc) // <--- QUAN TRỌNG: Bao gồm Monhoc từ Dethi
                .Include(k => k.Lichthi.Dethi) // Lặp lại include Dethi để thêm ThenInclude khác
                    .ThenInclude(d => d.Cauhois) // Vẫn include để đảm bảo model đủ dữ liệu cho view hiển thị chi tiết kết quả
                        .ThenInclude(c => c.Dapans)
                .FirstOrDefaultAsync(k => k.Lichthiid == id && k.Sinhvienid == sinhVien.Sinhvienid);

            if (ketQua == null)
            {
                Console.WriteLine("DEBUG: Không tìm thấy kết quả thi của sinh viên này. Chuyển hướng về Home.");
                TempData["ErrorMessage"] = "Không tìm thấy kết quả thi của bạn.";
                return RedirectToAction("Index", "Home", new { area = "HocSinh" });
            }
            Console.WriteLine($"DEBUG: Đã tìm thấy kết quả thi. Điểm: {ketQua.Diem}");

            Console.WriteLine($"DEBUG: Đang tải các câu trả lời của sinh viên cho lịch thi '{id}'.");
            var traLoiDict = await _context.TraloiSinhviens
                .Where(t => t.Sinhvienid == sinhVien.Sinhvienid && t.Cauhoiid != null && t.Dapanid != null)
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
