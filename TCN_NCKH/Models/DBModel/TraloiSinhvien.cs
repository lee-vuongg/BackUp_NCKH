using System;
using System.Collections.Generic;

namespace TCN_NCKH.Models.DBModel;

public partial class TraloiSinhvien
{
    public int Id { get; set; } // Giả định có ID
    public string? Sinhvienid { get; set; }
    public int? Cauhoiid { get; set; }
    public int? Dapanid { get; set; }
    public DateTime? Ngaytraloi { get; set; }

    // Các thuộc tính navigation
    public virtual Sinhvien? Sinhvien { get; set; }
    public virtual Cauhoi? Cauhoi { get; set; }
    public virtual Dapan? Dapan { get; set; }

    // *************** CÁI NÀY PHẢI CÓ ***************
    public int? KetquathiId { get; set; } // Khóa ngoại
    public virtual Ketquathi? Ketquathi { get; set; } // Thuộc tính navigation
    // ***********************************************
}