using System;
using System.Collections.Generic;

namespace TCN_NCKH.Models.DBModel;

public partial class Cauhoi
{
    public int Id { get; set; }

    public string Noidung { get; set; } = null!;

    public string? Dethiid { get; set; }

    public byte? Loaicauhoi { get; set; }

    public double? Diem { get; set; }

    public int? Bocauhoiid { get; set; }

    public string? DapandungKeys { get; set; }

    public virtual Bocauhoi? Bocauhoi { get; set; }

    public virtual ICollection<Dapan> Dapans { get; set; } = new List<Dapan>();

    public virtual Dethi? Dethi { get; set; }

    public virtual ICollection<TraloiSinhvien> TraloiSinhviens { get; set; } = new List<TraloiSinhvien>();
}
