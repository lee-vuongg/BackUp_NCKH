using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using TCN_NCKH.Models.DBModel; // Thêm để tham chiếu các models DB nếu cần

namespace TCN_NCKH.Models.ViewModels
{
    public class CauhoiViewModel
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Nội dung câu hỏi không được để trống.")]
        [StringLength(500, ErrorMessage = "Nội dung câu hỏi không được vượt quá 500 ký tự.")]
        [Display(Name = "Nội dung Câu hỏi")]
        public string Noidung { get; set; } = null!;

        [Display(Name = "Điểm")]
        [Required(ErrorMessage = "Điểm không được để trống.")]
        [Range(0.1, 1000.0, ErrorMessage = "Điểm phải là số dương (tối thiểu 0.1) và không quá 1000.")] // Cập nhật Range cho double
        public double Diem { get; set; } // ĐỔI TỪ int SANG double (hoặc double? nếu cho phép null)

        // Bỏ Mucdokho khỏi CauhoiViewModel vì nó được lấy từ Bocauhoi (nếu có)
        // Public byte? Loaicauhoi { get; set; }
        // Public string? DapandungKeys { get; set; }

        public int? Bocauhoiid { get; set; } // ID của bộ câu hỏi mà câu hỏi này thuộc về
        public string? Tenbocauhoi { get; set; } // Tên bộ câu hỏi để hiển thị trên form Create/Edit

        // Đáp án. Đảm bảo có ít nhất 4 đáp án khi tạo mới
        [MinLength(4, ErrorMessage = "Câu hỏi phải có ít nhất 4 đáp án.")]
        public List<DapanViewModel> Dapans { get; set; } = new List<DapanViewModel>();
    }

    public class DapanViewModel
    {
        public int Id { get; set; } // ID của đáp án (0 nếu là mới)

        [Required(ErrorMessage = "Nội dung đáp án không được để trống.")]
        [StringLength(500, MinimumLength = 1, ErrorMessage = "Nội dung phải có từ 10 đến 500 ký tự.")] // Sửa ở đây
        [Display(Name = "Nội dung Đáp án")]
        public string Noidung { get; set; } = null!;

        [Display(Name = "Là đáp án đúng")]
        public bool Dung { get; set; }
    }
}
