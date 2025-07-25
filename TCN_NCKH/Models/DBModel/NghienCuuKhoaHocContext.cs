﻿using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace TCN_NCKH.Models.DBModel;

public partial class NghienCuuKhoaHocContext : DbContext
{
    public NghienCuuKhoaHocContext()
    {
    }

    public NghienCuuKhoaHocContext(DbContextOptions<NghienCuuKhoaHocContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Bocauhoi> Bocauhois { get; set; }

    public virtual DbSet<Cauhoi> Cauhois { get; set; }

    public virtual DbSet<CheatDetectionLog> CheatDetectionLogs { get; set; }

    public virtual DbSet<Dapan> Dapans { get; set; }

    public virtual DbSet<Dethi> Dethis { get; set; }

    public virtual DbSet<Ketquathi> Ketquathis { get; set; }

    public virtual DbSet<LichSuLamBai> LichSuLamBais { get; set; }

    public virtual DbSet<Lichthi> Lichthis { get; set; }

    public virtual DbSet<LichthiSinhvien> LichthiSinhviens { get; set; }

    public virtual DbSet<Loainguoidung> Loainguoidungs { get; set; }

    public virtual DbSet<Lophoc> Lophocs { get; set; }

    public virtual DbSet<Monhoc> Monhocs { get; set; }

    public virtual DbSet<Nguoidung> Nguoidungs { get; set; }

    public virtual DbSet<Sinhvien> Sinhviens { get; set; }

    public virtual DbSet<TraloiSinhvien> TraloiSinhviens { get; set; }

//    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
////#warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see https://go.microsoft.com/fwlink/?LinkId=723263.
////        => optionsBuilder.UseSqlServer("Server=VUONG\\SQLEXPRESS;Database=nghien_cuu_khoa_hoc;Trusted_Connection=True;MultipleActiveResultSets=True; TrustServerCertificate=True");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Bocauhoi>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__BOCAUHOI__3214EC27B643B184");

            entity.ToTable("BOCAUHOI");

            entity.Property(e => e.Id).HasColumnName("ID");
            entity.Property(e => e.DapanMacdinhA)
                .HasMaxLength(500)
                .HasColumnName("DAPAN_MACDINH_A");
            entity.Property(e => e.DapanMacdinhB)
                .HasMaxLength(500)
                .HasColumnName("DAPAN_MACDINH_B");
            entity.Property(e => e.DapanMacdinhC)
                .HasMaxLength(500)
                .HasColumnName("DAPAN_MACDINH_C");
            entity.Property(e => e.DapanMacdinhD)
                .HasMaxLength(500)
                .HasColumnName("DAPAN_MACDINH_D");
            entity.Property(e => e.Monhocid)
                .HasMaxLength(5)
                .IsUnicode(false)
                .IsFixedLength()
                .HasColumnName("MONHOCID");
            entity.Property(e => e.Mota).HasColumnName("MOTA");
            entity.Property(e => e.Mucdokho).HasColumnName("MUCDOKHO");
            entity.Property(e => e.Tenbocauhoi)
                .HasMaxLength(255)
                .HasColumnName("TENBOCAUHOI");

            entity.HasOne(d => d.Monhoc).WithMany(p => p.Bocauhois)
                .HasForeignKey(d => d.Monhocid)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_BOCAUHOI_MONHOC");
        });

        modelBuilder.Entity<Cauhoi>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__CAUHOI__3214EC270EB3EBE9");

            entity.ToTable("CAUHOI");

            entity.Property(e => e.Id).HasColumnName("ID");
            entity.Property(e => e.Bocauhoiid).HasColumnName("BOCAUHOIID");
            entity.Property(e => e.DapandungKeys)
                .HasMaxLength(4)
                .IsUnicode(false)
                .HasColumnName("DAPANDUNG_KEYS");
            entity.Property(e => e.Dethiid)
                .HasMaxLength(10)
                .IsUnicode(false)
                .IsFixedLength()
                .HasColumnName("DETHIID");
            entity.Property(e => e.Diem).HasColumnName("DIEM");
            entity.Property(e => e.Loaicauhoi).HasColumnName("LOAICAUHOI");
            entity.Property(e => e.Noidung)
                .HasMaxLength(500)
                .HasColumnName("NOIDUNG");

            entity.HasOne(d => d.Bocauhoi).WithMany(p => p.Cauhois)
                .HasForeignKey(d => d.Bocauhoiid)
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("FK_CAUHOI_BOCAUHOI");

            entity.HasOne(d => d.Dethi).WithMany(p => p.Cauhois)
                .HasForeignKey(d => d.Dethiid)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("FK__CAUHOI__DETHIID__2E1BDC42");
        });

        modelBuilder.Entity<CheatDetectionLog>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__CheatDet__3214EC07974BE044");

            entity.HasIndex(e => e.LichSuLamBaiId, "IX_CheatDetectionLogs_LichSuLamBaiId");

            entity.HasIndex(e => new { e.UserId, e.Timestamp }, "IX_CheatDetectionLogs_UserId_Timestamp").IsDescending(false, true);

            entity.Property(e => e.Details).HasMaxLength(1000);
            entity.Property(e => e.DetectionType).HasMaxLength(50);
            entity.Property(e => e.Timestamp).HasDefaultValueSql("(getutcdate())");
            entity.Property(e => e.UserId)
                .HasMaxLength(15)
                .IsUnicode(false)
                .IsFixedLength();

            entity.HasOne(d => d.LichSuLamBai).WithMany(p => p.CheatDetectionLogs)
                .HasForeignKey(d => d.LichSuLamBaiId)
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("FK_CheatDetectionLogs_LichSuLamBai");

            entity.HasOne(d => d.User).WithMany(p => p.CheatDetectionLogs)
                .HasForeignKey(d => d.UserId)
                .HasConstraintName("FK_CheatDetectionLogs_NguoiDung");
        });

        modelBuilder.Entity<Dapan>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__DAPAN__3214EC2771EC18BD");

            entity.ToTable("DAPAN");

            entity.Property(e => e.Id).HasColumnName("ID");
            entity.Property(e => e.Cauhoiid).HasColumnName("CAUHOIID");
            entity.Property(e => e.Dung)
                .HasDefaultValue(false)
                .HasColumnName("DUNG");
            entity.Property(e => e.Noidung)
                .HasMaxLength(500)
                .HasColumnName("NOIDUNG");

            entity.HasOne(d => d.Cauhoi).WithMany(p => p.Dapans)
                .HasForeignKey(d => d.Cauhoiid)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("FK__DAPAN__CAUHOIID__31EC6D26");
        });

        modelBuilder.Entity<Dethi>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__DETHI__3214EC27318AC9FC");

            entity.ToTable("DETHI");

            entity.Property(e => e.Id)
                .HasMaxLength(10)
                .IsUnicode(false)
                .IsFixedLength()
                .HasColumnName("ID");
            entity.Property(e => e.Bocauhoiid).HasColumnName("BOCAUHOIID");
            entity.Property(e => e.DethikhacnhauCa).HasColumnName("DETHIKHACNHAU_CA");
            entity.Property(e => e.Monhocid)
                .HasMaxLength(5)
                .IsUnicode(false)
                .IsFixedLength()
                .HasColumnName("MONHOCID");
            entity.Property(e => e.MucdokhoDe).HasColumnName("MUCDOKHO_DE");
            entity.Property(e => e.MucdokhoKho).HasColumnName("MUCDOKHO_KHO");
            entity.Property(e => e.MucdokhoTrungbinh).HasColumnName("MUCDOKHO_TRUNGBINH");
            entity.Property(e => e.Ngaytao)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime")
                .HasColumnName("NGAYTAO");
            entity.Property(e => e.Nguoitao)
                .HasMaxLength(15)
                .IsUnicode(false)
                .IsFixedLength()
                .HasColumnName("NGUOITAO");
            entity.Property(e => e.Soluongcauhoi).HasColumnName("SOLUONGCAUHOI");
            entity.Property(e => e.Tendethi)
                .HasMaxLength(100)
                .HasColumnName("TENDETHI");
            entity.Property(e => e.Thoiluongthi).HasColumnName("THOILUONGTHI");
            entity.Property(e => e.Trangthai)
                .HasMaxLength(50)
                .HasDefaultValue("Draft")
                .HasColumnName("TRANGTHAI");

            entity.HasOne(d => d.Bocauhoi).WithMany(p => p.Dethis)
                .HasForeignKey(d => d.Bocauhoiid)
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("FK_DETHI_BOCAUHOI");

            entity.HasOne(d => d.Monhoc).WithMany(p => p.Dethis)
                .HasForeignKey(d => d.Monhocid)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("FK__DETHI__MONHOCID__29572725");

            entity.HasOne(d => d.NguoitaoNavigation).WithMany(p => p.Dethis)
                .HasForeignKey(d => d.Nguoitao)
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("FK__DETHI__NGUOITAO__2A4B4B5E");
        });

        modelBuilder.Entity<Ketquathi>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__KETQUATH__3214EC27512E5456");

            entity.ToTable("KETQUATHI");

            entity.Property(e => e.Id).HasColumnName("ID");
            entity.Property(e => e.Diem).HasColumnName("DIEM");
            entity.Property(e => e.Lichthiid).HasColumnName("LICHTHIID");
            entity.Property(e => e.Ngaythi)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime")
                .HasColumnName("NGAYTHI");
            entity.Property(e => e.Sinhvienid)
                .HasMaxLength(15)
                .IsUnicode(false)
                .IsFixedLength()
                .HasColumnName("SINHVIENID");
            entity.Property(e => e.Thoigianlam).HasColumnName("THOIGIANLAM");

            entity.HasOne(d => d.Lichthi).WithMany(p => p.Ketquathis)
                .HasForeignKey(d => d.Lichthiid)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("FK__KETQUATHI__LICHT__403A8C7D");

            entity.HasOne(d => d.Sinhvien).WithMany(p => p.Ketquathis)
                .HasForeignKey(d => d.Sinhvienid)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("FK__KETQUATHI__SINHV__3F466844");
        });

        modelBuilder.Entity<LichSuLamBai>(entity =>
        {
            entity.ToTable("LichSuLamBai");

            entity.Property(e => e.DaChamDiem).HasDefaultValue(false);
            entity.Property(e => e.Ipaddress)
                .HasMaxLength(45)
                .HasColumnName("IPAddress");
            entity.Property(e => e.MaDeThi)
                .HasMaxLength(10)
                .IsUnicode(false)
                .IsFixedLength();
            entity.Property(e => e.MaSinhVien)
                .HasMaxLength(15)
                .IsUnicode(false)
                .IsFixedLength();
            entity.Property(e => e.ThoiDiemBatDau).HasColumnType("datetime");
            entity.Property(e => e.ThoiDiemKetThuc).HasColumnType("datetime");
            entity.Property(e => e.TrangThaiNopBai).HasMaxLength(20);

            entity.HasOne(d => d.LichThi).WithMany(p => p.LichSuLamBais)
                .HasForeignKey(d => d.LichThiId)
                .HasConstraintName("FK_LichSuLamBai_Lichthi");

            entity.HasOne(d => d.MaDeThiNavigation).WithMany(p => p.LichSuLamBais)
                .HasForeignKey(d => d.MaDeThi)
                .HasConstraintName("FK_LichSuLamBai_Dethi");

            entity.HasOne(d => d.MaSinhVienNavigation).WithMany(p => p.LichSuLamBais)
                .HasForeignKey(d => d.MaSinhVien)
                .HasConstraintName("FK_LichSuLamBai_Sinhvien");
        });

        modelBuilder.Entity<Lichthi>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__LICHTHI__3214EC2703F100A0");

            entity.ToTable("LICHTHI");

            entity.Property(e => e.Id).HasColumnName("ID");
            entity.Property(e => e.Dethiid)
                .HasMaxLength(10)
                .IsUnicode(false)
                .IsFixedLength()
                .HasColumnName("DETHIID");
            entity.Property(e => e.Lophocid)
                .HasMaxLength(10)
                .IsUnicode(false)
                .IsFixedLength()
                .HasColumnName("LOPHOCID");
            entity.Property(e => e.Ngaythi)
                .HasColumnType("datetime")
                .HasColumnName("NGAYTHI");
            entity.Property(e => e.Phongthi)
                .HasMaxLength(50)
                .HasColumnName("PHONGTHI");
            entity.Property(e => e.Thoigian).HasColumnName("THOIGIAN");
            entity.Property(e => e.ThoigianBatdau)
                .HasColumnType("datetime")
                .HasColumnName("THOIGIAN_BATDAU");
            entity.Property(e => e.ThoigianKetthuc)
                .HasColumnType("datetime")
                .HasColumnName("THOIGIAN_KETTHUC");

            entity.HasOne(d => d.Dethi).WithMany(p => p.Lichthis)
                .HasForeignKey(d => d.Dethiid)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("FK__LICHTHI__DETHIID__3B75D760");

            entity.HasOne(d => d.Lophoc).WithMany(p => p.Lichthis)
                .HasForeignKey(d => d.Lophocid)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("FK__LICHTHI__LOPHOCI__3C69FB99");
        });

        modelBuilder.Entity<LichthiSinhvien>(entity =>
        {
            entity.HasKey(e => new { e.Lichthiid, e.Sinhvienid }).HasName("PK__LICHTHI___FFA2C5444CDD6EBB");

            entity.ToTable("LICHTHI_SINHVIEN");

            entity.Property(e => e.Lichthiid).HasColumnName("LICHTHIID");
            entity.Property(e => e.Sinhvienid)
                .HasMaxLength(15)
                .IsUnicode(false)
                .IsFixedLength()
                .HasColumnName("SINHVIENID");
            entity.Property(e => e.Duocphepthi)
                .HasDefaultValue(true)
                .HasColumnName("DUOCPHEPTHI");

            entity.HasOne(d => d.Lichthi).WithMany(p => p.LichthiSinhviens)
                .HasForeignKey(d => d.Lichthiid)
                .HasConstraintName("FK_LICHTHI_SINHVIEN_LICHTHI");

            entity.HasOne(d => d.Sinhvien).WithMany(p => p.LichthiSinhviens)
                .HasForeignKey(d => d.Sinhvienid)
                .HasConstraintName("FK_LICHTHI_SINHVIEN_SINHVIEN");
        });

        modelBuilder.Entity<Loainguoidung>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__LOAINGUO__3214EC2720455DF2");

            entity.ToTable("LOAINGUOIDUNG");

            entity.HasIndex(e => e.Tenloai, "UQ__LOAINGUO__BF45B9965D079753").IsUnique();

            entity.Property(e => e.Id).HasColumnName("ID");
            entity.Property(e => e.Tenloai)
                .HasMaxLength(50)
                .HasColumnName("TENLOAI");
        });

        modelBuilder.Entity<Lophoc>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__LOPHOC__3214EC274DD060B7");

            entity.ToTable("LOPHOC");

            entity.HasIndex(e => e.Tenlop, "UQ__LOPHOC__E100BDB72D7932BE").IsUnique();

            entity.Property(e => e.Id)
                .HasMaxLength(10)
                .IsUnicode(false)
                .IsFixedLength()
                .HasColumnName("ID");
            entity.Property(e => e.Tenlop)
                .HasMaxLength(50)
                .HasColumnName("TENLOP");
        });

        modelBuilder.Entity<Monhoc>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__MONHOC__3214EC275909EA44");

            entity.ToTable("MONHOC");

            entity.Property(e => e.Id)
                .HasMaxLength(5)
                .IsUnicode(false)
                .IsFixedLength()
                .HasColumnName("ID");
            entity.Property(e => e.Isactive)
                .HasDefaultValue(true)
                .HasColumnName("ISACTIVE");
            entity.Property(e => e.Mota)
                .HasColumnType("ntext")
                .HasColumnName("MOTA");
            entity.Property(e => e.Tenmon)
                .HasMaxLength(100)
                .HasColumnName("TENMON");
        });

        modelBuilder.Entity<Nguoidung>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__NGUOIDUN__3214EC274649593C");

            entity.ToTable("NGUOIDUNG");

            entity.HasIndex(e => e.Email, "UQ__NGUOIDUN__161CF724BBA79AFF").IsUnique();

            entity.HasIndex(e => e.Sdt, "UQ__NGUOIDUN__CA1930A5836D1A22").IsUnique();

            entity.Property(e => e.Id)
                .HasMaxLength(15)
                .IsUnicode(false)
                .IsFixedLength()
                .HasColumnName("ID");
            entity.Property(e => e.AnhDaiDien).HasMaxLength(255);
            entity.Property(e => e.Email)
                .HasMaxLength(100)
                .IsUnicode(false)
                .HasColumnName("EMAIL");
            entity.Property(e => e.Gioitinh).HasColumnName("GIOITINH");
            entity.Property(e => e.Hoten)
                .HasMaxLength(100)
                .HasColumnName("HOTEN");
            entity.Property(e => e.Loainguoidungid).HasColumnName("LOAINGUOIDUNGID");
            entity.Property(e => e.Matkhau)
                .HasMaxLength(255)
                .IsUnicode(false)
                .HasColumnName("MATKHAU");
            entity.Property(e => e.Ngaysinh).HasColumnName("NGAYSINH");
            entity.Property(e => e.Ngaytao)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime")
                .HasColumnName("NGAYTAO");
            entity.Property(e => e.Nguoitao)
                .HasMaxLength(15)
                .IsUnicode(false)
                .IsFixedLength()
                .HasColumnName("NGUOITAO");
            entity.Property(e => e.Sdt)
                .HasMaxLength(15)
                .IsUnicode(false)
                .HasColumnName("SDT");
            entity.Property(e => e.Trangthai)
                .HasDefaultValue(true)
                .HasColumnName("TRANGTHAI");

            entity.HasOne(d => d.Loainguoidung).WithMany(p => p.Nguoidungs)
                .HasForeignKey(d => d.Loainguoidungid)
                .HasConstraintName("FK__NGUOIDUNG__LOAIN__173876EA");

            entity.HasMany(d => d.Loainguoidungs).WithMany(p => p.NguoidungsNavigation)
                .UsingEntity<Dictionary<string, object>>(
                    "NguoidungLoai",
                    r => r.HasOne<Loainguoidung>().WithMany()
                        .HasForeignKey("Loainguoidungid")
                        .HasConstraintName("FK__NGUOIDUNG__LOAIN__44FF419A"),
                    l => l.HasOne<Nguoidung>().WithMany()
                        .HasForeignKey("Nguoidungid")
                        .HasConstraintName("FK__NGUOIDUNG__NGUOI__440B1D61"),
                    j =>
                    {
                        j.HasKey("Nguoidungid", "Loainguoidungid").HasName("PK__NGUOIDUN__B50EA8B510E1284D");
                        j.ToTable("NGUOIDUNG_LOAI");
                        j.IndexerProperty<string>("Nguoidungid")
                            .HasMaxLength(15)
                            .IsUnicode(false)
                            .IsFixedLength()
                            .HasColumnName("NGUOIDUNGID");
                        j.IndexerProperty<byte>("Loainguoidungid").HasColumnName("LOAINGUOIDUNGID");
                    });
        });

        modelBuilder.Entity<Sinhvien>(entity =>
        {
            entity.HasKey(e => e.Sinhvienid).HasName("PK__SINHVIEN__7B422E3A9725E501");

            entity.ToTable("SINHVIEN");

            entity.HasIndex(e => e.Msv, "UQ__SINHVIEN__C790E5AD46CF3D3E").IsUnique();

            entity.Property(e => e.Sinhvienid)
                .HasMaxLength(15)
                .IsUnicode(false)
                .IsFixedLength()
                .HasColumnName("SINHVIENID");
            entity.Property(e => e.Lophocid)
                .HasMaxLength(10)
                .IsUnicode(false)
                .IsFixedLength()
                .HasColumnName("LOPHOCID");
            entity.Property(e => e.Msv)
                .HasMaxLength(15)
                .IsUnicode(false)
                .IsFixedLength()
                .HasColumnName("MSV");

            entity.HasOne(d => d.Lophoc).WithMany(p => p.Sinhviens)
                .HasForeignKey(d => d.Lophocid)
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("FK__SINHVIEN__LOPHOC__239E4DCF");

            entity.HasOne(d => d.SinhvienNavigation).WithOne(p => p.Sinhvien)
                .HasForeignKey<Sinhvien>(d => d.Sinhvienid)
                .HasConstraintName("FK__SINHVIEN__SINHVI__22AA2996");
        });

        modelBuilder.Entity<TraloiSinhvien>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__TRALOI_S__3214EC2731C5707F");

            entity.ToTable("TRALOI_SINHVIEN");

            entity.HasIndex(e => e.KetquathiId, "IX_TRALOI_SINHVIEN_KetquathiId");

            entity.Property(e => e.Id).HasColumnName("ID");
            entity.Property(e => e.Cauhoiid).HasColumnName("CAUHOIID");
            entity.Property(e => e.Dapanid).HasColumnName("DAPANID");
            entity.Property(e => e.Ngaytraloi)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime")
                .HasColumnName("NGAYTRALOI");
            entity.Property(e => e.Sinhvienid)
                .HasMaxLength(15)
                .IsUnicode(false)
                .IsFixedLength()
                .HasColumnName("SINHVIENID");

            entity.HasOne(d => d.Cauhoi).WithMany(p => p.TraloiSinhviens)
                .HasForeignKey(d => d.Cauhoiid)
                .HasConstraintName("FK__TRALOI_SI__CAUHO__36B12243");

            entity.HasOne(d => d.Dapan).WithMany(p => p.TraloiSinhviens)
                .HasForeignKey(d => d.Dapanid)
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("FK__TRALOI_SI__DAPAN__37A5467C");

            entity.HasOne(d => d.Ketquathi).WithMany(p => p.TraloiSinhviens)
                .HasForeignKey(d => d.KetquathiId)
                .HasConstraintName("FK_TRALOI_SINHVIEN_Ketquathi_KetquathiId");

            entity.HasOne(d => d.Sinhvien).WithMany(p => p.TraloiSinhviens)
                .HasForeignKey(d => d.Sinhvienid)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("FK__TRALOI_SI__SINHV__35BCFE0A");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
