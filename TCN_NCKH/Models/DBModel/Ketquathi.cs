using System;
using System.Collections.Generic;

namespace TCN_NCKH.Models.DBModel;

public partial class Ketquathi
{
    public int Id { get; set; }

    public string? Sinhvienid { get; set; }

    public int? Lichthiid { get; set; }

    public double? Diem { get; set; }

    public int Thoigianlam { get; set; }

    public DateTime? Ngaythi { get; set; }

    public virtual Lichthi? Lichthi { get; set; }

    public virtual Sinhvien? Sinhvien { get; set; }

    public virtual ICollection<TraloiSinhvien> TraloiSinhviens { get; set; } = new List<TraloiSinhvien>();
}
