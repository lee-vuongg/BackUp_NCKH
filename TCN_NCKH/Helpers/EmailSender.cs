using System.Net;
using System.Net.Mail;
using System.Threading.Tasks;

namespace TCN_NCKH.Helpers
{
    public class EmailSender
    {
        public static async Task SendEmailAsync(string toEmail, string subject, string body)
        {
            var fromEmail = "lequocvuowng123@gmail.com";
            var password = "u w y n h f r t j u c f t f p z"; // không dùng mật khẩu Gmail thật

            var smtpClient = new SmtpClient("smtp.gmail.com")
            {
                Port = 587,
                Credentials = new NetworkCredential(fromEmail, password),
                EnableSsl = true,
            };

            var mailMessage = new MailMessage(fromEmail, toEmail, subject, body)
            {
                IsBodyHtml = true
            };

            await smtpClient.SendMailAsync(mailMessage);
        }
    }
}
