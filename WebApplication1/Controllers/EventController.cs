using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;
using WebApplication1.Models;

namespace WebApplication1.Controllers
{
    [Route("")]
    public class EventController : Controller
    {
        private readonly HttpClient _httpClient;
        private readonly event_base _context;

        public EventController(HttpClient httpClient, event_base context)
        {
            _httpClient = httpClient;
            _context = context;
        }

        [HttpGet("")]
        public async Task<IActionResult> Index()
        {
            await FetchAndSaveEvents();

            var events = await _context.Events
                .OrderBy(e => e.StartOfEvent)
                .ToListAsync();

            return View("EventList", events);
        }

        private async Task FetchAndSaveEvents()
        {
            try
            {
                string apiKey = Environment.GetEnvironmentVariable("TICKETMASTER_API_KEY");
                string baseUrl = "https://app.ticketmaster.com/discovery/v2/events.json";

                var query = new Dictionary<string, string>
                {
                    { "apikey", apiKey },
                    { "size", "100" },
                    { "countryCode", "PL" }
                };

                string url = QueryHelpers.AddQueryString(baseUrl, query);
                HttpResponseMessage response = await _httpClient.GetAsync(url);

                if (!response.IsSuccessStatusCode)
                    return;

                string json = await response.Content.ReadAsStringAsync();
                var ticketmasterData = JsonSerializer.Deserialize<TicketmasterResponse>(json);

                if (ticketmasterData?.Embedded?.Events == null)
                    return;

                foreach (var ev in ticketmasterData.Embedded.Events)
                {
                    if (_context.Events.Any(e => e.ExternalEventId == ev.Id))
                        continue;

                    var newEvent = new WebApplication1.Models.Event
                    {
                        ExternalEventId = ev.Id,
                        TypeOfEvent = ev.Type,
                        NameOfEvent = ev.Name,
                        UrlOfEvent = ev.Url,
                        PhotoUrl = ev.Images?.FirstOrDefault()?.Url,
                        SalesStartDate = DateTime.Parse(ev.Sales?.Public?.StartDateTime ?? DateTime.MinValue.ToString()),
                        SalesEndDate = DateTime.Parse(ev.Sales?.Public?.EndDateTime ?? DateTime.MinValue.ToString()),
                        StartOfEvent = DateTime.Parse(ev.Dates?.Start?.DateTime ?? DateTime.MinValue.ToString()),
                        EndOfEvent = DateTime.Parse(ev.Dates?.End?.DateTime ?? DateTime.MinValue.ToString()),
                        Address = ev.Embedded?.Venues?.FirstOrDefault()?.Address?.Line1,
                        NameOfClub = ev.Embedded?.Venues?.FirstOrDefault()?.Name,
                        Classifications = ev.Type
                    };

                    _context.Events.Add(newEvent);
                }

                await _context.SaveChangesAsync();
            }
            catch
            {
                // Można dodać logowanie błędu
            }
        }
    }
}
