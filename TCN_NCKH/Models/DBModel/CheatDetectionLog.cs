// Models/DBModel/CheatDetectionLog.cs
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TCN_NCKH.Models.DBModel;

public partial class CheatDetectionLog
{
    public int Id { get; set; }

    [Required]
    [Column(TypeName = "char(15)")] // Đảm bảo khớp với kiểu dữ liệu của NGUOIDUNG.ID
    public string UserId { get; set; } = null!;

    [Required]
    [MaxLength(50)]
    public string DetectionType { get; set; } = null!;

    [MaxLength(1000)]
    public string? Details { get; set; }

    public DateTime Timestamp { get; set; }

    public int? LichSuLamBaiId { get; set; }

    // Navigation Properties
    public virtual LichSuLamBai? LichSuLamBai { get; set; }

    public virtual Nguoidung User { get; set; } = null!; // Đảm bảo User không null nếu UserId là NOT NULL
}