using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using System.ComponentModel.DataAnnotations; // Thêm dòng này

namespace TCN_NCKH.Models.DBModel;

public partial class Dethi
{
    [Key] // Đảm bảo đây là khóa chính
    [BindNever] // Vẫn giữ BindNever
    // XÓA BẤT KỲ THUỘC TÍNH [Required] NÀO Ở ĐÂY NẾU CÓ
    public string Id { get; set; } = null!; // Giữ null! vì bạn sẽ gán giá trị

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

    public virtual Bocauhoi? Bocauhoi { get; set; }

    public virtual ICollection<Cauhoi> Cauhois { get; set; } = new List<Cauhoi>();

    public virtual ICollection<Lichthi> Lichthis { get; set; } = new List<Lichthi>();

    public virtual Monhoc? Monhoc { get; set; }

    public virtual Nguoidung? NguoitaoNavigation { get; set; }
}