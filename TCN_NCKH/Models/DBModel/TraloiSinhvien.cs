﻿using System;
using System.Collections.Generic;

namespace TCN_NCKH.Models.DBModel;

public partial class TraloiSinhvien
{
    public int Id { get; set; }

    public string? Sinhvienid { get; set; }

    public int? Cauhoiid { get; set; }

    public int? Dapanid { get; set; }

    public DateTime? Ngaytraloi { get; set; }

    public int? KetquathiId { get; set; }

    public virtual Cauhoi? Cauhoi { get; set; }

    public virtual Dapan? Dapan { get; set; }

    public virtual Ketquathi? Ketquathi { get; set; }

    public virtual Sinhvien? Sinhvien { get; set; }
}
