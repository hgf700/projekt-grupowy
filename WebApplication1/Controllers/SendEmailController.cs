//using Microsoft.AspNetCore.Mvc;
//using WebApplication1.ProjectSERVICES;

//namespace WebApplication1.Controllers
//{
//    [ApiController]
//    [Route("email")]
//    public class SendEmailController : Controller
//    {
//        private readonly EmailService _emailService;
//        string docelowyemail = Environment.GetEnvironmentVariable("TARGET_EMAIL");
//        public SendEmailController()
//        {
//            // Możesz też użyć DI, ale dla prostoty konstruktor bezparametrowy
//            _emailService = new EmailService();
//        }

//        [HttpGet("send")]
//        public IActionResult Index()
//        {
//            return View();
//        }

//        [HttpPost("send")]
//        public IActionResult SendEmailForm()
//        {
//            string toEmail = docelowyemail;

//            try
//            {
//                _emailService.SendEmail(toEmail);
//                ViewBag.Message = "Email został wysłany.";
//            }
//            catch (Exception ex)
//            {
//                ViewBag.Message = "Błąd podczas wysyłania emaila: " + ex.Message;
//            }

//            return View("Index");
//        }
//    }
//}
