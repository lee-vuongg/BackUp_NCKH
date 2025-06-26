using System;
using System.Collections.Generic;

namespace TCN_NCKH.Models.DBModel;

public partial class Lophoc
{
    public string Id { get; set; } = null!;

    public string Tenlop { get; set; } = null!;

    public virtual ICollection<Lichthi> Lichthis { get; set; } = new List<Lichthi>();

    public virtual ICollection<Sinhvien> Sinhviens { get; set; } = new List<Sinhvien>();
}
