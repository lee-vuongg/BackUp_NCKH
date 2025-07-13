// Services/PdfReportService.cs
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using TCN_NCKH.Models.DBModel;

namespace TCN_NCKH.Services
{
    public class PdfReportService
    {
        public byte[] GenerateExamResultPdf(LichSuLamBai lichSuLamBai, Ketquathi ketQuaThi, List<TraloiSinhvien> traLoiSinhVien, List<Cauhoi> cauHois)
        {
            // Đăng ký font tiếng Việt (nếu cần).
            // Mi cần đảm bảo rằng font này tồn tại trên hệ thống hoặc mi cung cấp file .ttf
            // Ví dụ: FontManager.RegisterSystemFonts(); // Đăng ký tất cả các font hệ thống
            // Hoặc: FontManager.RegisterFont(File.ReadAllBytes("Fonts/TimesNewRoman.ttf"));
            // Nếu không đăng ký, các ký tự tiếng Việt có thể không hiển thị đúng.
            // Để đơn giản cho ví dụ, tôi sẽ giả định Times New Roman có sẵn hoặc được xử lý bởi QuestPDF.

            return Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.A4);
                    page.Margin(30);
                    page.PageColor(Colors.White);
                    // Sử dụng FontFamily "Times New Roman" hoặc "Arial" để hỗ trợ tiếng Việt tốt hơn
                    page.DefaultTextStyle(x => x.FontSize(10).FontFamily("Times New Roman"));

                    string tenDeThi = lichSuLamBai.MaDeThiNavigation?.Tendethi ?? lichSuLamBai.LichThi?.Dethi?.Tendethi ?? "Không rõ đề thi";
                    string tenSinhVien = lichSuLamBai.MaSinhVienNavigation?.SinhvienNavigation?.Hoten ?? lichSuLamBai.MaSinhVien ?? "Không rõ sinh viên";

                    // Header
                    page.Header()
                        .PaddingBottom(15)
                        .Column(column =>
                        {
                            column.Item().Text("BÁO CÁO KẾT QUẢ THI").SemiBold().FontSize(18).AlignCenter().FontColor(Colors.Blue.Darken2);
                            column.Item().Text(tenDeThi.ToUpper()).SemiBold().FontSize(14).AlignCenter().FontColor(Colors.Blue.Darken1);
                        });

