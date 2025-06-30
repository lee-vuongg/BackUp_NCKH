namespace TCN_NCKH.Models.DBModel
{
    public class KetQuaViewModel
    {
        public Ketquathi Ketquathi { get; set; } = null!;

        // Dictionary<CauhoiId, List<DapanId>> - cho phép nhiều đáp án chọn
        public Dictionary<int, List<int>> DapAnChon { get; set; } = new();

        public double? Diem { get; set; } // Thêm dấu '?'
    }
}
