using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuestPDF.Fluent;
using Stripe;
using WebApplication1.Areas.Identity.Data;
using WebApplication1.Models;
using WebApplication1.ProjectSERVICES;

namespace WebApplication1.Controllers
{
    [ApiController]
    [Route("email")]
    public class SendEmailController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly QrService _qrService;
        private readonly SmsService _smsservice;
        private readonly EmailService _emailService;
        public SendEmailController(ApplicationDbContext context, QrService qrService, SmsService smsservice, EmailService emailService)
        {
            _context = context;
            _qrService = qrService;
            _smsservice = smsservice;
            _emailService = emailService;
        }


        [HttpGet("send")]
        public IActionResult Index()
        {
            return View();
        }

        [HttpPost("send")]
        public async Task<IActionResult> SendEmailForm()
        {

            try
            {
                int id = 1;

                var ev = await _context.Events.FindAsync(id);
                if (ev == null)
                    return NotFound();

                _qrService.GenerateQrCode(ev.UrlOfEvent);

                bool.TryParse(Environment.GetEnvironmentVariable("TWILIO_SMS_SEND_STATE"), out bool twilio_sms_state);

                if (twilio_sms_state == true)
                {
                    _smsservice.SendSMS(ev.UrlOfEvent);
                }

                var doc = new InvoiceDocument(
                    eventName: $"{ev.NameOfEvent}",
                    eventDate: $"{ev.StartOfEvent}",
                    eventAddress: $"{ev.Address}",
                    eventType: $"{ev.TypeOfEvent}"
                );

                string resourcesPath = Path.Combine(Directory.GetCurrentDirectory(), "Resources");
                string pdfPath = Path.Combine(resourcesPath, "bilet.pdf");
                doc.GeneratePdf(pdfPath);

                string docelowyemail = Environment.GetEnvironmentVariable("TARGET_EMAIL");

                _emailService.SendEmail(docelowyemail, ev.UrlOfEvent);

                ViewBag.Message = "Płatność zakończona sukcesem!";
                return View("Index");

            }
            catch (Exception ex)
            {
                ViewBag.Message = "Błąd podczas wysyłania emaila: " + ex.Message;
            }

            return View("Index");
        }
    }
}
