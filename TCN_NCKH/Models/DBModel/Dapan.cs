using System;
using System.Collections.Generic;

namespace TCN_NCKH.Models.DBModel;

public partial class Dapan
{
    public int Id { get; set; }

    public int? Cauhoiid { get; set; }

    public string Noidung { get; set; } = null!;

    public bool? Dung { get; set; }

    public virtual Cauhoi? Cauhoi { get; set; }

    public virtual ICollection<TraloiSinhvien> TraloiSinhviens { get; set; } = new List<TraloiSinhvien>();
}
