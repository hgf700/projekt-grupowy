using Microsoft.AspNetCore.Mvc;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using WebApplication1.Models;

namespace WebApplication1.Controllers
{
    public class EventController : Controller
    {
        private readonly HttpClient _httpClient;

        public EventController(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        [HttpGet]
        public IActionResult JoinToEvent()
        {
            return View();
        }

        //Chyba trzeba dac tylko pola z miastem i moze data? 

        [HttpPost]
        public async Task<IActionResult> JoinToEvent(Event eventDetails)
        {
            if (!ModelState.IsValid)
            {
                return View(eventDetails);
            }

            var json = JsonSerializer.Serialize(eventDetails);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await _httpClient.PostAsync("https://api.example.com/create-event", content);

            //if (response.IsSuccessStatusCode)
            //{
            //    TempData["SuccessMessage"] = "Event successfully created!";
            //    return RedirectToAction("JoinToEvent");
            //}

            //TempData["ErrorMessage"] = "Error while creating event.";
            return View(eventDetails);
        }
    }
}
