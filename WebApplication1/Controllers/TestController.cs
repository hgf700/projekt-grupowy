using Microsoft.AspNetCore.Mvc;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.WebUtilities;
using System.Text.Json;
using WebApplication1.Models;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using QRCoder;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using Microsoft.Identity.Client;
using Twilio;
using Twilio.Rest.Api.V2010.Account;
using Twilio.Types;

[ApiController]
[Route("test")]
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
    [HttpGet("test")]
    public async Task<IActionResult> GetEvents()
    {
        try
        {
            string apiKey = Environment.GetEnvironmentVariable("TICKETMASTER_API_KEY");
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

    [HttpGet("event")]
    public IActionResult SaveJsonToDb()
    {
        // Zwrócenie widoku SaveJsonToDb.cshtml
        return View();
    }

    [HttpPost("event")]
    public async Task<IActionResult> SaveEventJsonToDb()
    {
        try
        {
            string apiKey = Environment.GetEnvironmentVariable("TICKETMASTER_API_KEY");
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
                ExternalEventId = firstEvent.Id,
                TypeOfEvent = firstEvent.Type,
                NameOfEvent = firstEvent.Name,
                UrlOfEvent = firstEvent.Url,
                PhotoUrl = firstEvent.Images?.FirstOrDefault()?.Url,
                SalesStartDate = DateTime.Parse(firstEvent.Sales?.Public?.StartDateTime ?? DateTime.MinValue.ToString()),
                SalesEndDate = DateTime.Parse(firstEvent.Sales?.Public?.EndDateTime ?? DateTime.MinValue.ToString()),
                StartOfEvent = DateTime.Parse(firstEvent.Dates?.Start?.DateTime ?? DateTime.MinValue.ToString()),
                EndOfEvent = DateTime.Parse(firstEvent.Dates?.End?.DateTime ?? DateTime.MinValue.ToString()),
                Address = firstEvent.Embedded?.Venues?.FirstOrDefault()?.Address?.Line1,
                NameOfClub = firstEvent.Embedded?.Venues?.FirstOrDefault()?.Name,
                Classifications = firstEvent.Type // Możesz podmienić, jeśli masz inne źródło
            };

            _context.Events.Add(entity);
            await _context.SaveChangesAsync();

            string dir = Path.Combine(Directory.GetCurrentDirectory(), "Resources");
            Directory.SetCurrentDirectory(dir);
            string fullPath = Path.Combine(dir, $"QR.PNG");

            QRCodeGenerator qrGenerator = new QRCodeGenerator();
            QRCodeData qrCodeData = qrGenerator.CreateQrCode(firstEvent.Url, QRCodeGenerator.ECCLevel.Q);

            Bitmap icon = (Bitmap)System.Drawing.Image.FromFile("logo.PNG");

            QRCode qrCode = new QRCode(qrCodeData);
            Bitmap qrCodeImage = qrCode.GetGraphic(
                pixelsPerModule: 5,
                darkColor: Color.FromArgb(0, 0, 255),
                lightColor: Color.FromArgb(255, 0, 0),
                icon: icon,
                iconSizePercent: 20,
                iconBorderWidth: 20,
                drawQuietZones: true                
            );

            qrCodeImage.Save(fullPath, ImageFormat.Png);

            bool.TryParse(Environment.GetEnvironmentVariable("TWILIO_SMS_SEND_STATE"), out bool twilio_sms_state);
            if (twilio_sms_state == true)
            {
                string twilio_sid = Environment.GetEnvironmentVariable("TWILIO_SID");
                string twilio_token = Environment.GetEnvironmentVariable("TWILIO_TOKEN");
                string sms_receiver = Environment.GetEnvironmentVariable("SMS_RECEIVER");
                string twilio_number = Environment.GetEnvironmentVariable("TWILIO_NUMBER");

                TwilioClient.Init(twilio_sid, twilio_token);

                var message = MessageResource.Create(
                    body: "To jest testowa wiadomość SMS z Twilio",
                    from: new Twilio.Types.PhoneNumber($"{twilio_number}"), 
                    to: new Twilio.Types.PhoneNumber($"{sms_receiver}")   
                );

                Console.WriteLine(message.Body);

            }

            return Ok("wyslano");

        }
        catch (Exception ex)
        {
            return StatusCode(500, new { error = $"Błąd serwera: {ex.Message}" });
        }
    }
}
