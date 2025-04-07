using Microsoft.AspNetCore.Mvc;

namespace WebApplication1.Controllers
{
    public class SendEmailController : Controller
    {
        Environment.GetEnvironmentVariable("EMAIL_USER"),
        public IActionResult Index()
        {
            return View();
        }
    }
}
