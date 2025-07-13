using MailKit.Net.Smtp;
using MailKit.Security;
using MimeKit;
using Microsoft.Extensions.Configuration;
using System.Threading.Tasks;
using System;
using System.IO;

namespace TCN_NCKH.Services
{
    // Thêm ': IEmailService' vào đây
    public class EmailService : IEmailService
    {
        private readonly IConfiguration _configuration;

        // Trong Services/EmailService.cs
        public EmailService(IConfiguration configuration)
        {
            _configuration = configuration;
            // DEBUG: In ra cấu hình để kiểm tra
            Console.WriteLine($"DEBUG EmailService Config: Host={_configuration["EmailSettings:SmtpServer"]}, Port={_configuration["EmailSettings:SmtpPort"]}");
            Console.WriteLine($"DEBUG EmailService Config: Username={_configuration["EmailSettings:Username"]}, Password={_configuration["EmailSettings:Password"]}");
        }

        public async Task SendEmailAsync(string toEmail, string subject, string body, byte[]? attachmentBytes = null, string? attachmentFileName = null)
        {
            var emailSettings = _configuration.GetSection("EmailSettings");

            var message = new MimeMessage();
            message.From.Add(new MailboxAddress(emailSettings["SenderName"], emailSettings["SenderEmail"]));
            message.To.Add(new MailboxAddress("", toEmail));
            message.Subject = subject;

            var bodyBuilder = new BodyBuilder { HtmlBody = body };

            if (attachmentBytes != null && !string.IsNullOrEmpty(attachmentFileName))
            {
                bodyBuilder.Attachments.Add(attachmentFileName, attachmentBytes, ContentType.Parse("application/pdf"));
            }

            message.Body = bodyBuilder.ToMessageBody();

            using (var client = new SmtpClient())
            {
                try
                {
                    await client.ConnectAsync(
                        emailSettings["SmtpServer"],
                        int.Parse(emailSettings["SmtpPort"]),
                        SecureSocketOptions.StartTls
                    );
                    await client.AuthenticateAsync(emailSettings["Username"], emailSettings["Password"]);
                    await client.SendAsync(message);
                    await client.DisconnectAsync(true);
                    Console.WriteLine($"DEBUG: Email đã được gửi thành công đến {toEmail}");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"ERROR: Gửi email đến {toEmail} thất bại: {ex.Message}");
                    Console.WriteLine($"ERROR: Inner Exception: {ex.InnerException?.Message ?? "N/A"}");
                    // Quan trọng: Ném lại exception để Controller biết có lỗi và xử lý
                    throw; // Ném lại lỗi để controller có thể bắt và trả về 500
                }
            }
        }
    }
}
