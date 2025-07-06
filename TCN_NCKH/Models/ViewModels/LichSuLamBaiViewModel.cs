using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering; // Dùng cho SelectList

namespace TCN_NCKH.Models.ViewModels
{
    public class LichSuLamBaiViewModel
    {
        public int Id { get; set; }

        [Display(Name = "Mã Sinh Viên")]
        [Required(ErrorMessage = "Vui lòng chọn sinh viên.")]
        public string MaSinhVien { get; set; }

        [Display(Name = "Mã Đề Thi")]
        [Required(ErrorMessage = "Vui lòng chọn đề thi.")]
        public string? MaDeThi { get; set; }

        [Display(Name = "Lịch Thi")]
        [Required(ErrorMessage = "Vui lòng chọn lịch thi.")]
        public int LichThiId { get; set; }

        [Display(Name = "Thời Điểm Bắt Đầu")]
        [DataType(DataType.DateTime)]
        [DisplayFormat(DataFormatString = "{0:dd/MM/yyyy HH:mm}", ApplyFormatInEditMode = true)]
        public DateTime? ThoiDiemBatDau { get; set; }

        [Display(Name = "Thời Điểm Kết Thúc")]
        [DataType(DataType.DateTime)]
        [DisplayFormat(DataFormatString = "{0:dd/MM/yyyy HH:mm}", ApplyFormatInEditMode = true)]
        public DateTime? ThoiDiemKetThuc { get; set; }

        [Display(Name = "Số Lần Rời Màn Hình")]
        [Range(0, int.MaxValue, ErrorMessage = "Số lần rời màn hình không hợp lệ.")]
        public int? SoLanRoiManHinh { get; set; }

        [Display(Name = "Trạng Thái Nộp Bài")]
        [Required(ErrorMessage = "Vui lòng nhập trạng thái nộp bài.")]
        [StringLength(50, ErrorMessage = "Trạng thái không được vượt quá 50 ký tự.")]
        public string? TrangThaiNopBai { get; set; }

        [Display(Name = "Địa Chỉ IP")]
        [StringLength(45, ErrorMessage = "Địa chỉ IP không được vượt quá 45 ký tự.")]
        public string? Ipaddress { get; set; }

        [Display(Name = "Tổng Điểm Đạt Được")]
        [Range(0.0, 100.0, ErrorMessage = "Điểm phải nằm trong khoảng từ 0 đến 10.")]
        public double? TongDiemDatDuoc { get; set; }

        [Display(Name = "Đã Chấm Điểm")]
        public bool? DaChamDiem { get; set; }

        // Các thuộc tính cho SelectList
        public SelectList SinhVienList { get; set; }
        public SelectList DeThiList { get; set; }
        public SelectList LichThiList { get; set; }
    }

    public class LichSuLamBaiDetailsViewModel
    {
        public int Id { get; set; }

        [DisplayName("Thời Điểm Bắt Đầu")]
        [DisplayFormat(DataFormatString = "{0:dd/MM/yyyy HH:mm}", ApplyFormatInEditMode = true)]
        public DateTime? ThoiDiemBatDau { get; set; } // Đã sửa thành DateTime?

        [DisplayName("Thời Điểm Kết Thúc")]
        [DisplayFormat(DataFormatString = "{0:dd/MM/yyyy HH:mm}", ApplyFormatInEditMode = true)]
        public DateTime? ThoiDiemKetThuc { get; set; } // Đã sửa thành DateTime?

        [DisplayName("Số Lần Rời Màn Hình")]
        public int? SoLanRoiManHinh { get; set; } // Đã sửa thành int?

        [DisplayName("Trạng Thái Nộp Bài")]
        public string? TrangThaiNopBai { get; set; } // Đã sửa thành bool?

        [DisplayName("Địa Chỉ IP")]
        public string? Ipaddress { get; set; }

        [DisplayName("Tổng Điểm Đạt Được")]
        public double? TongDiemDatDuoc { get; set; } // Đã sửa thành double?

        [DisplayName("Đã Chấm Điểm")]
        public bool? DaChamDiem { get; set; } // Đã sửa thành bool?

        [DisplayName("Thông Tin Lịch Thi")]
        public string? ThongTinLichThi { get; set; }

        [DisplayName("Tên Đề Thi")]
        public string? TenDeThi { get; set; }

        [DisplayName("Tên Sinh Viên")]
        public string? TenSinhVien { get; set; }
    }
}
