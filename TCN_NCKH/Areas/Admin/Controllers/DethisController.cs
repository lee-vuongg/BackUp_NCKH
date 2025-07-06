using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using TCN_NCKH.Models.DBModel;
using TCN_NCKH.Models.ViewModels; // Thêm để sử dụng DethiListViewModel (và các ViewModels khác nếu có)
using System.Security.Claims; // Cần thiết để truy cập User.Identity.Claims

namespace TCN_NCKH.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class DethisController : Controller
    {
        private readonly NghienCuuKhoaHocContext _context;

        public DethisController(NghienCuuKhoaHocContext context)
        {
            _context = context;
        }

        // GET: Admin/Dethis
        public async Task<IActionResult> Index()
        {
            Console.WriteLine("DEBUG: [DethisController][Index] - Vào action Index.");
            // Đảm bảo Include NguoitaoNavigation và Cauhois để hiển thị đúng thông tin
            var nghienCuuKhoaHocContext = _context.Dethis
                .Include(d => d.Bocauhoi) // Để hiển thị tên bộ câu hỏi gốc (nếu cần)
                .Include(d => d.Monhoc)
                .Include(d => d.NguoitaoNavigation)
                .Include(d => d.Cauhois); // QUAN TRỌNG: Include Cauhois để đếm số lượng và hiển thị

            var dethisList = await nghienCuuKhoaHocContext.ToListAsync();
            Console.WriteLine($"DEBUG: [DethisController][Index] - Đã tải {dethisList.Count} đề thi.");
            foreach (var dt in dethisList)
            {
                Console.WriteLine($"DEBUG:   - Đề thi ID: {dt.Id}, Tên đề: {dt.Tendethi}, Người tạo (ID): {dt.Nguoitao ?? "N/A"}, Tên người tạo (Hoten): {dt.NguoitaoNavigation?.Hoten ?? "N/A"}, Số câu hỏi thực tế: {dt.Cauhois?.Count ?? 0}, Thời lượng: {dt.Thoiluongthi ?? 0} phút, Trạng thái: {dt.Trangthai ?? "N/A"}");
            }
            return View(dethisList);
        }

        // GET: Admin/Dethis/Details/5
        public async Task<IActionResult> Details(string id)
        {
            Console.WriteLine($"DEBUG: [DethisController][Details] - Vào action Details. ID: {id ?? "NULL"}");
            if (id == null)
            {
                TempData["ErrorMessage"] = "Không tìm thấy ID đề thi.";
                Console.WriteLine("DEBUG: [DethisController][Details] - ID chi tiết là null, trả về NotFound.");
                return NotFound();
            }

            var dethi = await _context.Dethis
                .Include(d => d.Bocauhoi)
                .Include(d => d.Monhoc)
                .Include(d => d.NguoitaoNavigation)
                .Include(d => d.Cauhois) // QUAN TRỌNG: Include Cauhois để có thể truy cập câu hỏi của đề thi này
                    .ThenInclude(ch => ch.Dapans) // Và đáp án của từng câu hỏi
                .FirstOrDefaultAsync(m => m.Id == id);

            if (dethi == null)
            {
                TempData["ErrorMessage"] = "Đề thi không tồn tại.";
                Console.WriteLine($"DEBUG: [DethisController][Details] - Không tìm thấy đề thi với ID: {id}. Chuyển hướng về Index.");
                return RedirectToAction(nameof(Index));
            }
            Console.WriteLine($"DEBUG: [DethisController][Details] - Đã tìm thấy đề thi: ID={dethi.Id}, Tên đề: {dethi.Tendethi}, Người tạo (ID): {dethi.Nguoitao ?? "N/A"}, Tên người tạo (Hoten): {dethi.NguoitaoNavigation?.Hoten ?? "N/A"}, Số câu hỏi thực tế: {dethi.Cauhois?.Count ?? 0}, Thời lượng: {dethi.Thoiluongthi ?? 0} phút, Trạng thái: {dethi.Trangthai ?? "N/A"}");
            if (dethi.Cauhois != null)
            {
                foreach (var ch in dethi.Cauhois)
                {
                    Console.WriteLine($"DEBUG:   - Câu hỏi ID: {ch.Id}, Nội dung: {ch.Noidung?.Substring(0, Math.Min(ch.Noidung.Length, 50))}..., Số đáp án: {ch.Dapans?.Count ?? 0}");
                    if (ch.Dapans != null)
                    {
                        foreach (var da in ch.Dapans)
                        {
                            Console.WriteLine($"DEBUG:     - Đáp án ID: {da.Id}, Nội dung: {da.Noidung?.Substring(0, Math.Min(da.Noidung.Length, 50))}..., Đúng: {da.Dung}");
                        }
                    }
                }
            }
            return View(dethi);
        }

        // GET: Admin/Dethis/Create
        public async Task<IActionResult> Create()
        {
            Console.WriteLine("DEBUG: [DethisController][Create] - Vào action Create (GET).");
            var viewModel = new DethiViewModel();
            await PopulateDropdowns(viewModel); // Tải dữ liệu cho dropdowns

            Console.WriteLine("DEBUG: [DethisController][Create] - ViewData cho Create (GET) đã được thiết lập.");
            return View(viewModel);
        }

        // POST: Admin/Dethis/Create
        // POST: Admin/Dethis/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(DethiViewModel viewModel)
        {
            Console.WriteLine("DEBUG: [DethisController][Create] - Vào action Create (POST).");
            Console.WriteLine($"DEBUG: Dữ liệu nhận từ form: Tendethi={viewModel.Tendethi}, Monhocid={viewModel.Monhocid}, Bocauhoiid={viewModel.Bocauhoiid}, Soluongcauhoi={viewModel.Soluongcauhoi}, ThoiLuongThi={viewModel.ThoiLuongThi}, TrangThai={viewModel.TrangThai}");

            ModelState.Remove("Monhoc");
            ModelState.Remove("Bocauhoi");
            ModelState.Remove("NguoitaoNavigation");
            ModelState.Remove("Nguoitao");
            ModelState.Remove("Ngaytao");

            if (ModelState.IsValid)
            {
                Console.WriteLine("DEBUG: [DethisController][Create] - ModelState.IsValid là TRUE. Tiến hành lưu dữ liệu.");
                try
                {
                    string currentUserClaimId = User.FindFirst("MaGiangVien")?.Value ?? User.FindFirst("MaQuanLy")?.Value;
                    if (string.IsNullOrEmpty(currentUserClaimId))
                    {
                        currentUserClaimId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                    }
                    if (string.IsNullOrEmpty(currentUserClaimId))
                    {
                        Console.WriteLine("WARNING: [Create] - Không tìm thấy ID người dùng từ bất kỳ Claims nào. Gán Nguoitao = 'AnonymousUser'.");
                        currentUserClaimId = "AnonymousUser";
                    }
                    Console.WriteLine($"DEBUG: [Create] - ID người dùng sẽ được gán cho Nguoitao: '{currentUserClaimId}'");

                    byte? selectedMucDoKhoBocauhoi = null; // Khởi tạo biến này

                    // =========================================================
                    // Logic kiểm tra Bộ câu hỏi và Số lượng câu hỏi (GIỮ NGUYÊN)
                    // =========================================================
                    if (!viewModel.Bocauhoiid.HasValue)
                    {
                        ModelState.AddModelError("Bocauhoiid", "Vui lòng chọn Bộ câu hỏi.");
                        Console.WriteLine($"ERROR: [Create] - Không có Bộ câu hỏi được chọn.");
                    }
                    else
                    {
                        var bocauhoi = await _context.Bocauhois.AsNoTracking().FirstOrDefaultAsync(b => b.Id == viewModel.Bocauhoiid.Value);
                        if (bocauhoi == null)
                        {
                            ModelState.AddModelError("Bocauhoiid", "Bộ câu hỏi được chọn không tồn tại.");
                            Console.WriteLine($"ERROR: [Create] - Bộ câu hỏi ID {viewModel.Bocauhoiid.Value} không tồn tại.");
                        }
                        else
                        {
                            selectedMucDoKhoBocauhoi = bocauhoi.Mucdokho; // Lấy mức độ khó từ Bộ câu hỏi
                            var totalQuestionsInBoCauHoi = await _context.Cauhois.CountAsync(ch => ch.Bocauhoiid == bocauhoi.Id);

                            if (viewModel.Soluongcauhoi <= 0)
                            {
                                ModelState.AddModelError("Soluongcauhoi", "Số lượng câu hỏi phải lớn hơn 0.");
                                Console.WriteLine($"ERROR: [Create] - Số lượng câu hỏi yêu cầu ({viewModel.Soluongcauhoi}) không hợp lệ.");
                            }
                            else if (viewModel.Soluongcauhoi > totalQuestionsInBoCauHoi)
                            {
                                ModelState.AddModelError("Soluongcauhoi", $"Số lượng câu hỏi yêu cầu ({viewModel.Soluongcauhoi}) lớn hơn số câu hỏi có sẵn trong bộ câu hỏi ({totalQuestionsInBoCauHoi}).");
                                Console.WriteLine($"ERROR: [Create] - Số lượng câu hỏi yêu cầu ({viewModel.Soluongcauhoi}) vượt quá số lượng có sẵn trong bộ câu hỏi ({totalQuestionsInBoCauHoi}).");
                            }
                        }
                    }

                    if (!ModelState.IsValid)
                    {
                        Console.WriteLine("DEBUG: [DethisController][Create] - ModelState.IsValid là FALSE sau kiểm tra số lượng/bộ câu hỏi. Trả về View với lỗi.");
                        await PopulateDropdowns(viewModel);
                        return View(viewModel);
                    }
                    // =========================================================
                    // Kết thúc logic kiểm tra Bộ câu hỏi và Số lượng câu hỏi
                    // =========================================================


                    var dethi = new Dethi
                    {
                        Id = Guid.NewGuid().ToString("N").Substring(0, 10),
                        Tendethi = viewModel.Tendethi,
                        Monhocid = viewModel.Monhocid,
                        Bocauhoiid = viewModel.Bocauhoiid,
                        Soluongcauhoi = viewModel.Soluongcauhoi,
                        DethikhacnhauCa = viewModel.DethikhacnhauCa,
                        Ngaytao = DateTime.Now,
                        Nguoitao = currentUserClaimId,
                        Thoiluongthi = viewModel.ThoiLuongThi,
                        Trangthai = viewModel.TrangThai ?? "Bản Nháp"
                    };
                    Console.WriteLine($"DEBUG: [Create] - dethi.Nguoitao được gán là: '{dethi.Nguoitao ?? "null"}'");
                    Console.WriteLine($"DEBUG: [Create] - Đề thi sẽ được tạo với Thời lượng: {dethi.Thoiluongthi} và Trạng thái: {dethi.Trangthai}");

                    // Gán mức độ khó vào Dethi từ mức độ khó của Bộ câu hỏi đã chọn (GIỮ NGUYÊN)
                    dethi.MucdokhoDe = (selectedMucDoKhoBocauhoi == 0);
                    dethi.MucdokhoTrungbinh = (selectedMucDoKhoBocauhoi == 1);
                    dethi.MucdokhoKho = (selectedMucDoKhoBocauhoi == 2);
                    Console.WriteLine($"DEBUG: Đề thi sẽ được lưu với: Dễ={dethi.MucdokhoDe}, TB={dethi.MucdokhoTrungbinh}, Khó={dethi.MucdokhoKho}");

                    _context.Add(dethi);
                    await _context.SaveChangesAsync();
                    Console.WriteLine($"DEBUG: [Create] - Đề thi gốc ID: {dethi.Id} đã được lưu thành công.");

                    // =========================================================
                    // Logic sao chép câu hỏi đã bị LOẠI BỎ ở đây
                    // dethi.Cauhois sẽ KHÔNG được populate trực tiếp từ bocauhoi
                    // Các câu hỏi sẽ được lấy từ Bocauhoi khi hiển thị chi tiết hoặc khi thi
                    // =========================================================

                    TempData["SuccessMessage"] = "Đề thi đã được tạo thành công!"; // Đổi lại thông báo
                    return RedirectToAction(nameof(Index));
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"ERROR: [DethisController][Create] - Lỗi khi lưu dữ liệu: {ex.Message}");
                    if (ex.InnerException != null)
                    {
                        Console.WriteLine($"ERROR: Inner Exception: {ex.InnerException.Message}");
                    }
                    TempData["ErrorMessage"] = $"Đã xảy ra lỗi khi tạo đề thi: {ex.Message}";
                }
            }
            else
            {
                Console.WriteLine("DEBUG: [DethisController][Create] - ModelState.IsValid là FALSE. Có lỗi validation.");
                foreach (var state in ModelState)
                {
                    if (state.Value.Errors.Count > 0)
                    {
                        Console.WriteLine($"DEBUG: Lỗi validation cho trường '{state.Key}':");
                        foreach (var error in state.Value.Errors)
                        {
                            Console.WriteLine($"DEBUG: - {error.ErrorMessage}");
                        }
                    }
                }
            }
            await PopulateDropdowns(viewModel);

            string validationErrors = string.Join("<br/>", ModelState.Values
                                                                 .SelectMany(v => v.Errors)
                                                                 .Select(e => e.ErrorMessage));
            if (!string.IsNullOrEmpty(validationErrors))
            {
                TempData["ErrorMessage"] = (TempData["ErrorMessage"] as string ?? "") + "<br/>" + validationErrors;
            }
            Console.WriteLine("DEBUG: [DethisController][Create] - Trả về View với lỗi.");
            return View(viewModel);
        }


        // GET: Admin/Dethis/Edit/5
        public async Task<IActionResult> Edit(string id)
        {
            Console.WriteLine($"DEBUG: [DethisController][Edit] - Vào action Edit (GET). ID: {id ?? "NULL"}");
            if (id == null)
            {
                TempData["ErrorMessage"] = "Không tìm thấy ID đề thi.";
                Console.WriteLine("DEBUG: [DethisController][Edit] - ID chỉnh sửa là null, trả về NotFound.");
                return NotFound();
            }

            var dethi = await _context.Dethis
                                    .AsNoTracking()
                                    .Include(d => d.NguoitaoNavigation)
                                    .Include(d => d.Bocauhoi) // Để hiển thị tên bộ câu hỏi gốc
                                    .Include(d => d.Monhoc) // Để hiển thị tên môn học
                                    .FirstOrDefaultAsync(m => m.Id == id);
            if (dethi == null)
            {
                TempData["ErrorMessage"] = "Đề thi không tồn tại.";
                Console.WriteLine($"DEBUG: [DethisController][Edit] - Không tìm thấy đề thi với ID: {id}. Chuyển hướng về Index.");
                return RedirectToAction(nameof(Index));
            }

            var viewModel = new DethiViewModel
            {
                Id = dethi.Id,
                Tendethi = dethi.Tendethi,
                Monhocid = dethi.Monhocid,
                Bocauhoiid = dethi.Bocauhoiid,
                Soluongcauhoi = dethi.Soluongcauhoi,
                DethikhacnhauCa = dethi.DethikhacnhauCa,
                ThoiLuongThi = dethi.Thoiluongthi, // GÁN THUỘC TÍNH MỚI
                TrangThai = dethi.Trangthai, // GÁN THUỘC TÍNH MỚI
                // Chuyển đổi 3 boolean thành 1 byte? để hiển thị trên form
                Mucdokho = dethi.MucdokhoDe ? (byte)0 : (dethi.MucdokhoTrungbinh ? (byte)1 : (dethi.MucdokhoKho ? (byte)2 : null))
            };
            await PopulateDropdowns(viewModel);
            Console.WriteLine($"DEBUG: [DethisController][Edit] - Đã tải ViewModel để chỉnh sửa đề thi: {dethi.Tendethi}, Người tạo (ID): {dethi.Nguoitao ?? "N/A"}, Tên người tạo (Hoten): {dethi.NguoitaoNavigation?.Hoten ?? "N/A"}, Thời lượng: {dethi.Thoiluongthi}, Trạng thái: {dethi.Trangthai}");
            Console.WriteLine("DEBUG: [DethisController][Edit] - ViewData cho Edit (GET) đã được thiết lập.");
            return View(viewModel);
        }

        // POST: Admin/Dethis/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(string id, DethiViewModel viewModel)
        {
            Console.WriteLine($"DEBUG: [DethisController][Edit] - Vào action Edit (POST). ID: {id}");
            Console.WriteLine($"DEBUG: Dữ liệu nhận từ form: Tendethi={viewModel.Tendethi}, Monhocid={viewModel.Monhocid}, Bocauhoiid={viewModel.Bocauhoiid}, Soluongcauhoi={viewModel.Soluongcauhoi}, Mucdokho={viewModel.Mucdokho}, ThoiLuongThi={viewModel.ThoiLuongThi}, TrangThai={viewModel.TrangThai}");

            if (id != viewModel.Id)
            {
                TempData["ErrorMessage"] = "ID đề thi không khớp.";
                Console.WriteLine($"DEBUG: [DethisController][Edit] - ID từ route ({id}) không khớp với ID từ form ({viewModel.Id}). Trả về NotFound.");
                return NotFound();
            }

            // Loại bỏ lỗi validation cho các navigation properties (nếu chúng gây lỗi)
            ModelState.Remove("Monhoc");
            ModelState.Remove("Bocauhoi");
            ModelState.Remove("NguoitaoNavigation");
            ModelState.Remove("Nguoitao"); // Loại bỏ Nguoitao khỏi ModelState
            ModelState.Remove("Ngaytao"); // Loại bỏ Ngaytao khỏi ModelState


            if (ModelState.IsValid)
            {
                Console.WriteLine("DEBUG: [DethisController][Edit] - ModelState.IsValid là TRUE. Tiến hành cập nhật dữ liệu.");
                try
                {
                    var dethiToUpdate = await _context.Dethis.FindAsync(id); // FindAsync sẽ theo dõi entity
                    if (dethiToUpdate == null)
                    {
                        TempData["ErrorMessage"] = "Đề thi không tồn tại hoặc đã bị xóa.";
                        Console.WriteLine($"DEBUG: [DethisController][Edit] - Không tìm thấy đề thi hiện có với ID: {id} trong DB.");
                        return NotFound();
                    }
                    // Thêm debug log cho Nguoitao cũ
                    Console.WriteLine($"DEBUG: [DethisController][Edit] - Đã tìm thấy đề thi gốc ID: {dethiToUpdate.Id}. Người tạo cũ (ID): {dethiToUpdate.Nguoitao ?? "N/A"}");


                    // Cập nhật các thuộc tính từ ViewModel vào entity đang được theo dõi
                    dethiToUpdate.Tendethi = viewModel.Tendethi;
                    dethiToUpdate.Monhocid = viewModel.Monhocid;

                    // Xử lý cập nhật Bocauhoiid và số lượng câu hỏi, có thể cần logic phức tạp hơn
                    // Tạm thời chỉ cập nhật Bocauhoiid và Soluongcauhoi nếu người dùng thay đổi trên form
                    // Nếu mi muốn thay đổi Bocauhoiid sẽ tự động tạo lại câu hỏi, cần logic tương tự như Create
                    if (dethiToUpdate.Bocauhoiid != viewModel.Bocauhoiid)
                    {
                        Console.WriteLine($"DEBUG: [Edit] - Bocauhoiid đã thay đổi từ {dethiToUpdate.Bocauhoiid} sang {viewModel.Bocauhoiid}. Cần xử lý lại câu hỏi.");
                        // TODO: Implement logic to delete existing questions for this Dethi
                        // Then re-copy questions from the new Bocauhoiid, similar to Create action.
                        // For simplicity, for now, we'll just update the Bocauhoiid and Soluongcauhoi.
                        // This might lead to inconsistency if old questions are still linked.
                    }
                    dethiToUpdate.Bocauhoiid = viewModel.Bocauhoiid;
                    dethiToUpdate.Soluongcauhoi = viewModel.Soluongcauhoi;
                    dethiToUpdate.DethikhacnhauCa = viewModel.DethikhacnhauCa;
                    dethiToUpdate.Thoiluongthi = viewModel.ThoiLuongThi; // CẬP NHẬT THUỘC TÍNH MỚI
                    dethiToUpdate.Trangthai = viewModel.TrangThai; // CẬP NHẬT THUỘC TÍNH MỚI

                    // Nguoitao và Ngaytao KHÔNG được cập nhật ở đây để giữ nguyên người tạo và ngày tạo ban đầu
                    Console.WriteLine($"DEBUG: [Edit] - Đề thi đã được cập nhật dữ liệu từ ViewModel.");

                    // Lấy thông tin mức độ khó từ Bộ câu hỏi được chọn
                    byte? selectedMucDoKhoBocauhoi = null;
                    if (viewModel.Bocauhoiid.HasValue)
                    {
                        var bocauhoi = await _context.Bocauhois
                                                     .AsNoTracking()
                                                     .FirstOrDefaultAsync(b => b.Id == viewModel.Bocauhoiid.Value);
                        if (bocauhoi != null)
                        {
                            selectedMucDoKhoBocauhoi = bocauhoi.Mucdokho;
                            Console.WriteLine($"DEBUG: Mức độ khó của Bộ câu hỏi được chọn (Edit): {selectedMucDoKhoBocauhoi}");
                        }
                        else
                        {
                            Console.WriteLine($"WARNING: Không tìm thấy Bộ câu hỏi với ID: {viewModel.Bocauhoiid.Value} khi chỉnh sửa.");
                        }
                    }

                    // Gán lại 3 thuộc tính boolean mức độ khó dựa trên Mucdokho của Bộ câu hỏi
                    dethiToUpdate.MucdokhoDe = (selectedMucDoKhoBocauhoi == 0);
                    dethiToUpdate.MucdokhoTrungbinh = (selectedMucDoKhoBocauhoi == 1);
                    dethiToUpdate.MucdokhoKho = (selectedMucDoKhoBocauhoi == 2);
                    Console.WriteLine($"DEBUG: Đề thi sẽ được cập nhật với: Dễ={dethiToUpdate.MucdokhoDe}, TB={dethiToUpdate.MucdokhoTrungbinh}, Khó={dethiToUpdate.MucdokhoKho}");


                    await _context.SaveChangesAsync();
                    Console.WriteLine($"DEBUG: [DethisController][Edit] - Đề thi ID {dethiToUpdate.Id} đã được cập nhật thành công.");
                    Console.WriteLine($"DEBUG: [DethisController][Edit] - Người tạo đã lưu trong DB cho đề thi này: {dethiToUpdate.Nguoitao ?? "N/A"}");
                    Console.WriteLine($"DEBUG: [DethisController][Edit] - Thời lượng: {dethiToUpdate.Thoiluongthi}, Trạng thái: {dethiToUpdate.Trangthai}");
                    TempData["SuccessMessage"] = "Đề thi đã được cập nhật thành công!";
                    return RedirectToAction(nameof(Index));
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!DethiExists(viewModel.Id))
                    {
                        Console.WriteLine($"ERROR: [DethisController][Edit] - Lỗi đồng thời: Đề thi ID {viewModel.Id} không tồn tại.");
                        TempData["ErrorMessage"] = "Đề thi không tồn tại hoặc đã bị xóa bởi người khác.";
                        return NotFound();
                    }
                    else
                    {
                        Console.WriteLine($"ERROR: [DethisController][Edit] - Lỗi đồng thời không xác định khi cập nhật đề thi ID {viewModel.Id}.");
                        TempData["ErrorMessage"] = "Đã xảy ra lỗi đồng thời khi cập nhật dữ liệu. Vui lòng thử lại.";
                        throw;
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"ERROR: [DethisController][Edit] - Lỗi khi cập nhật đề thi: {ex.Message}");
                    if (ex.InnerException != null)
                    {
                        Console.WriteLine($"ERROR: Inner Exception: {ex.InnerException.Message}");
                    }
                    TempData["ErrorMessage"] = $"Đã xảy ra lỗi hệ thống: {ex.Message}";
                }
            }
            else
            {
                Console.WriteLine("DEBUG: [DethisController][Edit] - ModelState.IsValid là FALSE. Có lỗi validation khi cập nhật.");
                foreach (var state in ModelState)
                {
                    if (state.Value.Errors.Count > 0)
                    {
                        Console.WriteLine($"DEBUG: Lỗi validation cho trường '{state.Key}':");
                        foreach (var error in state.Value.Errors)
                        {
                            Console.WriteLine($"DEBUG: - {error.ErrorMessage}");
                        }
                    }
                }
            }
            await PopulateDropdowns(viewModel); // Tải lại dữ liệu cho dropdowns nếu có lỗi

            string validationErrors = string.Join("<br/>", ModelState.Values
                                                            .SelectMany(v => v.Errors)
                                                            .Select(e => e.ErrorMessage));
            if (!string.IsNullOrEmpty(validationErrors))
            {
                TempData["ErrorMessage"] = (TempData["ErrorMessage"] as string ?? "") + "<br/>" + validationErrors;
            }
            Console.WriteLine("DEBUG: [DethisController][Edit] - Trả về View với lỗi.");
            return View(viewModel); // Trả về ViewModel
        }

        // GET: Admin/Dethis/Delete/5
        public async Task<IActionResult> Delete(string id)
        {
            Console.WriteLine($"DEBUG: [DethisController][Delete] - Vào action Delete (GET). ID: {id ?? "NULL"}");
            if (id == null)
            {
                TempData["ErrorMessage"] = "Không tìm thấy ID đề thi.";
                Console.WriteLine("DEBUG: [DethisController][Delete] - ID xóa là null, trả về NotFound.");
                return NotFound();
            }

            var dethi = await _context.Dethis
                .Include(d => d.Bocauhoi)
                .Include(d => d.Monhoc)
                .Include(d => d.NguoitaoNavigation)
                .Include(d => d.Cauhois) // Include Cauhois để hiển thị thông tin khi xóa
                    .ThenInclude(ch => ch.Dapans) // Include Dapans
                .FirstOrDefaultAsync(m => m.Id == id);

            if (dethi == null)
            {
                TempData["ErrorMessage"] = "Đề thi không tồn tại.";
                Console.WriteLine($"DEBUG: [DethisController][Delete] - Không tìm thấy đề thi với ID: {id}. Chuyển hướng về Index.");
                return RedirectToAction(nameof(Index));
            }
            Console.WriteLine($"DEBUG: [DethisController][Delete] - Đã tìm thấy đề thi để xóa: ID={dethi.Id}, Tên đề: {dethi.Tendethi}, Người tạo (ID): {dethi.Nguoitao ?? "N/A"}, Tên người tạo (Hoten): {dethi.NguoitaoNavigation?.Hoten ?? "N/A"}, Số câu hỏi thực tế: {dethi.Cauhois?.Count ?? 0}, Thời lượng: {dethi.Thoiluongthi ?? 0} phút, Trạng thái: {dethi.Trangthai ?? "N/A"}");
            return View(dethi);
        }

        // POST: Admin/Dethis/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(string id)
        {
            Console.WriteLine($"DEBUG: [DethisController][DeleteConfirmed] - Vào action DeleteConfirmed (POST). ID: {id}");
            // First, load the Dethi with its related Cauhois and Dapans to ensure they are deleted as well
            var dethi = await _context.Dethis
                                      .Include(d => d.Cauhois)
                                          .ThenInclude(ch => ch.Dapans)
                                      .FirstOrDefaultAsync(d => d.Id == id);

            if (dethi == null)
            {
                TempData["ErrorMessage"] = "Đề thi không tồn tại hoặc đã bị xóa.";
                Console.WriteLine($"DEBUG: [DethisController][DeleteConfirmed] - Không tìm thấy đề thi ID {id} để xóa.");
                return RedirectToAction(nameof(Index));
            }

            try
            {
                // EF Core sẽ tự động xóa các Cauhoi và Dapan liên quan nếu cấu hình cascade delete là đúng
                // trên database hoặc trong cấu hình Fluent API của mi.
                // Nếu không, mi cần xóa thủ công các câu hỏi và đáp án trước:
                // _context.Dapans.RemoveRange(dethi.Cauhois.SelectMany(ch => ch.Dapans));
                // _context.Cauhois.RemoveRange(dethi.Cauhois);

                _context.Dethis.Remove(dethi);
                await _context.SaveChangesAsync();
                Console.WriteLine($"DEBUG: [DethisController][DeleteConfirmed] - Đề thi ID {id} đã được xóa thành công.");
                TempData["SuccessMessage"] = "Đề thi đã được xóa thành công!";
            }
            catch (Exception ex)
            {
                Console.WriteLine($"ERROR: [DethisController][DeleteConfirmed] - Lỗi khi xóa đề thi: {ex.Message}");
                if (ex.InnerException != null)
                {
                    Console.WriteLine($"ERROR: Inner Exception: {ex.InnerException.Message}");
                }
                TempData["ErrorMessage"] = $"Đã xảy ra lỗi khi xóa đề thi: {ex.Message}";
            }
            return RedirectToAction(nameof(Index));
        }

        private bool DethiExists(string id)
        {
            Console.WriteLine($"DEBUG: [DethisController][DethiExists] - Kiểm tra tồn tại đề thi với ID: {id}");
            return _context.Dethis.Any(e => e.Id == id);
        }

        [HttpGet]
        public async Task<IActionResult> GetBocauhoiMucdokho(int bocauhoiId)
        {
            Console.WriteLine($"DEBUG: [DethisController][GetBocauhoiMucdokho] - Vào action GetBocauhoiMucdokho. BocauhoiId: {bocauhoiId}");
            var bocauhoi = await _context.Bocauhois
                                         .AsNoTracking()
                                         .Select(b => new { b.Id, b.Mucdokho }) // Chỉ chọn những gì cần thiết
                                         .FirstOrDefaultAsync(b => b.Id == bocauhoiId);

            if (bocauhoi != null)
            {
                Console.WriteLine($"DEBUG: [DethisController][GetBocauhoiMucdokho] - Tìm thấy Bộ câu hỏi ID {bocauhoiId}, Mức độ khó: {bocauhoi.Mucdokho}");
                return Json(new { mucDoKho = bocauhoi.Mucdokho });
            }
            Console.WriteLine($"DEBUG: [DethisController][GetBocauhoiMucdokho] - Không tìm thấy Bộ câu hỏi với ID: {bocauhoiId}");
            return NotFound(); // Trả về 404 nếu không tìm thấy
        }

        // Helper method để populate dropdowns trong DethiViewModel
        private async Task PopulateDropdowns(DethiViewModel viewModel)
        {
            Console.WriteLine("DEBUG: [DethisController][PopulateDropdowns] - Bắt đầu populate dropdowns.");
            viewModel.MonhocList = await _context.Monhocs
                                                 .OrderBy(m => m.Tenmon)
                                                 .Select(m => new SelectListItem { Value = m.Id, Text = m.Tenmon })
                                                 .ToListAsync();

            viewModel.BocauhoiList = await _context.Bocauhois
                                                     .OrderBy(b => b.Tenbocauhoi)
                                                     .Select(b => new SelectListItem { Value = b.Id.ToString(), Text = b.Tenbocauhoi })
                                                     .ToListAsync();

            // Populate danh sách trạng thái cho dropdown
            viewModel.TrangThaiList = new List<SelectListItem>
            {
                new SelectListItem { Value = "Draft", Text = "Bản nháp" },
                new SelectListItem { Value = "Ready", Text = "Sẵn sàng" },
                new SelectListItem { Value = "Active", Text = "Đang diễn ra" },
                new SelectListItem { Value = "Finished", Text = "Đã kết thúc" }
            };

            Console.WriteLine("DEBUG: [DethisController][PopulateDropdowns] - Dropdowns cho DethiViewModel đã được populate xong.");
        }
    }
}
