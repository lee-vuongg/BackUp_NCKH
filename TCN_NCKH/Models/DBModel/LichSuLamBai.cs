using System;
using System.Collections.Generic;

namespace TCN_NCKH.Models.DBModel;

public partial class LichSuLamBai
{
    public int Id { get; set; }

    public string? MaSinhVien { get; set; }

    public string? MaDeThi { get; set; }

    public int LichThiId { get; set; }

    public DateTime? ThoiDiemBatDau { get; set; }

    public DateTime? ThoiDiemKetThuc { get; set; }

    public int? SoLanRoiManHinh { get; set; }

    public string? TrangThaiNopBai { get; set; }

    public string? Ipaddress { get; set; }

    public double? TongDiemDatDuoc { get; set; }

    public bool? DaChamDiem { get; set; }

    public virtual Lichthi? LichThi { get; set; }

    public virtual Dethi? MaDeThiNavigation { get; set; }

    public virtual Sinhvien? MaSinhVienNavigation { get; set; }
}
