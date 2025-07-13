using System;
using System.Collections.Generic;

namespace TCN_NCKH.Models.DBModel;

public partial class Sinhvien
{
    public string Sinhvienid { get; set; } = null!;

    public string Msv { get; set; } = null!;

    public string? Lophocid { get; set; }

    public virtual ICollection<Ketquathi> Ketquathis { get; set; } = new List<Ketquathi>();

    public virtual ICollection<LichSuLamBai> LichSuLamBais { get; set; } = new List<LichSuLamBai>();

    public virtual ICollection<LichthiSinhvien> LichthiSinhviens { get; set; } = new List<LichthiSinhvien>();

    public virtual Lophoc? Lophoc { get; set; }

    public virtual Nguoidung SinhvienNavigation { get; set; } = null!;

    public virtual ICollection<TraloiSinhvien> TraloiSinhviens { get; set; } = new List<TraloiSinhvien>();
}
