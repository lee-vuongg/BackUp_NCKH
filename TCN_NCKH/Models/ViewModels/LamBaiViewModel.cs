using TCN_NCKH.Models.DBModel;
using System; // Thêm using System; cho DateTime

namespace TCN_NCKH.Areas.HocSinh.Models
{
    public class LamBaiViewModel
    {
        public Lichthi? LichThi { get; set; }
        public int LichSuLamBaiId { get; set; } // ID của bản ghi LichSuLamBai
        public string? SinhVienMsv { get; set; } // Để truyền MSV qua view nếu cần
        public DateTime ThoiGianKetThucThi { get; set; } // Để truyền thời gian kết thúc thi
        public bool IsExpired { get; set; } // Cờ để biết đã hết hạn/đã nộp
        public string? ExpiryMessage { get; set; } // Thông báo hết hạn/đã nộp
        // Có thể thêm các thuộc tính khác cần thiết cho View
    }
}