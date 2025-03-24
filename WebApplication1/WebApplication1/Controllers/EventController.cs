using Microsoft.AspNetCore.Mvc;
using WebApplication1.Models;
using System.Threading.Tasks;

namespace WebApplication1.Controllers
{
    public class EventController : Controller
    {
        [HttpGet]
        public async Task<IActionResult> JoinToEvent()
        {
            // Przykładowy obiekt Event do wyświetlenia w widoku
            var eventDetails = new Event
            {
                Id = 1,
                Title = "Sample Event",
                StartDate = DateTime.Now.AddDays(2),
                EndDate = DateTime.Now.AddDays(2).AddHours(4),
                Adress = "Sample Address",
                TypeOfMusic = "Rock",
                Url = "https://example.com",
                PhotoUrl = "https://via.placeholder.com/400x200",
                User = new User { Name = "John Doe" }
            };

            return View(eventDetails); // Przekazujemy eventDetails do widoku
        }
    }
}
