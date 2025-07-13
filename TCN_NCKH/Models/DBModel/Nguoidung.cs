using System;
using System.Collections.Generic;

namespace TCN_NCKH.Models.DBModel;

public partial class Nguoidung
{
    public string Id { get; set; } = null!;

    public string Hoten { get; set; } = null!;

    public string Email { get; set; } = null!;

    public string Matkhau { get; set; } = null!;

    public string? Sdt { get; set; }

    public DateOnly? Ngaysinh { get; set; }

    public bool? Gioitinh { get; set; }

    public bool? Trangthai { get; set; }

    public string? Nguoitao { get; set; }

    public DateTime? Ngaytao { get; set; }

    public byte? Loainguoidungid { get; set; }

    public string? AnhDaiDien { get; set; }

    public virtual ICollection<CheatDetectionLog> CheatDetectionLogs { get; set; } = new List<CheatDetectionLog>();

    public virtual ICollection<Dethi> Dethis { get; set; } = new List<Dethi>();

    public virtual Loainguoidung? Loainguoidung { get; set; }

    public virtual Sinhvien? Sinhvien { get; set; }

    public virtual ICollection<Loainguoidung> Loainguoidungs { get; set; } = new List<Loainguoidung>();
}
