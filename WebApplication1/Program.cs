using Microsoft.EntityFrameworkCore;
using WebApplication1.Models;
using DotNetEnv;
using Stripe;
using WebApplication1.ProjectSERVICES;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.AspNetCore.Identity;
using WebApplication1.Areas.Identity.Data;
using QuestPDF.Infrastructure;
using WebApplication1.Models.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Authentication;
using System.Text.Json;

var builder = WebApplication.CreateBuilder(args);
QuestPDF.Settings.License = LicenseType.Community;

// Wczytaj zmienne środowiskowe
DotNetEnv.Env.Load();

// Dodaj usługi MVC
builder.Services.AddControllersWithViews();
builder.Services.AddRazorPages();

// Konfiguracja bazy danych
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Konfiguracja Identity
builder.Services.AddIdentity<ApplicationUser, IdentityRole>()
    .AddEntityFrameworkStores<ApplicationDbContext>()
    .AddDefaultTokenProviders();

builder.Services.Configure<IdentityOptions>(options =>
{
    options.Password.RequireDigit = true;
    options.Password.RequireLowercase = true;
    options.Password.RequiredLength = 1;

    options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(1);
    options.Lockout.MaxFailedAccessAttempts = 10;
    options.Lockout.AllowedForNewUsers = true;

    options.User.AllowedUserNameCharacters = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789-._@+";
    options.User.RequireUniqueEmail = false;
});

// Konfiguracja ciasteczek autoryzacji
builder.Services.ConfigureApplicationCookie(options =>
{
    options.Cookie.HttpOnly = true;
    options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
    options.Cookie.SameSite = SameSiteMode.Lax;
    options.ExpireTimeSpan = TimeSpan.FromMinutes(30);
    options.SlidingExpiration = true;
    options.Cookie.MaxAge = options.ExpireTimeSpan;

    options.LoginPath = "/Identity/Account/Login";
    options.AccessDeniedPath = "/Identity/Account/AccessDenied";
    options.LogoutPath = "/Identity/Account/Logout";

    options.Events.OnValidatePrincipal = async context =>
    {
        var expiresAt = context.Properties.GetTokenValue("expires_at");
        if (DateTime.TryParse(expiresAt, null, System.Globalization.DateTimeStyles.RoundtripKind, out var expiresUtc))
        {
            if (expiresUtc < DateTime.UtcNow)
            {
                var refreshToken = context.Properties.GetTokenValue("refresh_token");
                var clientId = Environment.GetEnvironmentVariable("GOOGLE_CLIENT_ID");
                var clientSecret = Environment.GetEnvironmentVariable("GOOGLE_CLIENT_SECRET");

                var tokenRequest = new Dictionary<string, string>
                {
                    { "client_id", clientId },
                    { "client_secret", clientSecret },
                    { "refresh_token", refreshToken },
                    { "grant_type", "refresh_token" }
                };

                using var client = new HttpClient();
                var response = await client.PostAsync("https://oauth2.googleapis.com/token", new FormUrlEncodedContent(tokenRequest));
                var content = await response.Content.ReadAsStringAsync();

                var tokenResponse = JsonDocument.Parse(content);
                var newAccessToken = tokenResponse.RootElement.GetProperty("access_token").GetString();
                var newRefreshToken = tokenResponse.RootElement.TryGetProperty("refresh_token", out var rToken)
                    ? rToken.GetString()
                    : refreshToken;
                var newExpiresIn = tokenResponse.RootElement.GetProperty("expires_in").GetInt32();
                var newExpiresAt = DateTime.UtcNow.AddSeconds(newExpiresIn);

                context.Properties.UpdateTokenValue("access_token", newAccessToken);
                context.Properties.UpdateTokenValue("refresh_token", newRefreshToken);
                context.Properties.UpdateTokenValue("expires_at", newExpiresAt.ToString("o"));

                await context.HttpContext.SignInAsync(
                    CookieAuthenticationDefaults.AuthenticationScheme,
                    context.Principal,
                    context.Properties
                );
            }
        }
    };
});

// Konfiguracja OAuth Google
var googleClientId = Environment.GetEnvironmentVariable("GOOGLE_CLIENT_ID") ?? throw new InvalidOperationException("GOOGLE_CLIENT_ID is not set");
var googleClientSecret = Environment.GetEnvironmentVariable("GOOGLE_CLIENT_SECRET") ?? throw new InvalidOperationException("GOOGLE_CLIENT_SECRET is not set");

builder.Services.AddAuthentication(options =>
{
    options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = GoogleDefaults.AuthenticationScheme;
})
.AddCookie()
.AddGoogle(options =>
{
    options.ClientId = googleClientId;
    options.ClientSecret = googleClientSecret;
    options.CallbackPath = "/signin-google";
    options.AuthorizationEndpoint += "?access_type=offline&prompt=consent";
    options.SaveTokens = true;
});

// Dodatkowe usługi
builder.Services.AddHttpClient();
builder.Services.AddHttpClient<TokenService>();
builder.Services.AddTransient<IEmailSender, WebApplication1.ExtraTools.NullEmailSender>();
builder.Services.AddScoped<QrService>();
builder.Services.AddScoped<SmsService>();
builder.Services.AddScoped<EmailService>();
builder.Services.AddAuthorization();

// Tworzenie aplikacji
var app = builder.Build();

// Middleware bezpieczeństwa HTTP
app.Use(async (context, next) =>
{
    context.Response.Headers["Cache-Control"] = "no-store, no-cache, must-revalidate";
    context.Response.Headers["Pragma"] = "no-cache";
    context.Response.Headers["Expires"] = "-1";
    context.Response.Headers["X-Content-Type-Options"] = "nosniff";

    if (!context.Response.Headers.ContainsKey("Content-Security-Policy"))
        context.Response.Headers["Content-Security-Policy"] = "default-src 'self';";

    if (!context.Response.Headers.ContainsKey("X-XSS-Protection"))
        context.Response.Headers["X-XSS-Protection"] = "1; mode=block";

    if (!context.Response.Headers.ContainsKey("X-Frame-Options"))
        context.Response.Headers["X-Frame-Options"] = "DENY";

    if (context.Response.Headers.ContainsKey("P3P"))
        context.Response.Headers.Remove("P3P");

    await next();
});

// Obsługa błędów
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.MapRazorPages();
app.Run();
