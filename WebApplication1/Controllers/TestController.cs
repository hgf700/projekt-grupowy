using Microsoft.AspNetCore.Mvc;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.WebUtilities;
using System.Text.Json;
using WebApplication1.Models;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;

public class TestController : Controller
{
    private readonly event_base _context;
    private readonly HttpClient _httpClient;

    public TestController(HttpClient httpClient, event_base context)
    {
        _httpClient = httpClient;
        _context=context;
    }

    //testowe narazie
    [HttpGet]
    public async Task<IActionResult> GetEvents()
    {
        try
        {
            string apiKey = Environment.GetEnvironmentVariable("API_KEY");
            string baseUrl = "https://app.ticketmaster.com/discovery/v2/events.json";

            // Parametry zapytania masz dc dokumentacje
            //po wpisaniu w url /api/events wyswietli sie json
            // lepsze to bo automatycznei masz i nie musisz robic ?size=1&.......
            var query = new Dictionary<string, string>
            {
                { "apikey", apiKey },
                { "size", "1" },
                { "city", "Krakow" },
                { "unit", "km" },
                { "radius", "1" },
                { "type", "event" }
            };

            // Budowanie URL z zapytaniem
            string url = QueryHelpers.AddQueryString(baseUrl, query);

            // Wysyłanie zapytania HTTP
            HttpResponseMessage response = await _httpClient.GetAsync(url);

            if (!response.IsSuccessStatusCode)
            {
                return StatusCode((int)response.StatusCode, "Błąd pobierania danych z Ticketmaster API");
            }

            string json = await response.Content.ReadAsStringAsync();
            var jsonObject = System.Text.Json.JsonSerializer.Deserialize<object>(json);

            return Ok(jsonObject);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { error = $"Błąd serwera: {ex.Message}" });
        }
    }

    [HttpGet]
    public IActionResult SaveJsonToDb()
    {
        // Zwrócenie widoku SaveJsonToDb.cshtml
        return View();
    }

    [HttpPost]
    public async Task<IActionResult> SaveEventJsonToDb()
    {
        try
        {
            string apiKey = Environment.GetEnvironmentVariable("API_KEY");
            string baseUrl = "https://app.ticketmaster.com/discovery/v2/events.json";
            var query = new Dictionary<string, string>
            {
                { "apikey", apiKey },
                { "size", "1" },
                { "city", "Krakow" },
                { "unit", "km" },
                { "radius", "1" },
                { "type", "event" }
            };

            string url = QueryHelpers.AddQueryString(baseUrl, query);
            HttpResponseMessage response = await _httpClient.GetAsync(url);

            if (!response.IsSuccessStatusCode)
            {
                return StatusCode((int)response.StatusCode, "Błąd pobierania danych z Ticketmaster API");
            }

            string json = await response.Content.ReadAsStringAsync();
            var ticketmasterData = System.Text.Json.JsonSerializer.Deserialize<TicketmasterResponse>(json);

            var firstEvent = ticketmasterData.Embedded?.Events?.FirstOrDefault();
            if (firstEvent == null)
                return BadRequest("Nie znaleziono eventu.");

            var entity = new Event
            {
                EventId = firstEvent.Id,
                TypeOfEvent = firstEvent.Type,
                NameOfEvent = firstEvent.Name,
                Url = firstEvent.Url,
                PhotoUrl = firstEvent.Images?.FirstOrDefault()?.Url,
                SalesStartDate = DateTime.Parse(firstEvent.Sales?.Public?.StartDateTime ?? DateTime.MinValue.ToString()),
                SalesEndDate = DateTime.Parse(firstEvent.Sales?.Public?.EndDateTime ?? DateTime.MinValue.ToString()),
                StartOfEvent = DateTime.Parse(firstEvent.Dates?.Start?.DateTime ?? DateTime.MinValue.ToString()),
                EndOfEvent = DateTime.Parse(firstEvent.Dates?.End?.DateTime ?? DateTime.MinValue.ToString()),
                Address = firstEvent.Embedded?.Venues?.FirstOrDefault()?.Address?.Line1,
                NameOfClub = firstEvent.Embedded?.Venues?.FirstOrDefault()?.Name,
                Classifications = firstEvent.Type, // Możesz podmienić, jeśli masz inne źródło
                UserId = 1 // ← wstaw ID zalogowanego użytkownika
            };


            _context.Events.Add(entity);
            await _context.SaveChangesAsync();

            return Ok("Zapisano do bazy");
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { error = $"Błąd serwera: {ex.Message}" });
        }
    }

}
