namespace TCN_NCKH.Models.DBModel
{
    public class LichthiSummary
    {
        public Lichthi Lichthi { get; set; }
        public int SoLuongSinhVienDaLam { get; set; }
        public int SoLuongSinhVienChuaLam { get; set; }
        public int TongSoSinhVienLopHoc { get; set; }
    }

    public class KetquathiListViewModel
    {
        public IEnumerable<LichthiSummary> LichthiSummaries { get; set; }
        // Có thể thêm các thuộc tính phân trang nếu cần sau này, ví dụ:
        // public int PageNumber { get; set; } 
        // public int PageSize { get; set; }
        // public int TotalPages { get; set; }
    }
}