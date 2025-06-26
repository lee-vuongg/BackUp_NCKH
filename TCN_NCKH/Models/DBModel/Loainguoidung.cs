using System;
using System.Collections.Generic;

namespace TCN_NCKH.Models.DBModel;

public partial class Loainguoidung
{
    public byte Id { get; set; }

    public string Tenloai { get; set; } = null!;

    public virtual ICollection<Nguoidung> Nguoidungs { get; set; } = new List<Nguoidung>();

    public virtual ICollection<Nguoidung> NguoidungsNavigation { get; set; } = new List<Nguoidung>();
}
