using Microsoft.EntityFrameworkCore;
using WebApplication1.Models;
using DotNetEnv;

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

var app = builder.Build();

// Konfiguracja potoku HTTP
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();

// Routing domyœlny
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
