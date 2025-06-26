using System;
using System.Collections.Generic;

namespace TCN_NCKH.Models.DBModel;

public partial class Dethi
{
    public string Id { get; set; } = null!;

    public string? Tendethi { get; set; }

    public string? Monhocid { get; set; }

    public string? Nguoitao { get; set; }

    public DateTime? Ngaytao { get; set; }

    public virtual ICollection<Cauhoi> Cauhois { get; set; } = new List<Cauhoi>();

    public virtual ICollection<Lichthi> Lichthis { get; set; } = new List<Lichthi>();

    public virtual Monhoc? Monhoc { get; set; }

    public virtual Nguoidung? NguoitaoNavigation { get; set; }
}
