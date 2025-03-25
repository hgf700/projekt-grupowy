using Microsoft.EntityFrameworkCore;
using WebApplication1.Models;

namespace aspapp
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Dodanie us³ug do kontenera DI
            builder.Services.AddControllersWithViews();

            // Rejestracja bazy danych (DbContext)
            builder.Services.AddDbContext<event_base>(options =>
                options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

            // Rejestracja HttpClient w kontenerze DI
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

            app.MapControllerRoute(
                name: "default",
                pattern: "{controller=Home}/{action=Index}/{id?}");

            app.Run();
        }
    }
}
