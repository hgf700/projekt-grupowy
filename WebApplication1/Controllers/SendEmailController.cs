using Microsoft.AspNetCore.Mvc;
using WebApplication1.SendEmail;

namespace WebApplication1.Controllers
{
    [ApiController]
    [Route("email")]
    public class SendEmailController : Controller
    {
        private readonly Email _emailService;
        string docelowyemail = Environment.GetEnvironmentVariable("TARGET_EMAIL");
        public SendEmailController()
        {
            // Możesz też użyć DI, ale dla prostoty konstruktor bezparametrowy
            _emailService = new Email();
        }

        [HttpGet("send")]
        public IActionResult Index()
        {
            return View();
        }

        [HttpPost("send")]
        public IActionResult SendEmailForm()
        {
            string toEmail = docelowyemail;
            string subject = "Testowy temat";
            string body = @"
                <html>
                  <body style='background-color:#f0f0f0; padding:20px; font-family:Arial;'>
                    <h2 style='color:#333;'>Wiadomość testowa</h2>
                    <p>To jest testowy email z grafiką.</p>
                    <img src='cid:testImage' alt='test'>
                    <img src='cid:QRimage' alt='QR'>
                  </body>
                </html>";

            try
            {
                _emailService.SendEmail(toEmail, subject, body);
                ViewBag.Message = "Email został wysłany.";
            }
            catch (Exception ex)
            {
                ViewBag.Message = "Błąd podczas wysyłania emaila: " + ex.Message;
            }

            return View("Index");
        }
    }
}
