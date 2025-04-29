using System.Net;
using System.Net.Mail;
using System.Net.Mime;

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

            
            var plainView = AlternateView.CreateAlternateViewFromString("To jest tekstowa wersja wiadomości", null, "text/plain");
            AlternateView htmlView = AlternateView.CreateAlternateViewFromString(body, null, MediaTypeNames.Text.Html);

            
            MailAddress bcc = new MailAddress("manager1@contoso.com");
            MailAddress copy = new MailAddress("Notification_List@contoso.com");

            
            string dir = @"..\WebApplication1\Resources";
            Directory.SetCurrentDirectory(dir);

            string file = "cos.txt";
            // Create  the file attachment for this email message.
            Attachment data = new Attachment(file, MediaTypeNames.Text.Plain);
            data.TransferEncoding = TransferEncoding.Base64;
            // Add time stamp information for the file.
            ContentDisposition disposition = data.ContentDisposition;
            disposition.CreationDate = System.IO.File.GetCreationTime(file);
            disposition.ModificationDate = System.IO.File.GetLastWriteTime(file);
            disposition.ReadDate = System.IO.File.GetLastAccessTime(file);


            string imagePath = Path.Combine("test.jpg");
            LinkedResource image = new LinkedResource(imagePath, MediaTypeNames.Image.Jpeg);

            string pngPath = Path.Combine("QR.png");
            LinkedResource pngimage = new LinkedResource(pngPath, MediaTypeNames.Image.Png);

            image.ContentId = "testImage"; // ID dla cid
            image.TransferEncoding = TransferEncoding.Base64;

            pngimage.ContentId = "QRimage";
            pngimage.TransferEncoding = TransferEncoding.Base64;

            htmlView.LinkedResources.Add(image);
            htmlView.LinkedResources.Add(pngimage);

            using (var message = new MailMessage(fromAddress, toAddress)
            {
                Subject = subject,
                //Body = body, podwyzsza spam
                SubjectEncoding = System.Text.Encoding.UTF8,
                BodyEncoding = System.Text.Encoding.UTF8,
                IsBodyHtml = true,
                HeadersEncoding= System.Text.Encoding.UTF8,
                Priority= MailPriority.High,
            })
            {
                message.AlternateViews.Add(plainView);
                message.AlternateViews.Add(htmlView);
                message.Attachments.Add(data);
                message.Bcc.Add(bcc);
                message.CC.Add(copy);

                string messageId = $"<{Guid.NewGuid()}@{Dns.GetHostName()}>";

                message.Headers.Add("Message-Id", messageId);
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
}
