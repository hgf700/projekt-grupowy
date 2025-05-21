using Microsoft.EntityFrameworkCore;
using WebApplication1.Models;
using DotNetEnv;
using Stripe;
using WebApplication1.ProjectSERVICES;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.AspNetCore.Identity;
using WebApplication1.Areas.Identity.Data;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using QuestPDF.Infrastructure;
using WebApplication1.Models.Identity;

var builder = WebApplication.CreateBuilder(args);

QuestPDF.Settings.License = LicenseType.Community;

// Wczytaj zmienne �rodowiskowe z pliku .env (opcjonalnie, je�li u�ywasz DotNetEnv)
DotNetEnv.Env.Load(); // <- odkomentuj je�li masz .env

// Rejestracja kontroler�w z widokami
builder.Services.AddControllersWithViews();

// Rejestracja DbContext z konfiguracj� po��czenia
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddDefaultIdentity<IdentityUser>(options => options.SignIn.RequireConfirmedAccount = true).AddEntityFrameworkStores<ApplicationDbContext>();

builder.Services.AddIdentity<ApplicationUser, IdentityRole>()
    .AddEntityFrameworkStores<ApplicationDbContext>()
    .AddDefaultTokenProviders();


// Rejestracja HttpClient jako us�ugi DI
builder.Services.AddHttpClient();

builder.Services.AddScoped<QrService>();
builder.Services.AddScoped<SmsService>();
builder.Services.AddScoped<EmailService>();

builder.Services.AddAuthorization();




//identity  !!!!!!!!!!
builder.Services.Configure<IdentityOptions>(options =>
{
    // Password settings.
    options.Password.RequireDigit = true;
    options.Password.RequireLowercase = true;
    //options.Password.RequireUppercase = true;
    options.Password.RequiredLength = 1;

    // Lockout settings.
    options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(1);
    options.Lockout.MaxFailedAccessAttempts = 10;
    options.Lockout.AllowedForNewUsers = true;

    // User settings.
    options.User.AllowedUserNameCharacters =
    "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789-._@+";
    options.User.RequireUniqueEmail = false;


});

builder.Services.ConfigureApplicationCookie(options =>
{
    // Cookie settings
    options.Cookie.HttpOnly = true;
    options.ExpireTimeSpan = TimeSpan.FromMinutes(5);

    options.LoginPath = "/Identity/Account/Login";
    options.AccessDeniedPath = "/Identity/Account/AccessDenied";
    options.LogoutPath = "/Identity/Account/Logout";
    options.SlidingExpiration = true;
});







////wszystko do oauth !!!!!!!!!!

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

builder.Services.AddRazorPages(); // <--- dodaj to


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

app.MapRazorPages();

app.Run();
