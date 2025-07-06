using System;
using System.Collections.Generic;

namespace TCN_NCKH.Models.DBModel;

public partial class Bocauhoi
{
    public int Id { get; set; }

    public string Tenbocauhoi { get; set; } = null!;

    public string Monhocid { get; set; } = null!;

    public string? Mota { get; set; }

    public byte? Mucdokho { get; set; }

    public string? DapanMacdinhA { get; set; }

    public string? DapanMacdinhB { get; set; }

    public string? DapanMacdinhC { get; set; }

    public string? DapanMacdinhD { get; set; }

    public virtual ICollection<Cauhoi> Cauhois { get; set; } = new List<Cauhoi>();

    public virtual ICollection<Dethi> Dethis { get; set; } = new List<Dethi>();

    public virtual Monhoc Monhoc { get; set; } = null!;
}
