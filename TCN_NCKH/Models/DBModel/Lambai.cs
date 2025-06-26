using System;

namespace TCN_NCKH.Models.DBModel
{
    public partial class Lambai
    {
        public string Id { get; set; }  // Mã bài làm (Khóa chính)

        public string Dethiid { get; set; }  // Mã đề thi
        public string SinhVienid { get; set; }  // Mã sinh viên

        public DateTime Thoigianbatdau { get; set; }  // Thời gian bắt đầu làm bài
        public string Trangthai { get; set; }  // Trạng thái bài làm (VD: "DangLam", "HoanThanh")

        // Mối quan hệ với các bảng khác (mỗi bài làm có một đề thi và một sinh viên)
        public virtual Dethi Dethi { get; set; }  // Đề thi
        public virtual Sinhvien Sinhvien { get; set; }  // Sinh viên làm bài

        // Nếu bạn muốn lưu kết quả thi của sinh viên trong bảng Ketquathi
        public virtual Ketquathi Ketquathi { get; set; }
    }
}

