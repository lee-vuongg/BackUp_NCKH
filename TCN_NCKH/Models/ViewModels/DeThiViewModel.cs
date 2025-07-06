// Models/ViewModels/DethiViewModel.cs
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering; // Để sử dụng SelectListItem
using TCN_NCKH.Models.DBModel; // Để tham chiếu đến các models DB như Dethi

namespace TCN_NCKH.Models.ViewModels
{
    public class DethiViewModel
    {
        // Thuộc tính để chứa đối tượng Dethi gốc, hữu ích khi hiển thị chi tiết hoặc các tác vụ khác
        public Dethi? DeThi { get; set; }
        // Cờ cho biết đề thi đã được nộp bài hay chưa (có thể dùng trong context làm bài/kết quả)
        public bool DaNopBai { get; set; }
        [Display(Name = "Thời lượng thi (phút)")]
        [Range(1, 300, ErrorMessage = "Thời lượng thi phải từ 1 đến 300 phút.")]
        public int? ThoiLuongThi { get; set; }

        [Display(Name = "Trạng thái")]
        public string? TrangThai { get; set; }

        // Thêm list này để populate dropdown cho trạng thái
        public List<SelectListItem> TrangThaiList { get; set; } = new List<SelectListItem>();

        // ID của đề thi. Là string để khớp với kiểu dữ liệu trong model Dethi của mi.
        // Có thể là null khi tạo mới, nhưng cần cho chỉnh sửa.
        public string? Id { get; set; }

        [Required(ErrorMessage = "Tên đề thi không được để trống.")]
        [StringLength(100, ErrorMessage = "Tên đề thi không được vượt quá 100 ký tự.")]
        [Display(Name = "Tên đề thi")]
        public string Tendethi { get; set; } = null!;

        [Required(ErrorMessage = "Môn học không được để trống.")]
        [Display(Name = "Môn học")]
        // Monhocid là string để khớp với Monhoc.Id trong DB
        public string? Monhocid { get; set; }

        [Required(ErrorMessage = "Bộ câu hỏi không được để trống.")]
        [Display(Name = "Bộ câu hỏi")]
        public int? Bocauhoiid { get; set; }

        [Required(ErrorMessage = "Số lượng câu hỏi không được để trống.")]
        [Range(1, 100, ErrorMessage = "Số lượng câu hỏi phải là số nguyên dương và không quá 100.")]
        [Display(Name = "Số lượng câu hỏi")]
        public int Soluongcauhoi { get; set; }

        // Thuộc tính để chọn mức độ khó.
        // Nó sẽ được dùng trong View (dropdown) và sau đó được Controller chuyển đổi
        // thành các thuộc tính boolean MucdokhoDe, MucdokhoTrungbinh, MucdokhoKho
        [Display(Name = "Mức độ khó")]
        public byte? Mucdokho { get; set; }

        [Display(Name = "Đề thi khác nhau cho mỗi ca")]
        public bool DethikhacnhauCa { get; set; }

        // Các thuộc tính để đổ dữ liệu vào các dropdown list trong View
        public List<SelectListItem> MonhocList { get; set; } = new List<SelectListItem>();
        public List<SelectListItem> BocauhoiList { get; set; } = new List<SelectListItem>();

        // Các thuộc tính không hiển thị trên form nhưng cần thiết cho logic Controller
        public string? Nguoitao { get; set; }
        public DateTime? Ngaytao { get; set; }
    }
}
