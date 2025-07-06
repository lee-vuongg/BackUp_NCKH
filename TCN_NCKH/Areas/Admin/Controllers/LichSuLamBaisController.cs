using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using TCN_NCKH.Models.DBModel;
using TCN_NCKH.Models.ViewModels; // Thêm namespace của ViewModel

namespace TCN_NCKH.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class LichSuLamBaisController : Controller
    {
        private readonly NghienCuuKhoaHocContext _context;

        public LichSuLamBaisController(NghienCuuKhoaHocContext context)
        {
            _context = context;
        }

        // Helper method to get SinhVien SelectList
        private async Task<SelectList> GetSinhVienSelectList(string? selectedValue = null)
        {
            var sinhVienList = await _context.Sinhviens
                                             .Include(sv => sv.SinhvienNavigation)
                                             .Where(sv => sv.SinhvienNavigation != null && sv.SinhvienNavigation.Hoten != null)
                                             .OrderBy(s => s.SinhvienNavigation.Hoten)
                                             .ToListAsync();

            return new SelectList(
                sinhVienList,
                "Sinhvienid", // Đã sửa thành Sinhvienid để khớp với FK và giá trị lưu trong LichSuLamBai.MaSinhVien
                "SinhvienNavigation.Hoten",
                selectedValue
            );
        }

        // Helper method to get DeThi SelectList
        private async Task<SelectList> GetDeThiSelectList(string? selectedValue = null)
        {
            return new SelectList(
                await _context.Dethis.OrderBy(d => d.Tendethi).ToListAsync(),
                "Id",
                "Tendethi",
                selectedValue
            );
        }

        // Helper method to get LichThi SelectList
        private async Task<SelectList> GetLichThiSelectList(int? selectedValue = null)
        {
            var lichThiData = await _context.Lichthis
                .Include(lt => lt.Dethi)
                .Include(lt => lt.Lophoc)
                .OrderBy(lt => lt.Ngaythi)
                .Select(lt => new
                {
                    lt.Id,
                    DisplayText = $"{lt.Ngaythi.ToString("dd/MM/yyyy HH:mm")} - Phòng: {lt.Phongthi ?? "N/A"} - Đề: {lt.Dethi.Tendethi ?? "N/A"} - Lớp: {lt.Lophoc.Tenlop ?? "N/A"}"
                })
                .ToListAsync();

            return new SelectList(
                lichThiData,
                "Id",
                "DisplayText",
                selectedValue
            );
        }


        // GET: Admin/LichSuLamBais
        public async Task<IActionResult> Index()
        {
            var lichSuLamBaisQuery = _context.LichSuLamBais
                .Include(l => l.LichThi).ThenInclude(lt => lt.Dethi)
                .Include(l => l.LichThi).ThenInclude(lt => lt.Lophoc)
                .Include(l => l.MaDeThiNavigation)
                .Include(l => l.MaSinhVienNavigation).ThenInclude(sv => sv.SinhvienNavigation);

            var lichSuLamBais = await lichSuLamBaisQuery.ToListAsync();

            var viewModelList = lichSuLamBais.Select(l => new LichSuLamBaiDetailsViewModel
            {
                Id = l.Id,
                TenSinhVien = l.MaSinhVienNavigation?.SinhvienNavigation?.Hoten ?? "N/A",
                TenDeThi = l.MaDeThiNavigation?.Tendethi ?? "N/A",
                ThongTinLichThi = $"{l.LichThi?.Ngaythi.ToString("dd/MM/yyyy HH:mm")} - Phòng: {l.LichThi?.Phongthi ?? "N/A"} - Đề: {l.LichThi?.Dethi?.Tendethi ?? "N/A"} - Lớp: {l.LichThi?.Lophoc?.Tenlop ?? "N/A"}",
                ThoiDiemBatDau = l.ThoiDiemBatDau,
                ThoiDiemKetThuc = l.ThoiDiemKetThuc,
                SoLanRoiManHinh = l.SoLanRoiManHinh ?? 0,
                TrangThaiNopBai = l.TrangThaiNopBai,
                Ipaddress = l.Ipaddress,
                TongDiemDatDuoc = l.TongDiemDatDuoc,
                DaChamDiem = l.DaChamDiem ?? false
            }).ToList();

            return View(viewModelList);
        }

        // GET: Admin/LichSuLamBais/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                TempData["ErrorMessage"] = "Không tìm thấy lịch sử làm bài.";
                return NotFound();
            }

            var lichSuLamBai = await _context.LichSuLamBais
                .Include(l => l.LichThi).ThenInclude(lt => lt.Dethi)
                .Include(l => l.LichThi).ThenInclude(lt => lt.Lophoc)
                .Include(l => l.MaDeThiNavigation)
                .Include(l => l.MaSinhVienNavigation).ThenInclude(sv => sv.SinhvienNavigation)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (lichSuLamBai == null)
            {
                TempData["ErrorMessage"] = "Không tìm thấy lịch sử làm bài.";
                return NotFound();
            }

            var viewModel = new LichSuLamBaiDetailsViewModel
            {
                Id = lichSuLamBai.Id,
                TenSinhVien = lichSuLamBai.MaSinhVienNavigation?.SinhvienNavigation?.Hoten ?? "N/A",
                TenDeThi = lichSuLamBai.MaDeThiNavigation?.Tendethi ?? "N/A",
                ThongTinLichThi = $"{lichSuLamBai.LichThi?.Ngaythi.ToString("dd/MM/yyyy HH:mm")} - Phòng: {lichSuLamBai.LichThi?.Phongthi ?? "N/A"} - Đề: {lichSuLamBai.LichThi?.Dethi?.Tendethi ?? "N/A"} - Lớp: {lichSuLamBai.LichThi?.Lophoc?.Tenlop ?? "N/A"}",
                ThoiDiemBatDau = lichSuLamBai.ThoiDiemBatDau,
                ThoiDiemKetThuc = lichSuLamBai.ThoiDiemKetThuc,
                SoLanRoiManHinh = lichSuLamBai.SoLanRoiManHinh ?? 0,
                TrangThaiNopBai = lichSuLamBai.TrangThaiNopBai,
                Ipaddress = lichSuLamBai.Ipaddress,
                TongDiemDatDuoc = lichSuLamBai.TongDiemDatDuoc,
                DaChamDiem = lichSuLamBai.DaChamDiem ?? false
            };

            return View(viewModel);
        }

        // GET: Admin/LichSuLamBais/Create
        public async Task<IActionResult> Create()
        {
            var viewModel = new LichSuLamBaiViewModel
            {
                SinhVienList = await GetSinhVienSelectList(),
                DeThiList = await GetDeThiSelectList(),
                LichThiList = await GetLichThiSelectList()
            };
            return View(viewModel);
        }

        // POST: Admin/LichSuLamBais/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(LichSuLamBaiViewModel viewModel)
        {
            if (ModelState.IsValid)
            {
                var lichSuLamBai = new LichSuLamBai
                {
                    MaSinhVien = viewModel.MaSinhVien,
                    MaDeThi = viewModel.MaDeThi,
                    LichThiId = viewModel.LichThiId,
                    ThoiDiemBatDau = viewModel.ThoiDiemBatDau,
                    ThoiDiemKetThuc = viewModel.ThoiDiemKetThuc,
                    SoLanRoiManHinh = viewModel.SoLanRoiManHinh,
                    TrangThaiNopBai = viewModel.TrangThaiNopBai,
                    Ipaddress = viewModel.Ipaddress,
                    TongDiemDatDuoc = viewModel.TongDiemDatDuoc,
                    DaChamDiem = viewModel.DaChamDiem
                };

                _context.Add(lichSuLamBai);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Tạo lịch sử làm bài thành công!";
                return RedirectToAction(nameof(Index));
            }

            viewModel.SinhVienList = await GetSinhVienSelectList(viewModel.MaSinhVien);
            viewModel.DeThiList = await GetDeThiSelectList(viewModel.MaDeThi);
            viewModel.LichThiList = await GetLichThiSelectList(viewModel.LichThiId);

            TempData["ErrorMessage"] = "Có lỗi xảy ra khi tạo lịch sử làm bài. Vui lòng kiểm tra lại thông tin.";
            return View(viewModel);
        }

        // GET: Admin/LichSuLamBais/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            Console.WriteLine($"DEBUG: Vào action Edit của LichSuLamBaisController cho ID: {id}");

            if (id == null)
            {
                Console.WriteLine("DEBUG: ID là null. Trả về NotFound.");
                TempData["ErrorMessage"] = "Không tìm thấy lịch sử làm bài để chỉnh sửa.";
                return NotFound();
            }

            var lichSuLamBai = await _context.LichSuLamBais.FindAsync(id);

            if (lichSuLamBai == null)
            {
                Console.WriteLine($"DEBUG: Không tìm thấy bản ghi LichSuLamBai với ID: {id}. Trả về NotFound.");
                TempData["ErrorMessage"] = "Không tìm thấy lịch sử làm bài để chỉnh sửa.";
                return NotFound();
            }

            Console.WriteLine($"DEBUG: Đã tìm thấy LichSuLamBai. ID: {lichSuLamBai.Id}");
            Console.WriteLine($"DEBUG: ThoiDiemBatDau từ DB: {lichSuLamBai.ThoiDiemBatDau}");
            Console.WriteLine($"DEBUG: ThoiDiemKetThuc từ DB: {lichSuLamBai.ThoiDiemKetThuc}");
            Console.WriteLine($"DEBUG: MaSinhVien từ DB: '{lichSuLamBai.MaSinhVien?.Trim()}'");

            // Map DB model to ViewModel
            var viewModel = new LichSuLamBaiViewModel
            {
                Id = lichSuLamBai.Id,
                MaSinhVien = lichSuLamBai.MaSinhVien,
                MaDeThi = lichSuLamBai.MaDeThi,
                LichThiId = lichSuLamBai.LichThiId,
                ThoiDiemBatDau = lichSuLamBai.ThoiDiemBatDau,
                ThoiDiemKetThuc = lichSuLamBai.ThoiDiemKetThuc,
                SoLanRoiManHinh = lichSuLamBai.SoLanRoiManHinh ?? 0,
                TrangThaiNopBai = lichSuLamBai.TrangThaiNopBai,
                Ipaddress = lichSuLamBai.Ipaddress,
                TongDiemDatDuoc = lichSuLamBai.TongDiemDatDuoc,
                DaChamDiem = lichSuLamBai.DaChamDiem ?? false,

                // Khởi tạo SelectList với giá trị đã chọn
                SinhVienList = await GetSinhVienSelectList(lichSuLamBai.MaSinhVien),
                DeThiList = await GetDeThiSelectList(lichSuLamBai.MaDeThi),
                LichThiList = await GetLichThiSelectList(lichSuLamBai.LichThiId)
            };

            Console.WriteLine($"DEBUG: ViewModel ThoiDiemBatDau: {viewModel.ThoiDiemBatDau}");
            Console.WriteLine($"DEBUG: ViewModel ThoiDiemKetThuc: {viewModel.ThoiDiemKetThuc}");

            return View(viewModel);
        }

        // POST: Admin/LichSuLamBais/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, LichSuLamBaiViewModel viewModel)
        {
            if (id != viewModel.Id)
            {
                TempData["ErrorMessage"] = "ID lịch sử làm bài không khớp.";
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    var lichSuLamBai = await _context.LichSuLamBais.FindAsync(id);
                    if (lichSuLamBai == null)
                    {
                        TempData["ErrorMessage"] = "Không tìm thấy lịch sử làm bài để cập nhật.";
                        return NotFound();
                    }

                    // Update properties from ViewModel to DB model
                    lichSuLamBai.MaSinhVien = viewModel.MaSinhVien;
                    lichSuLamBai.MaDeThi = viewModel.MaDeThi;
                    lichSuLamBai.LichThiId = viewModel.LichThiId;
                    lichSuLamBai.ThoiDiemBatDau = viewModel.ThoiDiemBatDau;
                    lichSuLamBai.ThoiDiemKetThuc = viewModel.ThoiDiemKetThuc;
                    lichSuLamBai.SoLanRoiManHinh = viewModel.SoLanRoiManHinh;
                    lichSuLamBai.TrangThaiNopBai = viewModel.TrangThaiNopBai;
                    lichSuLamBai.Ipaddress = viewModel.Ipaddress;
                    lichSuLamBai.TongDiemDatDuoc = viewModel.TongDiemDatDuoc;
                    lichSuLamBai.DaChamDiem = viewModel.DaChamDiem;

                    _context.Update(lichSuLamBai);
                    await _context.SaveChangesAsync();
                    TempData["SuccessMessage"] = "Cập nhật lịch sử làm bài thành công!";
                    return RedirectToAction(nameof(Index));
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!LichSuLamBaiExists(viewModel.Id))
                    {
                        TempData["ErrorMessage"] = "Lịch sử làm bài không còn tồn tại.";
                        return NotFound();
                    }
                    else
                    {
                        throw; // Xử lý lỗi đồng thời phức tạp hơn nếu cần
                    }
                }
                catch (Exception ex)
                {
                    TempData["ErrorMessage"] = $"Lỗi khi cập nhật lịch sử làm bài: {ex.Message}";
                }
            }

            viewModel.SinhVienList = await GetSinhVienSelectList(viewModel.MaSinhVien);
            viewModel.DeThiList = await GetDeThiSelectList(viewModel.MaDeThi);
            viewModel.LichThiList = await GetLichThiSelectList(viewModel.LichThiId);

            TempData["ErrorMessage"] = "Có lỗi xảy ra khi cập nhật lịch sử làm bài. Vui lòng kiểm tra lại thông tin.";
            return View(viewModel);
        }

        // GET: Admin/LichSuLamBais/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                TempData["ErrorMessage"] = "Không tìm thấy lịch sử làm bài để xóa.";
                return NotFound();
            }

            var lichSuLamBai = await _context.LichSuLamBais
                .Include(l => l.LichThi).ThenInclude(lt => lt.Dethi)
                .Include(l => l.LichThi).ThenInclude(lt => lt.Lophoc)
                .Include(l => l.MaDeThiNavigation)
                .Include(l => l.MaSinhVienNavigation).ThenInclude(sv => sv.SinhvienNavigation)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (lichSuLamBai == null)
            {
                TempData["ErrorMessage"] = "Không tìm thấy lịch sử làm bài để xóa.";
                return NotFound();
            }

            var viewModel = new LichSuLamBaiDetailsViewModel
            {
                Id = lichSuLamBai.Id,
                TenSinhVien = lichSuLamBai.MaSinhVienNavigation?.SinhvienNavigation?.Hoten ?? "N/A",
                TenDeThi = lichSuLamBai.MaDeThiNavigation?.Tendethi ?? "N/A",
                ThongTinLichThi = $"{lichSuLamBai.LichThi?.Ngaythi.ToString("dd/MM/yyyy HH:mm")} - Phòng: {lichSuLamBai.LichThi?.Phongthi ?? "N/A"} - Đề: {lichSuLamBai.LichThi?.Dethi?.Tendethi ?? "N/A"} - Lớp: {lichSuLamBai.LichThi?.Lophoc?.Tenlop ?? "N/A"}",
                ThoiDiemBatDau = lichSuLamBai.ThoiDiemBatDau,
                ThoiDiemKetThuc = lichSuLamBai.ThoiDiemKetThuc,
                SoLanRoiManHinh = lichSuLamBai.SoLanRoiManHinh ?? 0,
                TrangThaiNopBai = lichSuLamBai.TrangThaiNopBai,
                Ipaddress = lichSuLamBai.Ipaddress,
                TongDiemDatDuoc = lichSuLamBai.TongDiemDatDuoc,
                DaChamDiem = lichSuLamBai.DaChamDiem ?? false
            };

            return View(viewModel);
        }

        private bool LichSuLamBaiExists(int id)
        {
            return _context.LichSuLamBais.Any(e => e.Id == id);
        }
    }
}
