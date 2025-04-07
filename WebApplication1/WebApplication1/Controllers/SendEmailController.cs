using Microsoft.AspNetCore.Mvc;
using WebApplication1.SendEmail;

namespace WebApplication1.Controllers
{
    public class SendEmailController : Controller
    {
        private readonly Email _emailService;
        string docelowyemail = Environment.GetEnvironmentVariable("TARGET_EMAIL");
        public SendEmailController()
        {
            // Możesz też użyć DI, ale dla prostoty konstruktor bezparametrowy
            _emailService = new Email();
        }

        [HttpGet]
        public IActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public IActionResult SendEmailForm()
        {
            string toEmail = docelowyemail;
            string subject = "Testowy temat";
            string body = "Wiadomość testowa";

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
