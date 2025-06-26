namespace TCN_NCKH.Models.DBModel;


    public class DethiListViewModel
    {
        public IEnumerable<Dethi> Dethis { get; set; } // Danh sách đề thi cho trang hiện tại
        public int PageNumber { get; set; } // Trang hiện tại
        public int PageSize { get; set; } // Số lượng đề thi trên mỗi trang
        public int TotalPages { get; set; } // Tổng số trang
        public int TotalItems { get; set; } // Tổng số đề thi
    }
