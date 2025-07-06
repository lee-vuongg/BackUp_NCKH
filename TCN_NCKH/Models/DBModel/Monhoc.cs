using System;
using System.Collections.Generic;

namespace TCN_NCKH.Models.DBModel;

public partial class Monhoc
{
    public string Id { get; set; } = null!;

    public string Tenmon { get; set; } = null!;

    public string? Mota { get; set; }

    public bool? Isactive { get; set; }

    public virtual ICollection<Bocauhoi> Bocauhois { get; set; } = new List<Bocauhoi>();

    public virtual ICollection<Dethi> Dethis { get; set; } = new List<Dethi>();
}
