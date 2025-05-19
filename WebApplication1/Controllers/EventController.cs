using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;
using WebApplication1.Areas.Identity.Data;
using WebApplication1.Models;

namespace WebApplication1.Controllers
{
    [Route("Event")]
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
        public async Task<IActionResult> Index(int pageNumber = 1, int pageSize = 20, string city = null)
        {
            IQueryable<Event> query = _context.Events;

            //await FetchAndSaveEvents();

            if (!string.IsNullOrWhiteSpace(city))
            {
                ViewBag.City = city;
                query = query.Where(e => e.Address.Contains(city) || e.NameOfClub.Contains(city));
            }

            var allEvents = await query.OrderBy(e => e.StartOfEvent).ToListAsync();

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

            return View("Index", paginatedEvents);
        }

        [HttpGet("SearchRequiredEvent")]
        public async Task<IActionResult> SearchRequiredEvent(string city, int pageNumber = 1)
        {
            if (string.IsNullOrWhiteSpace(city))
                return BadRequest("Miasto jest wymagane.");

            try
            {
                string apiKey = Environment.GetEnvironmentVariable("TICKETMASTER_API_KEY");
                if (string.IsNullOrWhiteSpace(apiKey))
                    return StatusCode(500, "Brak klucza API");

                string baseUrl = "https://app.ticketmaster.com/discovery/v2/events.json";
                var query = new Dictionary<string, string>
                {
                    { "apikey", apiKey },
                    { "size", "20" },
                    { "city", city },
                    { "page", (pageNumber - 1).ToString() } // Ticketmaster uses 0-based indexing
                };

                string url = QueryHelpers.AddQueryString(baseUrl, query);
                HttpResponseMessage response = await _httpClient.GetAsync(url).ConfigureAwait(false);

                if (!response.IsSuccessStatusCode)
                    return StatusCode((int)response.StatusCode, "Błąd podczas pobierania danych z Ticketmaster API");

                string json = await response.Content.ReadAsStringAsync().ConfigureAwait(false);

                var ticketmasterData = JsonSerializer.Deserialize<TicketmasterResponse>(json, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

                var externalEvents = ticketmasterData?.Embedded?.Events;

                if (externalEvents == null || !externalEvents.Any())
                    return NotFound("Nie znaleziono wydarzeń.");

                var existingEventIds = _context.Events
                    .AsNoTracking()
                    .Select(e => e.ExternalEventId)
                    .ToHashSet();

                var newEvents = externalEvents
                    .Where(ev => !existingEventIds.Contains(ev.Id))
                    .Select(ev =>
                    {
                        var venue = ev.Embedded?.Venues?.FirstOrDefault();

                        return new WebApplication1.Models.Event
                        {
                            ExternalEventId = ev.Id,
                            TypeOfEvent = ev.Type,
                            NameOfEvent = ev.Name,
                            UrlOfEvent = ev.Url,
                            PhotoUrl = ev.Images?.FirstOrDefault()?.Url,
                            StartOfEvent = DateTime.TryParse(ev.Dates?.Start?.DateTime, out var start) ? start : DateTime.MinValue,
                            Address = venue?.Address?.Line1,
                            City = venue?.City?.Name,
                            Country = venue?.Country?.Name,
                            NameOfClub = venue?.Name,
                        };
                    })
                    .ToList();

                if (newEvents.Any())
                {
                    _context.Events.AddRange(newEvents);
                    await _context.SaveChangesAsync().ConfigureAwait(false);
                }

                // Dodaj ViewBag do paginacji
                ViewBag.City = city;
                ViewBag.CurrentPage = pageNumber;
                ViewBag.TotalPages = ticketmasterData?.Page?.TotalPages ?? 1;

                return View("Index", newEvents);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Wystąpił błąd: {ex.Message}");
            }
        }



    }
}

