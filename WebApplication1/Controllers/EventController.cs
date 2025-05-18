using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;
using WebApplication1.Areas.Identity.Data;
using WebApplication1.Models;

namespace WebApplication1.Controllers
{
    [Route("")]
    public class EventController : Controller
    {
        private readonly HttpClient _httpClient;
        private readonly ApplicationDbContext _context;

        public EventController(HttpClient httpClient, ApplicationDbContext context)
        {
            _httpClient = httpClient;
            _context = context;
        }

        [HttpGet("")]
        public async Task<IActionResult> Index(int pageNumber = 1, int pageSize = 20)
        {
            //await FetchAndSaveEvents();

            // Pobierz wszystkie eventy z bazy danych (można też filtrować wcześniej po potrzebie)
            var allEvents = await _context.Events
                .OrderBy(e => e.StartOfEvent)
                .ToListAsync();

            // Grupowanie po nazwie, i wybieramy pierwszy event z każdej grupy
            var uniqueEvents = allEvents
                .GroupBy(e => e.NameOfEvent)
                .Select(g => g.First())
                .ToList();

            var totalEvents = uniqueEvents.Count;

            var paginatedEvents = uniqueEvents
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            ViewBag.TotalPages = (int)Math.Ceiling(totalEvents / (double)pageSize);
            ViewBag.CurrentPage = pageNumber;

            return View("EventList", paginatedEvents);
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
