using Microsoft.EntityFrameworkCore;
using WebApplication1.Models;
using DotNetEnv;

var builder = WebApplication.CreateBuilder(args);

// Wczytaj zmienne �rodowiskowe z pliku .env (opcjonalnie, je�li u�ywasz DotNetEnv)
DotNetEnv.Env.Load(); // <- odkomentuj je�li masz .env

// Rejestracja kontroler�w z widokami
builder.Services.AddControllersWithViews();

// Rejestracja DbContext z konfiguracj� po��czenia
builder.Services.AddDbContext<event_base>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Rejestracja HttpClient jako us�ugi DI
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

// Routing domy�lny
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
