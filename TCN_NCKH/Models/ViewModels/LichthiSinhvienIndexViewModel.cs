using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc.Rendering; // Cần thiết cho SelectList
using TCN_NCKH.Models.DBModel; // Cần thiết cho LichthiSinhvien và Lichthi

namespace TCN_NCKH.Models.ViewModels
{
    public class LichthiSinhvienIndexViewModel
    {
        // Danh sách các phân công lịch thi-sinh viên sẽ được hiển thị (có thể đã được lọc)
        public IEnumerable<LichthiSinhvien> Assignments { get; set; }

        // Danh sách các lịch thi để populate dropdown (dùng cho bộ lọc)
        public SelectList AvailableLichthis { get; set; }

        // ID của lịch thi đang được chọn trên bộ lọc (hoặc null nếu không chọn gì)
        public int? SelectedLichthiId { get; set; }

        public LichthiSinhvienIndexViewModel()
        {
            // Khởi tạo các danh sách để tránh NullReferenceException
            Assignments = new List<LichthiSinhvien>();
            // Khởi tạo AvailableLichthis với một danh sách rỗng để tránh NullReferenceException trước khi được populate
            AvailableLichthis = new SelectList(new List<Lichthi>(), "Id", "Tenlichthi");
        }
    }
}
