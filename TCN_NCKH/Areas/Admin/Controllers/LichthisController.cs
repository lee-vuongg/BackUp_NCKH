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
    public class LichthisController : Controller
    {
        private readonly NghienCuuKhoaHocContext _context;

        public LichthisController(NghienCuuKhoaHocContext context)
        {
            _context = context;
        }

        // GET: Admin/Lichthis
        public async Task<IActionResult> Index()
        {
            Console.WriteLine("DEBUG: [LichthisController][Index] - Vào action Index.");
            var nghienCuuKhoaHocContext = _context.Lichthis
                                                    .Include(l => l.Dethi)
                                                    .Include(l => l.Lophoc)
                                                    .OrderBy(l => l.Ngaythi); // Sắp xếp để dễ nhìn hơn

            var lichthisList = await nghienCuuKhoaHocContext.ToListAsync();
            Console.WriteLine($"DEBUG: [LichthisController][Index] - Đã tải {lichthisList.Count} lịch thi.");
            foreach (var lt in lichthisList)
            {
                Console.WriteLine($"DEBUG:   - Lịch thi ID: {lt.Id}, Đề thi: {lt.Dethi?.Tendethi}, Lớp: {lt.Lophoc?.Tenlop}, Ngày thi: {lt.Ngaythi}, Bắt đầu: {lt.ThoigianBatdau?.ToString("HH:mm") ?? "N/A"}, Kết thúc: {lt.ThoigianKetthuc?.ToString("HH:mm") ?? "N/A"}");
            }
            return View(lichthisList);
        }

        // GET: Admin/Lichthis/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            Console.WriteLine($"DEBUG: [LichthisController][Details] - Vào action Details. ID: {id ?? null}");
            if (id == null)
            {
                Console.WriteLine("DEBUG: [LichthisController][Details] - ID chi tiết là null, trả về NotFound.");
                return NotFound();
            }

            var lichthi = await _context.Lichthis
                .Include(l => l.Dethi)
                .Include(l => l.Lophoc)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (lichthi == null)
            {
                Console.WriteLine($"DEBUG: [LichthisController][Details] - Không tìm thấy lịch thi với ID: {id}");
                return NotFound();
            }
            Console.WriteLine($"DEBUG: [LichthisController][Details] - Đã tìm thấy lịch thi: ID={lichthi.Id}, Đề thi: {lichthi.Dethi?.Tendethi}, Lớp: {lichthi.Lophoc?.Tenlop}, Ngày thi: {lichthi.Ngaythi}, Bắt đầu: {lichthi.ThoigianBatdau?.ToString("HH:mm") ?? "N/A"}, Kết thúc: {lichthi.ThoigianKetthuc?.ToString("HH:mm") ?? "N/A"}");
            return View(lichthi);
        }

        // GET: Admin/Lichthis/Create
        public IActionResult Create()
        {
            Console.WriteLine("DEBUG: [LichthisController][Create] - Vào action Create (GET).");
            ViewData["Dethiid"] = new SelectList(_context.Dethis.OrderBy(d => d.Tendethi), "Id", "Tendethi");
            ViewData["Lophocid"] = new SelectList(_context.Lophocs.OrderBy(l => l.Tenlop), "Id", "Tenlop");
            Console.WriteLine("DEBUG: [LichthisController][Create] - ViewData cho Create (GET) đã được thiết lập.");
            return View();
        }

        // POST: Admin/Lichthis/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(
            [Bind("Dethiid,Lophocid,Phongthi,Thoigian")] Lichthi lichthi,
            DateTime ngayThiPicker, // Input type="date"
            string gioThiPicker)    // Input type="time", format "HH:mm"
        {
            Console.WriteLine("DEBUG: [LichthisController][Create] - Vào action Create (POST).");
            Console.WriteLine($"DEBUG: Dữ liệu nhận từ form:");
            Console.WriteLine($"DEBUG: - NgayThiPicker: {ngayThiPicker.ToString("yyyy-MM-dd")}");
            Console.WriteLine($"DEBUG: - GioThiPicker: {gioThiPicker}");
            Console.WriteLine($"DEBUG: - Dethiid: {lichthi.Dethiid}");
            Console.WriteLine($"DEBUG: - Lophocid: {lichthi.Lophocid}");
            Console.WriteLine($"DEBUG: - Thoigian: {lichthi.Thoigian}");
            Console.WriteLine($"DEBUG: - Phongthi: {lichthi.Phongthi}");


            // Kết hợp ngày và giờ
            if (TimeSpan.TryParse(gioThiPicker, out TimeSpan parsedTime))
            {
                lichthi.Ngaythi = ngayThiPicker.Date + parsedTime;
                Console.WriteLine($"DEBUG: Ngaythi đã được kết hợp: {lichthi.Ngaythi.ToString("dd/MM/yyyy HH:mm:ss")}");

                // Tính toán ThoigianBatdau và ThoigianKetthuc
                lichthi.ThoigianBatdau = lichthi.Ngaythi; // Thời gian bắt đầu chính là Ngaythi đã được kết hợp
                if (lichthi.Thoigian.HasValue)
                {
                    lichthi.ThoigianKetthuc = lichthi.ThoigianBatdau.Value.AddMinutes(lichthi.Thoigian.Value);
                }
                else
                {
                    lichthi.ThoigianKetthuc = null; // Hoặc một giá trị mặc định khác nếu Thoigian không có giá trị
                }
                Console.WriteLine($"DEBUG: ThoigianBatdau: {lichthi.ThoigianBatdau?.ToString("dd/MM/yyyy HH:mm:ss") ?? "N/A"}");
                Console.WriteLine($"DEBUG: ThoigianKetthuc: {lichthi.ThoigianKetthuc?.ToString("dd/MM/yyyy HH:mm:ss") ?? "N/A"}");
            }
            else
            {
                ModelState.AddModelError(string.Empty, "Định dạng giờ thi không hợp lệ. Vui lòng nhập giờ theo định dạng HH:mm.");
                Console.WriteLine("ERROR: Lỗi định dạng giờ thi.");
            }

            // Xóa lỗi validation cho ID nếu có (ID là tự động tăng)
            ModelState.Remove("Id");
            // Xóa lỗi cho navigation properties nếu chúng không được bind trực tiếp từ form
            if (ModelState.ContainsKey("Dethi")) ModelState.Remove("Dethi");
            if (ModelState.ContainsKey("Lophoc")) ModelState.Remove("Lophoc");
            if (ModelState.ContainsKey("Ketquathis")) ModelState.Remove("Ketquathis");
            if (ModelState.ContainsKey("LichthiSinhviens")) ModelState.Remove("LichthiSinhviens");


            if (ModelState.IsValid)
            {
                Console.WriteLine("DEBUG: [LichthisController][Create] - ModelState.IsValid là TRUE. Tiến hành lưu.");
                try
                {
                    _context.Add(lichthi);
                    await _context.SaveChangesAsync();
                    Console.WriteLine($"DEBUG: [LichthisController][Create] - Lịch thi ID {lichthi.Id} đã được tạo thành công.");
                    TempData["SuccessMessage"] = "Lịch thi đã được tạo thành công!";
                    return RedirectToAction(nameof(Index));
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"ERROR: [LichthisController][Create] - Lỗi khi tạo lịch thi: {ex.Message}");
                    TempData["ErrorMessage"] = $"Đã xảy ra lỗi khi tạo lịch thi: {ex.Message}";
                }
            }
            else
            {
                Console.WriteLine("DEBUG: [LichthisController][Create] - ModelState.IsValid là FALSE. Có lỗi validation.");
                var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList();
                if (errors.Any())
                {
                    Console.WriteLine("DEBUG: Các lỗi validation:");
                    foreach (var error in errors)
                    {
                        Console.WriteLine($"DEBUG: - {error}");
                    }
                    TempData["ErrorMessage"] = "Thông tin lịch thi không hợp lệ:<br/>" + string.Join("<br/>", errors);
                }
                else
                {
                    TempData["ErrorMessage"] = "Thông tin lịch thi không hợp lệ. Vui lòng kiểm tra lại.";
                }
            }

            // Nếu ModelState invalid hoặc có lỗi, cần re-populate SelectList
            ViewData["Dethiid"] = new SelectList(_context.Dethis.OrderBy(d => d.Tendethi), "Id", "Tendethi", lichthi.Dethiid);
            ViewData["Lophocid"] = new SelectList(_context.Lophocs.OrderBy(l => l.Tenlop), "Id", "Tenlop", lichthi.Lophocid);
            // Quan trọng: Truyền lại giá trị ngày và giờ cho input type="date"/"time" nếu có lỗi
            ViewData["NgayThiPicker"] = ngayThiPicker.ToString("yyyy-MM-dd");
            ViewData["GioThiPicker"] = gioThiPicker; // Giữ nguyên string để hiển thị lại
            Console.WriteLine("DEBUG: [LichthisController][Create] - ViewData đã được thiết lập lại. Trả về View với lỗi.");
            return View(lichthi);
        }

        // GET: Admin/Lichthis/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            Console.WriteLine($"DEBUG: [LichthisController][Edit] - Vào action Edit (GET). ID: {id ?? null}");
            if (id == null)
            {
                Console.WriteLine("DEBUG: [LichthisController][Edit] - ID chỉnh sửa là null, trả về NotFound.");
                return NotFound();
            }

            var lichthi = await _context.Lichthis.FindAsync(id);
            if (lichthi == null)
            {
                Console.WriteLine($"DEBUG: [LichthisController][Edit] - Không tìm thấy lịch thi với ID: {id}");
                return NotFound();
            }

            // Truyền giá trị ngày và giờ riêng biệt cho form hiển thị
            ViewData["NgayThiPicker"] = lichthi.Ngaythi.ToString("yyyy-MM-dd"); // Định dạng cho input type="date"
            ViewData["GioThiPicker"] = lichthi.Ngaythi.ToString("HH:mm");       // Định dạng cho input type="time"

            ViewData["Dethiid"] = new SelectList(_context.Dethis.OrderBy(d => d.Tendethi), "Id", "Tendethi", lichthi.Dethiid);
            ViewData["Lophocid"] = new SelectList(_context.Lophocs.OrderBy(l => l.Tenlop), "Id", "Tenlop", lichthi.Lophocid);
            Console.WriteLine($"DEBUG: [LichthisController][Edit] - Đã tìm thấy lịch thi: ID={lichthi.Id}, Đề thi: {lichthi.Dethiid}, Lớp: {lichthi.Lophocid}, Ngày thi: {lichthi.Ngaythi}, Giờ thi gốc: {ViewData["GioThiPicker"]}");
            Console.WriteLine($"DEBUG: [LichthisController][Edit] - ViewData cho Edit (GET) đã được thiết lập. Ngày thi: {ViewData["NgayThiPicker"]}, Giờ thi: {ViewData["GioThiPicker"]}");
            return View(lichthi);
        }

        // POST: Admin/Lichthis/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(
            int id,
            [Bind("Id,Dethiid,Lophocid,Phongthi,Thoigian")] Lichthi lichthiFromForm,
            DateTime ngayThiPicker, // Input type="date"
            string gioThiPicker)    // Input type="time", format "HH:mm"
        {
            Console.WriteLine($"DEBUG: [LichthisController][Edit] - Vào action Edit (POST). ID từ route: {id}");
            Console.WriteLine($"DEBUG: Dữ liệu nhận từ form:");
            Console.WriteLine($"DEBUG: - NgayThiPicker: {ngayThiPicker.ToString("yyyy-MM-dd")}");
            Console.WriteLine($"DEBUG: - GioThiPicker: {gioThiPicker}");
            Console.WriteLine($"DEBUG: - LichthiFromForm.Id: {lichthiFromForm.Id}");
            Console.WriteLine($"DEBUG: - Dethiid: {lichthiFromForm.Dethiid}");
            Console.WriteLine($"DEBUG: - Lophocid: {lichthiFromForm.Lophocid}");
            Console.WriteLine($"DEBUG: - Thoigian: {lichthiFromForm.Thoigian}");
            Console.WriteLine($"DEBUG: - Phongthi: {lichthiFromForm.Phongthi}");

            if (id != lichthiFromForm.Id)
            {
                Console.WriteLine($"DEBUG: [LichthisController][Edit] - ID từ route ({id}) không khớp với ID từ form ({lichthiFromForm.Id}). Trả về NotFound.");
                return NotFound();
            }

            var existingLichthi = await _context.Lichthis.FindAsync(id);
            if (existingLichthi == null)
            {
                Console.WriteLine($"DEBUG: [LichthisController][Edit] - Không tìm thấy lịch thi gốc với ID: {id} trong DB.");
                return NotFound();
            }
            Console.WriteLine($"DEBUG: [LichthisController][Edit] - Đã tìm thấy lịch thi gốc ID: {existingLichthi.Id}.");

            // Kết hợp ngày và giờ
            if (TimeSpan.TryParse(gioThiPicker, out TimeSpan parsedTime))
            {
                existingLichthi.Ngaythi = ngayThiPicker.Date + parsedTime;
                Console.WriteLine($"DEBUG: [LichthisController][Edit] - Ngaythi gốc đã được cập nhật thành: {existingLichthi.Ngaythi.ToString("dd/MM/yyyy HH:mm:ss")}");

                // Tính toán ThoigianBatdau và ThoigianKetthuc
                existingLichthi.ThoigianBatdau = existingLichthi.Ngaythi; // Thời gian bắt đầu chính là Ngaythi đã được kết hợp
                if (lichthiFromForm.Thoigian.HasValue) // Sử dụng Thoigian từ form
                {
                    existingLichthi.Thoigian = lichthiFromForm.Thoigian; // Cập nhật Thoigian
                    existingLichthi.ThoigianKetthuc = existingLichthi.ThoigianBatdau.Value.AddMinutes(lichthiFromForm.Thoigian.Value);
                }
                else
                {
                    existingLichthi.Thoigian = null; // Cập nhật Thoigian là null nếu không có
                    existingLichthi.ThoigianKetthuc = null;
                }
                Console.WriteLine($"DEBUG: [LichthisController][Edit] - ThoigianBatdau: {existingLichthi.ThoigianBatdau?.ToString("dd/MM/yyyy HH:mm:ss") ?? "N/A"}");
                Console.WriteLine($"DEBUG: [LichthisController][Edit] - ThoigianKetthuc: {existingLichthi.ThoigianKetthuc?.ToString("dd/MM/yyyy HH:mm:ss") ?? "N/A"}");
            }
            else
            {
                ModelState.AddModelError(string.Empty, "Định dạng giờ thi không hợp lệ. Vui lòng nhập giờ theo định dạng HH:mm.");
                Console.WriteLine("ERROR: [LichthisController][Edit] - Lỗi định dạng giờ thi.");
            }

            // Xóa lỗi validation cho các navigation properties (nếu có và không được bind trực tiếp)
            if (ModelState.ContainsKey("Dethi")) ModelState.Remove("Dethi");
            if (ModelState.ContainsKey("Lophoc")) ModelState.Remove("Lophoc");
            if (ModelState.ContainsKey("Ketquathis")) ModelState.Remove("Ketquathis");
            if (ModelState.ContainsKey("LichthiSinhviens")) ModelState.Remove("LichthiSinhviens");


            if (ModelState.IsValid)
            {
                Console.WriteLine("DEBUG: [LichthisController][Edit] - ModelState.IsValid là TRUE. Tiến hành cập nhật.");
                try
                {
                    // Cập nhật các thuộc tính còn lại từ form
                    existingLichthi.Dethiid = lichthiFromForm.Dethiid;
                    existingLichthi.Lophocid = lichthiFromForm.Lophocid;
                    existingLichthi.Phongthi = lichthiFromForm.Phongthi;
                    // Thoigian đã được xử lý ở trên cùng với ThoigianKetthuc
                    Console.WriteLine($"DEBUG: [LichthisController][Edit] - Cập nhật các trường: Dethiid={existingLichthi.Dethiid}, Lophocid={existingLichthi.Lophocid}, Phongthi='{existingLichthi.Phongthi}', Thoigian={existingLichthi.Thoigian}");

                    await _context.SaveChangesAsync();
                    Console.WriteLine($"DEBUG: [LichthisController][Edit] - Lịch thi ID {existingLichthi.Id} đã được cập nhật thành công.");
                    TempData["SuccessMessage"] = "Lịch thi đã được cập nhật thành công!";
                    return RedirectToAction(nameof(Index));
                }
                catch (DbUpdateConcurrencyException)
                {
                    Console.WriteLine($"ERROR: [LichthisController][Edit] - Lỗi đồng thời khi cập nhật lịch thi ID: {id}.");
                    if (!LichthiExists(lichthiFromForm.Id))
                    {
                        TempData["ErrorMessage"] = "Lịch thi không tồn tại hoặc đã bị xóa bởi người khác.";
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
                    Console.WriteLine($"ERROR: [LichthisController][Edit] - Lỗi hệ thống khi cập nhật lịch thi ID: {id}: {ex.Message}");
                    TempData["ErrorMessage"] = $"Đã xảy ra lỗi hệ thống: {ex.Message}";
                }
            }
            else
            {
                Console.WriteLine("DEBUG: [LichthisController][Edit] - ModelState.IsValid là FALSE. Có lỗi validation.");
                var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList();
                if (errors.Any())
                {
                    Console.WriteLine("DEBUG: Các lỗi validation:");
                    foreach (var error in errors)
                    {
                        Console.WriteLine($"DEBUG: - {error}");
                    }
                    TempData["ErrorMessage"] = "Thông tin lịch thi không hợp lệ:<br/>" + string.Join("<br/>", errors);
                }
                else
                {
                    TempData["ErrorMessage"] = "Thông tin lịch thi không hợp lệ. Vui lòng kiểm tra lại.";
                }
            }

            // Re-populate SelectList nếu có lỗi
            ViewData["Dethiid"] = new SelectList(_context.Dethis.OrderBy(d => d.Tendethi), "Id", "Tendethi", existingLichthi.Dethiid); // Dùng existingLichthi.Dethiid
            ViewData["Lophocid"] = new SelectList(_context.Lophocs.OrderBy(l => l.Tenlop), "Id", "Tenlop", existingLichthi.Lophocid); // Dùng existingLichthi.Lophocid
            // Cần truyền lại giá trị ngày và giờ cho input type="date"/"time" nếu có lỗi
            ViewData["NgayThiPicker"] = ngayThiPicker.ToString("yyyy-MM-dd");
            ViewData["GioThiPicker"] = gioThiPicker;
            Console.WriteLine("DEBUG: [LichthisController][Edit] - ViewData đã được thiết lập lại. Trả về View với lỗi.");
            return View(existingLichthi); // Trả về existingLichthi để giữ lại các giá trị đã binding
        }

        // GET: Admin/Lichthis/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            Console.WriteLine($"DEBUG: [LichthisController][Delete] - Vào action Delete (GET). ID: {id ?? null}");
            if (id == null)
            {
                Console.WriteLine("DEBUG: [LichthisController][Delete] - ID xóa là null, trả về NotFound.");
                return NotFound();
            }

            var lichthi = await _context.Lichthis
                .Include(l => l.Dethi)
                .Include(l => l.Lophoc)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (lichthi == null)
            {
                Console.WriteLine($"DEBUG: [LichthisController][Delete] - Không tìm thấy lịch thi với ID: {id}");
                return NotFound();
            }
            Console.WriteLine($"DEBUG: [LichthisController][Delete] - Đã tìm thấy lịch thi để xóa: ID={lichthi.Id}, Đề thi: {lichthi.Dethi?.Tendethi}, Lớp: {lichthi.Lophoc?.Tenlop}, Ngày thi: {lichthi.Ngaythi}");
            return View(lichthi);
        }

        // POST: Admin/Lichthis/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            Console.WriteLine($"DEBUG: [LichthisController][DeleteConfirmed] - Vào action DeleteConfirmed (POST). ID: {id}");
            var lichthi = await _context.Lichthis.FindAsync(id);
            if (lichthi == null)
            {
                Console.WriteLine($"DEBUG: [LichthisController][DeleteConfirmed] - Lịch thi với ID {id} không tồn tại hoặc đã bị xóa.");
                TempData["ErrorMessage"] = "Lịch thi không tồn tại hoặc đã bị xóa trước đó.";
                return RedirectToAction(nameof(Index));
            }

            try
            {
                _context.Lichthis.Remove(lichthi);
                Console.WriteLine($"DEBUG: [LichthisController][DeleteConfirmed] - Đang xóa lịch thi ID: {lichthi.Id}.");
                await _context.SaveChangesAsync();
                Console.WriteLine($"DEBUG: [LichthisController][DeleteConfirmed] - Lịch thi với ID {id} đã được xóa thành công.");
                TempData["SuccessMessage"] = "Lịch thi đã được xóa thành công!";
            }
            catch (Exception ex)
            {
                Console.WriteLine($"ERROR: [LichthisController][DeleteConfirmed] - Lỗi khi xóa lịch thi: {ex.Message}");
                TempData["ErrorMessage"] = $"Đã xảy ra lỗi khi xóa lịch thi: {ex.Message}";
            }
            return RedirectToAction(nameof(Index));
        }

        private bool LichthiExists(int id)
        {
            Console.WriteLine($"DEBUG: [LichthisController][LichthiExists] - Kiểm tra tồn tại lịch thi với ID: {id}");
            return _context.Lichthis.Any(e => e.Id == id);
        }
    }
}
