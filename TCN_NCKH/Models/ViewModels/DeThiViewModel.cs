// File: TCN_NCKH/Models/ViewModels/DeThiViewModel.cs

using TCN_NCKH.Models.DBModel; // Đảm bảo đúng namespace cho các model DB của mi
using System;

namespace TCN_NCKH.Models.ViewModels
{
    public class DeThiViewModel
    {
        public Dethi? DeThi { get; set; } // Chứa thông tin của đối tượng đề thi gốc
        public bool DaNopBai { get; set; } // Thuộc tính mới để lưu trạng thái đã nộp hay chưa
        // Mi có thể thêm các thuộc tính khác ở đây nếu muốn hiển thị thêm thông tin cụ thể về kết quả
        // Ví dụ: public double? DiemSo { get; set; }
    }
}