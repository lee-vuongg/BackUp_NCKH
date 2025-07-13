using System.Threading.Tasks;

namespace TCN_NCKH.Services
{
    public interface IEmailService
    {
        // Định nghĩa phương thức gửi email giống hệt trong EmailService.cs
        Task SendEmailAsync(string toEmail, string subject, string body, byte[]? attachmentBytes = null, string? attachmentFileName = null);
    }
}
