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
using Stripe.Checkout;
using Stripe;
using WebApplication1.ProjectSERVICES;
using WebApplication1.Areas.Identity.Data;

[ApiController]
[Route("test")]
public class TestController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly HttpClient _httpClient;
    private readonly QrService _qrService;
    private readonly SmsService _smsservice;
    string YOUR_DOMAIN = "https://localhost:7022";

    public TestController(HttpClient httpClient, ApplicationDbContext context, QrService qrService, SmsService smsservice)
    {
        _httpClient = httpClient;
        _context=context;
        _qrService=qrService;
        _smsservice = smsservice;
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

            var entity = new WebApplication1.Models.Event
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

            _qrService.GenerateQrCode(firstEvent.Url);

            bool.TryParse(Environment.GetEnvironmentVariable("TWILIO_SMS_SEND_STATE"), out bool twilio_sms_state);

            if (twilio_sms_state == true)
            {
                _smsservice.SendSMS(firstEvent.Url);
            }

            return Ok("wyslano");
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { error = $"Błąd serwera: {ex.Message}" });
        }
    }

    [HttpPost("payment")]
    public IActionResult CreateCheckoutSession()
    {
        StripeConfiguration.ApiKey = Environment.GetEnvironmentVariable("STRIP_SEC_KEY");

        var options = new SessionCreateOptions
        {


            LineItems = new List<SessionLineItemOptions>
            {
                new SessionLineItemOptions
                {
                    PriceData = new SessionLineItemPriceDataOptions
                    {
                        Currency = "pln",
                        UnitAmount = 1000, // 10.00 PLN
                        ProductData = new SessionLineItemPriceDataProductDataOptions
                        {
                            Name = "Bilet na wydarzenie",
                        },
                    },
                    Quantity = 1,
                },

            },

            Mode = "payment",
            SuccessUrl = $"{YOUR_DOMAIN}/Test/PaymentSuccess",  // Akcja przekierowania na sukces
            CancelUrl = $"{YOUR_DOMAIN}/Test/PaymentFailed",
        };

        var service = new SessionService();
        Session session = service.Create(options);

        return Redirect(session.Url);
    }

    [HttpGet("PaymentSuccess")]
    public IActionResult PaymentSuccess()
    {
        // Logika po udanej płatności
        ViewBag.Message = "Płatność zakończona sukcesem!";
        return View("Success"); // Zwróć widok "Success.cshtml"
    }

    [HttpGet("PaymentFailed")]
    public IActionResult PaymentFailed()
    {
        // Logika po nieudanej płatności
        ViewBag.Message = "Płatność nie powiodła się. Spróbuj ponownie.";
        return View("Failed"); // Zwróć widok "Failed.cshtml"
    }


}
