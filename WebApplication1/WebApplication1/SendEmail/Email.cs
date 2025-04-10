using System.Net;
using System.Net.Mail;

namespace WebApplication1.SendEmail
{
    public class Email
    {
        public void SendEmail(string toEmail, string subject, string body)
        {
            // Mailtrap SMTP dane z ENV
            string smtpUser = Environment.GetEnvironmentVariable("MAILTRAP_SENDER_USER");
            string smtpPass = Environment.GetEnvironmentVariable("MAILTRAP_SENDER_PASS");

            if (string.IsNullOrEmpty(smtpUser) || string.IsNullOrEmpty(smtpPass))
            {
                throw new Exception("Brakuje zmiennych środowiskowych: MAILTRAP_USER lub MAILTRAP_PASS");
            }

            var fromAddress = new MailAddress("test@example.com", "Mailtrap Test", System.Text.Encoding.UTF8);
            var toAddress = new MailAddress(toEmail);

            using (var message = new MailMessage(fromAddress, toAddress)
            {
                Subject = subject,
                Body = body,
                SubjectEncoding = System.Text.Encoding.UTF8,
                BodyEncoding = System.Text.Encoding.UTF8
            })
            {
                var smtp = new SmtpClient
                {
                    Host = "sandbox.smtp.mailtrap.io",
                    Port = 2525, 
                    EnableSsl = true,
                    DeliveryMethod = SmtpDeliveryMethod.Network,
                    Credentials = new NetworkCredential(smtpUser, smtpPass),
                    Timeout = 20000
                };

                smtp.Send(message);
                System.Console.WriteLine("Sent");
            }
        }
    }
}


