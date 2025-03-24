using Microsoft.AspNetCore.Mvc;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.WebUtilities;
using System.Text.Json;
using WebApplication1.Models;
using Microsoft.EntityFrameworkCore.Metadata.Internal;

[Route("api/events")]
[ApiController]
public class EventsController : ControllerBase
{
    private readonly HttpClient _httpClient;

    public EventsController(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    //testowe narazie
    [HttpGet]
    public async Task<IActionResult> GetEvents()
    {
        try
        {
            string apiKey = "0XWFV4YA1DPusHBAIZGvcGzPgH8HUlza";  
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
            var jsonObject = JsonSerializer.Deserialize<object>(json);

            return Ok(jsonObject);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { error = $"Błąd serwera: {ex.Message}" });
        }
    }

}
