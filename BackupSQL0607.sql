USE [nghien_cuu_khoa_hoc]
GO
/****** Object:  Table [dbo].[__EFMigrationsHistory]    Script Date: 06/07/2025 7:31:01 CH ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[__EFMigrationsHistory](
	[MigrationId] [nvarchar](150) NOT NULL,
	[ProductVersion] [nvarchar](32) NOT NULL,
 CONSTRAINT [PK___EFMigrationsHistory] PRIMARY KEY CLUSTERED 
(
	[MigrationId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO
/****** Object:  Table [dbo].[BOCAUHOI]    Script Date: 06/07/2025 7:31:01 CH ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_PADDING ON
GO
CREATE TABLE [dbo].[BOCAUHOI](
	[ID] [int] IDENTITY(1,1) NOT NULL,
	[TENBOCAUHOI] [nvarchar](255) NOT NULL,
	[MONHOCID] [char](5) NOT NULL,
	[MOTA] [nvarchar](max) NULL,
	[MUCDOKHO] [tinyint] NULL,
	[DAPAN_MACDINH_A] [nvarchar](500) NULL,
	[DAPAN_MACDINH_B] [nvarchar](500) NULL,
	[DAPAN_MACDINH_C] [nvarchar](500) NULL,
	[DAPAN_MACDINH_D] [nvarchar](500) NULL,
PRIMARY KEY CLUSTERED 
(
	[ID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]

GO
SET ANSI_PADDING OFF
GO
/****** Object:  Table [dbo].[CAUHOI]    Script Date: 06/07/2025 7:31:01 CH ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_PADDING ON
GO
CREATE TABLE [dbo].[CAUHOI](
	[ID] [int] IDENTITY(1,1) NOT NULL,
	[NOIDUNG] [nvarchar](500) NOT NULL,
	[DETHIID] [char](10) NULL,
	[LOAICAUHOI] [tinyint] NULL,
	[DIEM] [float] NOT NULL,
	[BOCAUHOIID] [int] NULL,
	[DAPANDUNG_KEYS] [varchar](4) NULL,
PRIMARY KEY CLUSTERED 
(
	[ID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO
SET ANSI_PADDING OFF
GO
/****** Object:  Table [dbo].[DAPAN]    Script Date: 06/07/2025 7:31:01 CH ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[DAPAN](
	[ID] [int] IDENTITY(1,1) NOT NULL,
	[CAUHOIID] [int] NULL,
	[NOIDUNG] [nvarchar](500) NOT NULL,
	[DUNG] [bit] NULL DEFAULT ((0)),
PRIMARY KEY CLUSTERED 
(
	[ID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO
/****** Object:  Table [dbo].[DETHI]    Script Date: 06/07/2025 7:31:01 CH ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_PADDING ON
GO
CREATE TABLE [dbo].[DETHI](
	[ID] [char](10) NOT NULL,
	[TENDETHI] [nvarchar](100) NOT NULL,
	[MONHOCID] [char](5) NULL,
	[NGUOITAO] [char](15) NULL,
	[NGAYTAO] [datetime] NULL DEFAULT (getdate()),
	[BOCAUHOIID] [int] NULL,
	[SOLUONGCAUHOI] [int] NOT NULL DEFAULT ((0)),
	[MUCDOKHO_DE] [bit] NOT NULL CONSTRAINT [DF_DETHI_MUCDOKHO_DE]  DEFAULT ((0)),
	[MUCDOKHO_TRUNGBINH] [bit] NOT NULL CONSTRAINT [DF_DETHI_MUCDOKHO_TRUNGBINH]  DEFAULT ((0)),
	[MUCDOKHO_KHO] [bit] NOT NULL CONSTRAINT [DF_DETHI_MUCDOKHO_KHO]  DEFAULT ((0)),
	[DETHIKHACNHAU_CA] [bit] NOT NULL CONSTRAINT [DF_DETHI_DETHIKHACNHAU_CA]  DEFAULT ((0)),
	[THOILUONGTHI] [int] NULL,
	[TRANGTHAI] [nvarchar](50) NULL CONSTRAINT [DF_DETHI_TRANGTHAI]  DEFAULT (N'Draft'),
PRIMARY KEY CLUSTERED 
(
	[ID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO
SET ANSI_PADDING OFF
GO
/****** Object:  Table [dbo].[KETQUATHI]    Script Date: 06/07/2025 7:31:01 CH ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_PADDING ON
GO
CREATE TABLE [dbo].[KETQUATHI](
	[ID] [int] IDENTITY(1,1) NOT NULL,
	[SINHVIENID] [char](15) NULL,
	[LICHTHIID] [int] NULL,
	[DIEM] [float] NOT NULL,
	[THOIGIANLAM] [int] NOT NULL,
	[NGAYTHI] [datetime] NULL DEFAULT (getdate()),
PRIMARY KEY CLUSTERED 
(
	[ID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO
SET ANSI_PADDING OFF
GO
/****** Object:  Table [dbo].[LichSuLamBai]    Script Date: 06/07/2025 7:31:01 CH ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_PADDING ON
GO
CREATE TABLE [dbo].[LichSuLamBai](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[MaSinhVien] [char](15) NULL,
	[MaDeThi] [char](10) NULL,
	[LichThiId] [int] NULL,
	[ThoiDiemBatDau] [datetime] NULL,
	[ThoiDiemKetThuc] [datetime] NULL,
	[SoLanRoiManHinh] [int] NOT NULL DEFAULT ((0)),
	[TrangThaiNopBai] [nvarchar](20) NULL,
	[IPAddress] [nvarchar](45) NULL,
	[TongDiemDatDuoc] [float] NULL,
	[DaChamDiem] [bit] NULL DEFAULT ((0)),
 CONSTRAINT [PK_LichSuLamBai] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO
SET ANSI_PADDING OFF
GO
/****** Object:  Table [dbo].[LICHTHI]    Script Date: 06/07/2025 7:31:01 CH ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_PADDING ON
GO
CREATE TABLE [dbo].[LICHTHI](
	[ID] [int] IDENTITY(1,1) NOT NULL,
	[DETHIID] [char](10) NULL,
	[LOPHOCID] [char](10) NULL,
	[NGAYTHI] [datetime] NOT NULL,
	[THOIGIAN] [int] NOT NULL,
	[PHONGTHI] [nvarchar](50) NULL,
	[THOIGIAN_BATDAU] [datetime] NULL,
	[THOIGIAN_KETTHUC] [datetime] NULL,
PRIMARY KEY CLUSTERED 
(
	[ID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO
SET ANSI_PADDING OFF
GO
/****** Object:  Table [dbo].[LICHTHI_SINHVIEN]    Script Date: 06/07/2025 7:31:01 CH ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_PADDING ON
GO
CREATE TABLE [dbo].[LICHTHI_SINHVIEN](
	[LICHTHIID] [int] NOT NULL,
	[SINHVIENID] [char](15) NOT NULL,
	[DUOCPHEPTHI] [bit] NOT NULL DEFAULT ((1)),
PRIMARY KEY CLUSTERED 
(
	[LICHTHIID] ASC,
	[SINHVIENID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO
SET ANSI_PADDING OFF
GO
/****** Object:  Table [dbo].[LOAINGUOIDUNG]    Script Date: 06/07/2025 7:31:01 CH ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[LOAINGUOIDUNG](
	[ID] [tinyint] NOT NULL,
	[TENLOAI] [nvarchar](50) NOT NULL,
PRIMARY KEY CLUSTERED 
(
	[ID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY],
UNIQUE NONCLUSTERED 
(
	[TENLOAI] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO
/****** Object:  Table [dbo].[LOPHOC]    Script Date: 06/07/2025 7:31:01 CH ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_PADDING ON
GO
CREATE TABLE [dbo].[LOPHOC](
	[ID] [char](10) NOT NULL,
	[TENLOP] [nvarchar](50) NOT NULL,
PRIMARY KEY CLUSTERED 
(
	[ID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY],
UNIQUE NONCLUSTERED 
(
	[TENLOP] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO
SET ANSI_PADDING OFF
GO
/****** Object:  Table [dbo].[MONHOC]    Script Date: 06/07/2025 7:31:01 CH ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_PADDING ON
GO
CREATE TABLE [dbo].[MONHOC](
	[ID] [char](5) NOT NULL,
	[TENMON] [nvarchar](100) NOT NULL,
	[MOTA] [ntext] NULL,
	[ISACTIVE] [bit] NULL DEFAULT ((1)),
PRIMARY KEY CLUSTERED 
(
	[ID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]

GO
SET ANSI_PADDING OFF
GO
/****** Object:  Table [dbo].[NGUOIDUNG]    Script Date: 06/07/2025 7:31:01 CH ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_PADDING ON
GO
CREATE TABLE [dbo].[NGUOIDUNG](
	[ID] [char](15) NOT NULL,
	[HOTEN] [nvarchar](100) NOT NULL,
	[EMAIL] [varchar](100) NOT NULL,
	[MATKHAU] [varchar](255) NOT NULL,
	[SDT] [varchar](15) NULL,
	[NGAYSINH] [date] NULL,
	[GIOITINH] [bit] NULL,
	[TRANGTHAI] [bit] NULL DEFAULT ((1)),
	[NGUOITAO] [char](15) NULL,
	[NGAYTAO] [datetime] NULL DEFAULT (getdate()),
	[LOAINGUOIDUNGID] [tinyint] NULL,
	[AnhDaiDien] [nvarchar](255) NULL,
PRIMARY KEY CLUSTERED 
(
	[ID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY],
UNIQUE NONCLUSTERED 
(
	[EMAIL] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY],
UNIQUE NONCLUSTERED 
(
	[SDT] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO
SET ANSI_PADDING OFF
GO
/****** Object:  Table [dbo].[NGUOIDUNG_LOAI]    Script Date: 06/07/2025 7:31:01 CH ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_PADDING ON
GO
CREATE TABLE [dbo].[NGUOIDUNG_LOAI](
	[NGUOIDUNGID] [char](15) NOT NULL,
	[LOAINGUOIDUNGID] [tinyint] NOT NULL,
PRIMARY KEY CLUSTERED 
(
	[NGUOIDUNGID] ASC,
	[LOAINGUOIDUNGID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO
SET ANSI_PADDING OFF
GO
/****** Object:  Table [dbo].[SINHVIEN]    Script Date: 06/07/2025 7:31:01 CH ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_PADDING ON
GO
CREATE TABLE [dbo].[SINHVIEN](
	[SINHVIENID] [char](15) NOT NULL,
	[MSV] [char](15) NOT NULL,
	[LOPHOCID] [char](10) NULL,
PRIMARY KEY CLUSTERED 
(
	[SINHVIENID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY],
UNIQUE NONCLUSTERED 
(
	[MSV] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO
SET ANSI_PADDING OFF
GO
/****** Object:  Table [dbo].[TRALOI_SINHVIEN]    Script Date: 06/07/2025 7:31:01 CH ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_PADDING ON
GO
CREATE TABLE [dbo].[TRALOI_SINHVIEN](
	[ID] [int] IDENTITY(1,1) NOT NULL,
	[SINHVIENID] [char](15) NULL,
	[CAUHOIID] [int] NULL,
	[DAPANID] [int] NULL,
	[NGAYTRALOI] [datetime] NULL DEFAULT (getdate()),
	[KetquathiId] [int] NULL,
PRIMARY KEY CLUSTERED 
(
	[ID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO
SET ANSI_PADDING OFF
GO
ALTER TABLE [dbo].[BOCAUHOI]  WITH CHECK ADD  CONSTRAINT [FK_BOCAUHOI_MONHOC] FOREIGN KEY([MONHOCID])
REFERENCES [dbo].[MONHOC] ([ID])
GO
ALTER TABLE [dbo].[BOCAUHOI] CHECK CONSTRAINT [FK_BOCAUHOI_MONHOC]
GO
ALTER TABLE [dbo].[CAUHOI]  WITH CHECK ADD FOREIGN KEY([DETHIID])
REFERENCES [dbo].[DETHI] ([ID])
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[CAUHOI]  WITH CHECK ADD  CONSTRAINT [FK_CAUHOI_BOCAUHOI] FOREIGN KEY([BOCAUHOIID])
REFERENCES [dbo].[BOCAUHOI] ([ID])
ON DELETE SET NULL
GO
ALTER TABLE [dbo].[CAUHOI] CHECK CONSTRAINT [FK_CAUHOI_BOCAUHOI]
GO
ALTER TABLE [dbo].[DAPAN]  WITH CHECK ADD FOREIGN KEY([CAUHOIID])
REFERENCES [dbo].[CAUHOI] ([ID])
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[DETHI]  WITH CHECK ADD FOREIGN KEY([MONHOCID])
REFERENCES [dbo].[MONHOC] ([ID])
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[DETHI]  WITH CHECK ADD FOREIGN KEY([NGUOITAO])
REFERENCES [dbo].[NGUOIDUNG] ([ID])
ON DELETE SET NULL
GO
ALTER TABLE [dbo].[DETHI]  WITH CHECK ADD  CONSTRAINT [FK_DETHI_BOCAUHOI] FOREIGN KEY([BOCAUHOIID])
REFERENCES [dbo].[BOCAUHOI] ([ID])
ON DELETE SET NULL
GO
ALTER TABLE [dbo].[DETHI] CHECK CONSTRAINT [FK_DETHI_BOCAUHOI]
GO
ALTER TABLE [dbo].[KETQUATHI]  WITH CHECK ADD FOREIGN KEY([LICHTHIID])
REFERENCES [dbo].[LICHTHI] ([ID])
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[KETQUATHI]  WITH CHECK ADD FOREIGN KEY([SINHVIENID])
REFERENCES [dbo].[SINHVIEN] ([SINHVIENID])
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[LichSuLamBai]  WITH CHECK ADD  CONSTRAINT [FK_LichSuLamBai_Dethi] FOREIGN KEY([MaDeThi])
REFERENCES [dbo].[DETHI] ([ID])
GO
ALTER TABLE [dbo].[LichSuLamBai] CHECK CONSTRAINT [FK_LichSuLamBai_Dethi]
GO
ALTER TABLE [dbo].[LichSuLamBai]  WITH CHECK ADD  CONSTRAINT [FK_LichSuLamBai_Lichthi] FOREIGN KEY([LichThiId])
REFERENCES [dbo].[LICHTHI] ([ID])
GO
ALTER TABLE [dbo].[LichSuLamBai] CHECK CONSTRAINT [FK_LichSuLamBai_Lichthi]
GO
ALTER TABLE [dbo].[LichSuLamBai]  WITH CHECK ADD  CONSTRAINT [FK_LichSuLamBai_Sinhvien] FOREIGN KEY([MaSinhVien])
REFERENCES [dbo].[SINHVIEN] ([SINHVIENID])
GO
ALTER TABLE [dbo].[LichSuLamBai] CHECK CONSTRAINT [FK_LichSuLamBai_Sinhvien]
GO
ALTER TABLE [dbo].[LICHTHI]  WITH CHECK ADD FOREIGN KEY([DETHIID])
REFERENCES [dbo].[DETHI] ([ID])
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[LICHTHI]  WITH CHECK ADD FOREIGN KEY([LOPHOCID])
REFERENCES [dbo].[LOPHOC] ([ID])
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[LICHTHI_SINHVIEN]  WITH CHECK ADD  CONSTRAINT [FK_LICHTHI_SINHVIEN_LICHTHI] FOREIGN KEY([LICHTHIID])
REFERENCES [dbo].[LICHTHI] ([ID])
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[LICHTHI_SINHVIEN] CHECK CONSTRAINT [FK_LICHTHI_SINHVIEN_LICHTHI]
GO
ALTER TABLE [dbo].[LICHTHI_SINHVIEN]  WITH CHECK ADD  CONSTRAINT [FK_LICHTHI_SINHVIEN_SINHVIEN] FOREIGN KEY([SINHVIENID])
REFERENCES [dbo].[SINHVIEN] ([SINHVIENID])
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[LICHTHI_SINHVIEN] CHECK CONSTRAINT [FK_LICHTHI_SINHVIEN_SINHVIEN]
GO
ALTER TABLE [dbo].[NGUOIDUNG]  WITH CHECK ADD FOREIGN KEY([LOAINGUOIDUNGID])
REFERENCES [dbo].[LOAINGUOIDUNG] ([ID])
GO
ALTER TABLE [dbo].[NGUOIDUNG_LOAI]  WITH CHECK ADD FOREIGN KEY([LOAINGUOIDUNGID])
REFERENCES [dbo].[LOAINGUOIDUNG] ([ID])
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[NGUOIDUNG_LOAI]  WITH CHECK ADD FOREIGN KEY([NGUOIDUNGID])
REFERENCES [dbo].[NGUOIDUNG] ([ID])
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[SINHVIEN]  WITH CHECK ADD FOREIGN KEY([LOPHOCID])
REFERENCES [dbo].[LOPHOC] ([ID])
ON DELETE SET NULL
GO
ALTER TABLE [dbo].[SINHVIEN]  WITH CHECK ADD FOREIGN KEY([SINHVIENID])
REFERENCES [dbo].[NGUOIDUNG] ([ID])
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[TRALOI_SINHVIEN]  WITH CHECK ADD FOREIGN KEY([CAUHOIID])
REFERENCES [dbo].[CAUHOI] ([ID])
GO
ALTER TABLE [dbo].[TRALOI_SINHVIEN]  WITH CHECK ADD FOREIGN KEY([DAPANID])
REFERENCES [dbo].[DAPAN] ([ID])
ON DELETE SET NULL
GO
ALTER TABLE [dbo].[TRALOI_SINHVIEN]  WITH CHECK ADD FOREIGN KEY([SINHVIENID])
REFERENCES [dbo].[SINHVIEN] ([SINHVIENID])
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[TRALOI_SINHVIEN]  WITH CHECK ADD  CONSTRAINT [FK_TRALOI_SINHVIEN_Ketquathi_KetquathiId] FOREIGN KEY([KetquathiId])
REFERENCES [dbo].[KETQUATHI] ([ID])
GO
ALTER TABLE [dbo].[TRALOI_SINHVIEN] CHECK CONSTRAINT [FK_TRALOI_SINHVIEN_Ketquathi_KetquathiId]
GO
ALTER TABLE [dbo].[CAUHOI]  WITH CHECK ADD CHECK  (([LOAICAUHOI]=(1) OR [LOAICAUHOI]=(0)))
GO
