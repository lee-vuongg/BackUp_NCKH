using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using TCN_NCKH.Models.DBModel;

namespace TCN_NCKH.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class KetquathisController : Controller
    {
        private readonly NghienCuuKhoaHocContext _context;

        public KetquathisController(NghienCuuKhoaHocContext context)
        {
            _context = context;
        }

        // GET: Admin/Ketquathis
        // Phương thức Index này sẽ hiển thị tổng quan các lịch thi và thống kê sinh viên
        public async Task<IActionResult> Index()
        {
            // Lấy tất cả lịch thi và eager load thông tin cần thiết
            var lichthis = await _context.Lichthis
                                         .Include(lt => lt.Dethi)
                                         .Include(lt => lt.Lophoc)
                                             .ThenInclude(lh => lh.Sinhviens) // Eager load Sinhviens trong Lophoc để đếm tổng SV
                                         .OrderByDescending(lt => lt.Ngaythi) // Sắp xếp theo ngày thi mới nhất
                                         .ToListAsync();

            var lichthiSummaries = new List<LichthiSummary>();

            foreach (var lichthi in lichthis)
            {
                // Tổng số sinh viên trong lớp học của lịch thi này
                int totalStudents = lichthi.Lophoc?.Sinhviens.Count ?? 0;

                // Số sinh viên đã hoàn thành bài thi cho lịch thi này
                // Sử dụng Distinct() để đảm bảo mỗi sinh viên chỉ được tính một lần
                int studentsCompleted = await _context.Ketquathis
                                                    .Where(kq => kq.Lichthiid == lichthi.Id)
                                                    .Select(kq => kq.Sinhvienid)
                                                    .Distinct()
                                                    .CountAsync();

                // Số sinh viên chưa hoàn thành bài thi
                int studentsNotCompleted = totalStudents - studentsCompleted;
                if (studentsNotCompleted < 0) studentsNotCompleted = 0; // Đảm bảo không âm

                lichthiSummaries.Add(new LichthiSummary
                {
                    Lichthi = lichthi,
                    TongSoSinhVienLopHoc = totalStudents,
                    SoLuongSinhVienDaLam = studentsCompleted,
                    SoLuongSinhVienChuaLam = studentsNotCompleted
                });
            }

            var viewModel = new KetquathiListViewModel
            {
                LichthiSummaries = lichthiSummaries
            };

            return View(viewModel);
        }

        // GET: Admin/Ketquathis/Details/5 (Chi tiết một kết quả thi cụ thể của sinh viên)
        // Phương thức này dùng để xem CHI TIẾT KẾT QUẢ CỦA MỘT BÀI LÀM CỤ THỂ, chứ không phải tổng quan lịch thi.
        // Trong KetquathisController.cs
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                TempData["ErrorMessage"] = "Không tìm thấy ID kết quả thi.";
                return NotFound();
            }

            var ketquathi = await _context.Ketquathis
                .Include(k => k.Lichthi)
                    .ThenInclude(lt => lt.Dethi)
                .Include(k => k.Lichthi)
                    .ThenInclude(lt => lt.Lophoc)
                .Include(k => k.Sinhvien)
                    .ThenInclude(sv => sv.Lophoc)
                .Include(k => k.Sinhvien)
                    .ThenInclude(sv => sv.SinhvienNavigation) // Đã sửa thành SinhvienNavigation
                .Include(k => k.TraloiSinhviens) // Load các câu trả lời của sinh viên
                    .ThenInclude(ts => ts.Dapan) // Load đáp án mà sinh viên đã chọn
                        .ThenInclude(da => da.Cauhoi) // Load câu hỏi liên quan đến đáp án đó
                            .ThenInclude(ch => ch.Dapans) // <== THÊM DÒNG NÀY: Load tất cả đáp án của câu hỏi để tìm đáp án đúng
                .FirstOrDefaultAsync(m => m.Id == id);

            if (ketquathi == null)
            {
                TempData["ErrorMessage"] = "Kết quả thi không tồn tại.";
                return RedirectToAction(nameof(Index));
            }

            return View(ketquathi);
        }

        // GET: Admin/Ketquathis/StudentsByLichthi/5
        // Hiển thị danh sách sinh viên đã làm bài / chưa làm bài cho một lịch thi cụ thể
        // Hiển thị danh sách sinh viên đã làm bài / chưa làm bài cho một lịch thi cụ thể
        public async Task<IActionResult> StudentsByLichthi(int? id, bool? showCompleted = true)
        {
            if (id == null)
            {
                TempData["ErrorMessage"] = "Không tìm thấy ID lịch thi.";
                return NotFound();
            }

            var lichthi = await _context.Lichthis
                                        .Include(lt => lt.Dethi)
                                        .Include(lt => lt.Lophoc)
                                            .ThenInclude(lh => lh.Sinhviens) // Load tất cả sinh viên trong lớp
                                        .FirstOrDefaultAsync(lt => lt.Id == id);

            if (lichthi == null)
            {
                TempData["ErrorMessage"] = "Lịch thi không tồn tại.";
                return RedirectToAction(nameof(Index));
            }

            ViewData["Lichthi"] = lichthi;
            ViewData["ShowCompleted"] = showCompleted;

            List<Sinhvien> students = new List<Sinhvien>();

            if (showCompleted == true) // Lấy sinh viên đã làm bài
            {
                students = await _context.Ketquathis
                                        .Where(kq => kq.Lichthiid == id)
                                        .Select(kq => kq.Sinhvien!)
                                        .Distinct()
                                        .Include(sv => sv.Lophoc)
                                        .Include(sv => sv.SinhvienNavigation)
                                        .ToListAsync();

                foreach (var sv in students)
                {
                    await _context.Entry(sv)
                                  .Collection(s => s.Ketquathis)
                                  .Query()
                                  .Where(kq => kq.Lichthiid == id)
                                  .LoadAsync();
                }
            }
            else // Lấy sinh viên chưa làm bài
            {
                students = await _context.Sinhviens
                    .Include(sv => sv.Lophoc)
                    .Include(sv => sv.SinhvienNavigation)
                    .Where(sv => sv.Lophocid == lichthi.Lophocid) // Lọc sinh viên trong lớp học của lịch thi
                    .Where(sv => !_context.Ketquathis // Tìm những sinh viên KHÔNG CÓ kết quả thi cho lịch thi này
                        .Any(kq => kq.Lichthiid == id && kq.Sinhvienid == sv.Sinhvienid))
                    .ToListAsync();
            }

            return View(students);
        }



        // GET: Admin/Ketquathis/Create
        public IActionResult Create()
        {
            var lichthis = _context.Lichthis
                                .Include(lt => lt.Dethi)
                                .Include(lt => lt.Lophoc)
                                .AsEnumerable() // Chuyển sang client-side để tạo chuỗi hiển thị
                                .Select(lt => new
                                {
                                    Id = lt.Id,
                                    DisplayText = $"{lt.Dethi?.Tendethi} - {lt.Lophoc?.Tenlop} ({lt.Thoigian.ToString("dd/MM/yyyy HH:mm")})"
                                })
                                .ToList();

            ViewData["Lichthiid"] = new SelectList(lichthis, "Id", "DisplayText");
            ViewData["Sinhvienid"] = new SelectList(_context.Sinhviens.OrderBy(sv => sv.Msv), "Sinhvienid", "Msv");
            return View();
        }

        // POST: Admin/Ketquathis/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Sinhvienid,Lichthiid,Diem,Thoigianlam,Ngaythi")] Ketquathi ketquathi)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    _context.Add(ketquathi);
                    await _context.SaveChangesAsync();
                    TempData["SuccessMessage"] = "Kết quả thi đã được tạo thành công!";
                    return RedirectToAction(nameof(Index));
                }
                catch (Exception ex)
                {
                    TempData["ErrorMessage"] = $"Đã xảy ra lỗi khi tạo kết quả thi: {ex.Message}";
                }
            }
            var lichthis = _context.Lichthis
                                .Include(lt => lt.Dethi)
                                .Include(lt => lt.Lophoc)
                                .AsEnumerable()
                                .Select(lt => new
                                {
                                    Id = lt.Id,
                                    DisplayText = $"{lt.Dethi?.Tendethi} - {lt.Lophoc?.Tenlop} ({lt.Thoigian.ToString("dd/MM/yyyy HH:mm")})"
                                })
                                .ToList();

            ViewData["Lichthiid"] = new SelectList(lichthis, "Id", "DisplayText", ketquathi.Lichthiid);
            ViewData["Sinhvienid"] = new SelectList(_context.Sinhviens.OrderBy(sv => sv.Msv), "Sinhvienid", "Msv", ketquathi.Sinhvienid);
            TempData["ErrorMessage"] = "Có lỗi xảy ra khi tạo kết quả thi. Vui lòng kiểm tra lại thông tin.";
            string validationErrors = string.Join("<br/>", ModelState.Values
                                                  .SelectMany(v => v.Errors)
                                                  .Select(e => e.ErrorMessage));
            if (!string.IsNullOrEmpty(validationErrors))
            {
                TempData["ErrorMessage"] = (TempData["ErrorMessage"] as string ?? "") + "<br/>" + validationErrors;
            }
            return View(ketquathi);
        }

        // GET: Admin/Ketquathis/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                TempData["ErrorMessage"] = "Không tìm thấy ID kết quả thi.";
                return NotFound();
            }

            var ketquathi = await _context.Ketquathis
                                        .Include(k => k.Lichthi)
                                            .ThenInclude(lt => lt.Dethi)
                                        .Include(k => k.Lichthi)
                                            .ThenInclude(lt => lt.Lophoc)
                                        .Include(k => k.Sinhvien)
                                        .FirstOrDefaultAsync(m => m.Id == id);
            if (ketquathi == null)
            {
                TempData["ErrorMessage"] = "Kết quả thi không tồn tại.";
                return RedirectToAction(nameof(Index));
            }

            var lichthis = _context.Lichthis
                                .Include(lt => lt.Dethi)
                                .Include(lt => lt.Lophoc)
                                .AsEnumerable()
                                .Select(lt => new
                                {
                                    Id = lt.Id,
                                    DisplayText = $"{lt.Dethi?.Tendethi} - {lt.Lophoc?.Tenlop} ({lt.Thoigian.ToString("dd/MM/yyyy HH:mm")})"
                                })
                                .ToList();

            ViewData["Lichthiid"] = new SelectList(lichthis, "Id", "DisplayText", ketquathi.Lichthiid);
            ViewData["Sinhvienid"] = new SelectList(_context.Sinhviens.OrderBy(sv => sv.Msv), "Sinhvienid", "Msv", ketquathi.Sinhvienid);
            return View(ketquathi);
        }

        // POST: Admin/Ketquathis/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Sinhvienid,Lichthiid,Diem,Thoigianlam,Ngaythi")] Ketquathi ketquathi)
        {
            if (id != ketquathi.Id)
            {
                TempData["ErrorMessage"] = "ID kết quả thi không hợp lệ.";
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(ketquathi);
                    await _context.SaveChangesAsync();
                    TempData["SuccessMessage"] = "Kết quả thi đã được cập nhật thành công!";
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!KetquathiExists(ketquathi.Id))
                    {
                        TempData["ErrorMessage"] = "Kết quả thi không tồn tại hoặc đã bị xóa bởi người khác.";
                        return NotFound();
                    }
                    else
                    {
                        TempData["ErrorMessage"] = "Đã xảy ra lỗi đồng thời khi cập nhật dữ liệu. Vui lòng thử lại.";
                        throw;
                    }
                }
                catch (Exception ex)
                {
                    TempData["ErrorMessage"] = $"Đã xảy ra lỗi hệ thống: {ex.Message}";
                }
                return RedirectToAction(nameof(Index));
            }
            var lichthis = _context.Lichthis
                                .Include(lt => lt.Dethi)
                                .Include(lt => lt.Lophoc)
                                .AsEnumerable()
                                .Select(lt => new
                                {
                                    Id = lt.Id,
                                    DisplayText = $"{lt.Dethi?.Tendethi} - {lt.Lophoc?.Tenlop} ({lt.Thoigian.ToString("dd/MM/yyyy HH:mm")})"
                                })
                                .ToList();
            ViewData["Lichthiid"] = new SelectList(lichthis, "Id", "DisplayText", ketquathi.Lichthiid);
            ViewData["Sinhvienid"] = new SelectList(_context.Sinhviens.OrderBy(sv => sv.Msv), "Sinhvienid", "Msv", ketquathi.Sinhvienid);
            TempData["ErrorMessage"] = "Thông tin kết quả thi không hợp lệ. Vui lòng kiểm tra lại.";
            string validationErrors = string.Join("<br/>", ModelState.Values
                                                  .SelectMany(v => v.Errors)
                                                  .Select(e => e.ErrorMessage));
            if (!string.IsNullOrEmpty(validationErrors))
            {
                TempData["ErrorMessage"] = (TempData["ErrorMessage"] as string ?? "") + "<br/>" + validationErrors;
            }
            return View(ketquathi);
        }

        // GET: Admin/Ketquathis/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                TempData["ErrorMessage"] = "Không tìm thấy ID kết quả thi.";
                return NotFound();
            }

            var ketquathi = await _context.Ketquathis
                .Include(k => k.Lichthi)
                    .ThenInclude(lt => lt.Dethi)
                .Include(k => k.Lichthi)
                    .ThenInclude(lt => lt.Lophoc)
                .Include(k => k.Sinhvien)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (ketquathi == null)
            {
                TempData["ErrorMessage"] = "Kết quả thi không tồn tại.";
                return RedirectToAction(nameof(Index));
            }

            return View(ketquathi);
        }

        // POST: Admin/Ketquathis/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var ketquathi = await _context.Ketquathis.FindAsync(id);
            if (ketquathi == null)
            {
                TempData["ErrorMessage"] = "Kết quả thi không tồn tại hoặc đã bị xóa trước đó.";
                return RedirectToAction(nameof(Index));
            }

            try
            {
                _context.Ketquathis.Remove(ketquathi);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Kết quả thi đã được xóa thành công!";
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Đã xảy ra lỗi khi xóa kết quả thi: {ex.Message}";
            }
            return RedirectToAction(nameof(Index));
        }
        
        private bool KetquathiExists(int id)
        {
            return _context.Ketquathis.Any(e => e.Id == id);
        }
    }
}