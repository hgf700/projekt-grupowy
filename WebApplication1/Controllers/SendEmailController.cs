using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuestPDF.Fluent;
using WebApplication1.Areas.Identity.Data;
using WebApplication1.ProjectSERVICES;

namespace WebApplication1.Controllers
{
    [ApiController]
    [Route("email")]
    public class SendEmailController : Controller
    {
        private readonly ApplicationDbContext _context;
        public SendEmailController(ApplicationDbContext context)
        {
            _context = context;
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
                int id = 143;
                var ev = await _context.Events.FindAsync(id);
                if (ev == null)
                    return NotFound();

                var doc = new InvoiceDocument(
                    eventName: $"{ev.NameOfEvent}",
                    eventDate: $"{ev.StartOfEvent}",
                    eventAddress: $"{ev.Address}"
                );

                string resourcesPath = Path.Combine(Directory.GetCurrentDirectory(), "Resources");
                string pdfPath = Path.Combine(resourcesPath, "bilet.pdf");
                doc.GeneratePdf(pdfPath);
            }
            catch (Exception ex)
            {
                ViewBag.Message = "Błąd podczas wysyłania emaila: " + ex.Message;
            }

            return View("Index");
        }
    }
}