                    // Content
                    page.Content()
                        .PaddingVertical(10)
                        .Column(column =>
                        {
                            column.Spacing(15); // Khoảng cách giữa các phần chính

                            // Thông tin thí sinh và kết quả tóm tắt
                            column.Item().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).PaddingBottom(5)
                                .Text("THÔNG TIN THÍ SINH & KẾT QUẢ").SemiBold().FontSize(12).FontColor(Colors.Grey.Darken2);

                            column.Item().Table(table =>
                            {
                                table.ColumnsDefinition(columns =>
                                {
                                    columns.RelativeColumn();
                                    columns.RelativeColumn();
                                });

                                table.Cell().Text("Họ và tên:").SemiBold();
                                table.Cell().Text(tenSinhVien);

                                table.Cell().Text("Mã số sinh viên:").SemiBold();
                                table.Cell().Text(lichSuLamBai.MaSinhVien);

                                table.Cell().Text("Ngày thi:").SemiBold();
                                table.Cell().Text(lichSuLamBai.ThoiDiemKetThuc?.ToString("dd/MM/yyyy HH:mm") ?? "N/A");

                                TimeSpan? duration = null;
                                if (lichSuLamBai.ThoiDiemBatDau.HasValue && lichSuLamBai.ThoiDiemKetThuc.HasValue)
                                {
                                    duration = lichSuLamBai.ThoiDiemKetThuc.Value - lichSuLamBai.ThoiDiemBatDau.Value;
                                }
                                table.Cell().Text("Thời gian làm bài:").SemiBold();
                                table.Cell().Text(duration.HasValue ? $"{duration.Value.TotalMinutes:F0} phút" : "N/A");

                                table.Cell().Text("Điểm đạt được:").SemiBold().FontSize(11).FontColor(Colors.Blue.Darken2);
                                table.Cell().Text(ketQuaThi.Diem?.ToString("F2") ?? "Chưa chấm")
                                    .SemiBold().FontSize(11).FontColor(Colors.Green.Darken2);
                            });

                            // Chi tiết bài làm
                            column.Item().PaddingTop(15).BorderBottom(1).BorderColor(Colors.Grey.Lighten2).PaddingBottom(5)
                                .Text("CHI TIẾT BÀI LÀM").SemiBold().FontSize(12).FontColor(Colors.Grey.Darken2);

                            int questionNumber = 1;
                            foreach (var cauHoi in cauHois.OrderBy(c => c.Id))
                            {
                                column.Item().Column(questionColumn =>
                                {
                                    questionColumn.Spacing(5);
                                    questionColumn.Item().Text($"Câu {questionNumber++}: {cauHoi.Noidung}")
                                        .SemiBold().FontSize(10).FontColor(Colors.Black);

                                    // Lấy các đáp án sinh viên đã chọn
                                    var dapAnSinhVienChonIds = traLoiSinhVien
                                        .Where(tl => tl.Cauhoiid == cauHoi.Id && tl.Dapanid.HasValue)
                                        .Select(tl => tl.Dapanid.Value)
                                        .ToHashSet();

                                    // Lấy các đáp án đúng của câu hỏi (ưu tiên Dung, sau đó là DapandungKeys)
                                    List<int> correctDapanIds = new List<int>();
                                    // Kiểm tra thuộc tính Dung trên Dapans trước
                                    if (cauHoi.Dapans != null && cauHoi.Dapans.Any(d => d.Dung == true)) // ĐÃ SỬA: d.Dung == true
                                    {
                                        correctDapanIds = cauHoi.Dapans.Where(d => d.Dung == true).Select(d => d.Id).ToList(); // ĐÃ SỬA: d.Dung == true
                                    }
                                    // Nếu không có Dung, thử dùng DapandungKeys (logic cũ của mi)
                                    else if (!string.IsNullOrEmpty(cauHoi.DapandungKeys))
                                    {
                                        var parsedInts = cauHoi.DapandungKeys
                                            .Split(',', StringSplitOptions.RemoveEmptyEntries)
                                            .Select(s => int.TryParse(s.Trim(), out int id) ? (int?)id : null)
                                            .Where(id => id.HasValue)
                                            .Select(id => id.Value)
                                            .ToList();

                                        if (parsedInts.Any())
                                        {
                                            correctDapanIds = parsedInts;
                                        }
                                        else
                                        {
                                            var correctKeys = cauHoi.DapandungKeys
                                                .Split(',', StringSplitOptions.RemoveEmptyEntries)
                                                .Select(s => s.Trim().ToUpper())
                                                .ToList();

                                            var allDapansOrdered = cauHoi.Dapans?.OrderBy(d => d.Id).ToList() ?? new List<Dapan>();
                                            foreach (var key in correctKeys)
                                            {
                                                if (key.Length == 1 && char.IsLetter(key[0]))
                                                {
                                                    int index = key[0] - 'A';
                                                    if (index >= 0 && index < allDapansOrdered.Count)
                                                    {
                                                        correctDapanIds.Add(allDapansOrdered[index].Id);
                                                    }
                                                }
                                            }
                                        }
                                    }


                                    foreach (var dapan in cauHoi.Dapans.OrderBy(d => d.Id))
                                    {
                                        string prefix = "[ ]";
                                        bool isStudentChosen = dapAnSinhVienChonIds.Contains(dapan.Id);
                                        bool isCorrectAnswer = correctDapanIds.Contains(dapan.Id);

                                        TextStyle currentStyle = TextStyle.Default.FontSize(9); // Kích thước chữ nhỏ hơn cho đáp án

                                        if (isCorrectAnswer)
                                        {
                                            currentStyle = currentStyle.FontColor(Colors.Green.Darken2).SemiBold();
                                        }

                                        if (isStudentChosen)
                                        {
                                            prefix = "[X]";
                                            // Nếu sinh viên chọn và đó là đáp án đúng, màu xanh
                                            // Nếu sinh viên chọn và đó là đáp án sai, màu đỏ
                                            currentStyle = currentStyle.FontColor(isCorrectAnswer ? Colors.Green.Darken2 : Colors.Red.Darken2).Italic();
                                        }
                                        else if (isCorrectAnswer)
                                        {
                                            // Nếu là đáp án đúng nhưng sinh viên không chọn, vẫn hiển thị màu xanh
                                            currentStyle = currentStyle.FontColor(Colors.Green.Darken2).SemiBold();
                                        }
                                        else
                                        {
                                            // Đáp án không được chọn và không phải là đáp án đúng
                                            currentStyle = currentStyle.FontColor(Colors.Black);
                                        }


                                        questionColumn.Item().Text(text =>
                                        {
                                            text.Span(prefix).Style(currentStyle.FontFamily("Courier New")); // Font monospace cho checkbox
                                            text.Span(" ").Style(currentStyle);
                                            text.Span(dapan.Noidung).Style(currentStyle);
                                        });
                                    }
                                });
                                column.Item().PaddingBottom(10).LineHorizontal(1).LineColor(Colors.Grey.Lighten4); // Dòng phân cách giữa các câu hỏi
                            }
                        });

                    // Footer
                    page.Footer()
                        .AlignCenter()
                        .Text(x =>
                        {
                            x.Span("Trang ").FontSize(9);
                            x.CurrentPageNumber().FontSize(9);
                            x.Span(" / ").FontSize(9);
                            x.TotalPages().FontSize(9);
                        });
                });
            }).GeneratePdf();
        }
    }
}
