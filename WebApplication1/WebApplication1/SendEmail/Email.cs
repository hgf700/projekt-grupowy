using System.Net;
using System.Net.Mail;

namespace WebApplication1.SendEmail
{
    public class Email
    {
        public void SendEmail(string toEmail, string subject, string body)
        {
            // Pobieranie z ENV / konfiguracji
            string senderEmail = Environment.GetEnvironmentVariable("SENDER_EMAIL");
            string senderPassword = Environment.GetEnvironmentVariable("SENDER_PASSWORD");

            if (string.IsNullOrEmpty(senderEmail) || string.IsNullOrEmpty(senderPassword))
            {
                throw new Exception("Brakuje zmiennych środowiskowych: EMAIL_USER lub EMAIL_PASS");
            }

            var fromAddress = new MailAddress(senderEmail, "bilety Ø dostawca", System.Text.Encoding.UTF8);
            var toAddress = new MailAddress(toEmail);

            using (var message = new MailMessage(fromAddress, toAddress)
            {
                Subject = subject + " ←↑→↓",
                Body = body + "\nStrzałki: ←↑→↓",
                SubjectEncoding = System.Text.Encoding.UTF8,
                BodyEncoding = System.Text.Encoding.UTF8
            })
            {
                var smtp = new SmtpClient
                {
                    Host = "smtp.gmail.com",
                    Port = 587,
                    EnableSsl = true,
                    DeliveryMethod = SmtpDeliveryMethod.Network,
                    Credentials = new NetworkCredential(senderEmail, senderPassword),
                    Timeout = 20000
                };

                smtp.Send(message);
            }
        }
    }
}
