using Microsoft.EntityFrameworkCore;
using WebApplication1.Models;
using DotNetEnv;
using Stripe;
using WebApplication1.ProjectSERVICES;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.Google;

var builder = WebApplication.CreateBuilder(args);

// Wczytaj zmienne œrodowiskowe z pliku .env (opcjonalnie, jeœli u¿ywasz DotNetEnv)
DotNetEnv.Env.Load(); // <- odkomentuj jeœli masz .env

// Rejestracja kontrolerów z widokami
builder.Services.AddControllersWithViews();

// Rejestracja DbContext z konfiguracj¹ po³¹czenia
builder.Services.AddDbContext<event_base>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Rejestracja HttpClient jako us³ugi DI
builder.Services.AddHttpClient();

builder.Services.AddScoped<QrService>();
builder.Services.AddScoped<SmsService>();
builder.Services.AddScoped<EmailService>();

builder.Services.AddAuthorization();




//wszystko do oauth



string googleClientId = Environment.GetEnvironmentVariable("GOOGLE_CLIENT_ID");

string googleClientSecret = Environment.GetEnvironmentVariable("GOOGLE_CLIENT_SECRET");

string googleRedirectUri = Environment.GetEnvironmentVariable("GOOGLE_REDIRECT_URI");

//builder.WebHost.UseUrls("https://localhost:7022");

builder.Services.AddHttpClient<TokenService>();

builder.Services.ConfigureApplicationCookie(options =>
{
    options.Cookie.HttpOnly = true;
    options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
    options.Cookie.SameSite = SameSiteMode.Strict;
    options.ExpireTimeSpan = TimeSpan.FromMinutes(30);
    options.SlidingExpiration = true;
    options.Cookie.MaxAge = options.ExpireTimeSpan;
});

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
});

var app = builder.Build();

app.Use(async (context, next) =>
{
    // Set Cache-Control headers to disable caching for sensitive responses
    context.Response.Headers["Cache-Control"] = "no-store, no-cache, must-revalidate";  // Disable caching
    context.Response.Headers["Pragma"] = "no-cache";  // For older browsers
    context.Response.Headers["Expires"] = "-1";  // To prevent caching in older browsers

    // Add the X-Content-Type-Options header to prevent MIME type sniffing
    context.Response.Headers["X-Content-Type-Options"] = "nosniff";

    // Ensure these security headers are present, they help protect against various attacks
    if (!context.Response.Headers.ContainsKey("Content-Security-Policy"))
    {
        context.Response.Headers["Content-Security-Policy"] = "default-src 'self';"; // Basic CSP rule to restrict content loading to same-origin
    }

    if (!context.Response.Headers.ContainsKey("X-XSS-Protection"))
    {
        context.Response.Headers["X-XSS-Protection"] = "1; mode=block"; // Enable XSS filter
    }

    if (!context.Response.Headers.ContainsKey("X-Frame-Options"))
    {
        context.Response.Headers["X-Frame-Options"] = "DENY"; // Prevent clickjacking
    }

    // Check and remove the P3P header if it exists (deprecated)
    if (context.Response.Headers.ContainsKey("P3P"))
    {
        context.Response.Headers.Remove("P3P");
    }

    // Continue processing the next middleware
    await next.Invoke();
});

// Konfiguracja potoku HTTP
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseAuthentication();
app.UseAuthorization();
app.UseRouting();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
