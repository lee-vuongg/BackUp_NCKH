using System;
using System.Collections.Generic;

namespace TCN_NCKH.Models.DBModel;

public partial class Dethi
{
    public string Id { get; set; } = null!;

    public string Tendethi { get; set; } = null!;

    public string? Monhocid { get; set; }

    public string? Nguoitao { get; set; }

    public DateTime? Ngaytao { get; set; }

    public int? Bocauhoiid { get; set; }

    public int Soluongcauhoi { get; set; }

    public bool MucdokhoDe { get; set; }

    public bool MucdokhoTrungbinh { get; set; }

    public bool MucdokhoKho { get; set; }

    public bool DethikhacnhauCa { get; set; }

    public int? Thoiluongthi { get; set; }

    public string? Trangthai { get; set; }

    public virtual Bocauhoi? Bocauhoi { get; set; }

    public virtual ICollection<Cauhoi> Cauhois { get; set; } = new List<Cauhoi>();

    public virtual ICollection<LichSuLamBai> LichSuLamBais { get; set; } = new List<LichSuLamBai>();

    public virtual ICollection<Lichthi> Lichthis { get; set; } = new List<Lichthi>();

    public virtual Monhoc? Monhoc { get; set; }

    public virtual Nguoidung? NguoitaoNavigation { get; set; }
}
