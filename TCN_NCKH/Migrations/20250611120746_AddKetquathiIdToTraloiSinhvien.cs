using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TCN_NCKH.Migrations
{
    /// <inheritdoc />
    public partial class AddKetquathiIdToTraloiSinhvien : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "KetquathiId",
                table: "TRALOI_SINHVIEN", // Tên bảng của mi có thể là "TraloiSinhviens" hoặc "TraloiSinhvien" tùy cách mi cấu hình
                type: "int",
                nullable: true); // Hoặc false nếu mi muốn nó là bắt buộc (tùy thuộc vào yêu cầu nghiệp vụ)

            migrationBuilder.CreateIndex(
                name: "IX_TRALOI_SINHVIEN_KetquathiId", // Tên index
                table: "TRALOI_SINHVIEN",
                column: "KetquathiId");

            migrationBuilder.AddForeignKey(
                name: "FK_TRALOI_SINHVIEN_Ketquathi_KetquathiId", // Tên khóa ngoại
                table: "TRALOI_SINHVIEN",
                column: "KetquathiId",
                principalTable: "Ketquathi", // Tên bảng Ketquathi của mi
                principalColumn: "Id");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_TRALOI_SINHVIEN_Ketquathi_KetquathiId",
                table: "TRALOI_SINHVIEN");

            migrationBuilder.DropIndex(
                name: "IX_TRALOI_SINHVIEN_KetquathiId",
                table: "TRALOI_SINHVIEN");

            migrationBuilder.DropColumn(
                name: "KetquathiId",
                table: "TRALOI_SINHVIEN");
        }

    }
}
