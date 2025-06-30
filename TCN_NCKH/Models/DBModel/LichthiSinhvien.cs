using System;
using System.Collections.Generic;

namespace TCN_NCKH.Models.DBModel;

public partial class LichthiSinhvien
{
    public int Lichthiid { get; set; }

    public string Sinhvienid { get; set; } = null!;

    public bool Duocphepthi { get; set; }

    public virtual Lichthi Lichthi { get; set; } = null!;

    public virtual Sinhvien Sinhvien { get; set; } = null!;
}
