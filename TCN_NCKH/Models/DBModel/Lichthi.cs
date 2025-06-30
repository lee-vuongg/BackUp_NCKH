using System;
using System.Collections.Generic;

namespace TCN_NCKH.Models.DBModel;

public partial class Lichthi
{
    public int Id { get; set; }

    public string? Dethiid { get; set; }

    public string? Lophocid { get; set; }

    public DateTime Ngaythi { get; set; }

    public int? Thoigian { get; set; }

    public string? Phongthi { get; set; }

    public DateTime? ThoigianBatdau { get; set; }

    public DateTime? ThoigianKetthuc { get; set; }

    public virtual Dethi? Dethi { get; set; }

    public virtual ICollection<Ketquathi> Ketquathis { get; set; } = new List<Ketquathi>();

    public virtual ICollection<LichthiSinhvien> LichthiSinhviens { get; set; } = new List<LichthiSinhvien>();

    public virtual Lophoc? Lophoc { get; set; }
}
