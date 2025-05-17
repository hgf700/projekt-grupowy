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

[ApiController]
[Route("test")]
public class TestController : Controller
{
    private readonly event_base _context;
    private readonly HttpClient _httpClient;
    private readonly QrService _qrService;
    private readonly SmsService _smsservice;

    public TestController(HttpClient httpClient, event_base context, QrService qrService, SmsService smsservice)
    {
        _httpClient = httpClient;
        _context=context;
        _qrService=qrService;
        _smsservice = smsservice;
    }

  

    [HttpPost("payment")]
    public IActionResult CreateCheckoutSession()
    {
        StripeConfiguration.ApiKey = Environment.GetEnvironmentVariable("STRIP_SEC_KEY");

        string YOUR_DOMAIN = "https://localhost:7022";

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
