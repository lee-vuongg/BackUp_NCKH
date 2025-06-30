using System.Collections.Generic;
using TCN_NCKH.Models.DBModel; // Đảm bảo đúng namespace cho các Models DB

namespace TCN_NCKH.Models.ViewModels
{
    public class AssignStudentsToLichthiViewModel
    {
        public int LichthiId { get; set; }
        public string? LichthiDisplayText { get; set; } // Hiển thị thông tin lịch thi (vd: Đề thi - Lớp - Ngày)
        public string? LophocId { get; set; } // ID của lớp học liên quan đến lịch thi
        public string? LophocTenlop { get; set; } // Tên lớp học

        // Danh sách tất cả sinh viên trong lớp học này để hiển thị checkbox
        public List<SinhvienForAssignment> StudentsInClass { get; set; } = new List<SinhvienForAssignment>();

        // Danh sách các ID sinh viên đã được chọn từ form (khi POST)
        public List<string>? SelectedSinhvienIds { get; set; }

        // Mặc định cho phép thi khi tạo mới
        public bool DefaultDuocphepthi { get; set; } = true;
    }

    // Class con để chứa thông tin sinh viên cần thiết cho việc phân công
    public class SinhvienForAssignment
    {
        public string Sinhvienid { get; set; } = null!; // Khóa chính của sinh viên
        public string Msv { get; set; } = null!; // Mã sinh viên
        public string Hoten { get; set; } = null!; // Tên đầy đủ
        public bool IsAssigned { get; set; } // True nếu sinh viên đã được phân công cho lịch thi này
        public bool IsSelected { get; set; } // True nếu checkbox của sinh viên này được chọn trên form
    }
}
