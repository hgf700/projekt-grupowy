using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using WebApplication1.Models;
using WebApplication1.Models.ViewModel;
using System.Linq;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication.Google;
using WebApplication1.Areas.Identity.Data;


namespace WebApplication1.Controllers
{
    [ApiController]
    [Route("user")]
    public class UserController : Controller
    {
        private readonly ApplicationDbContext _context;

        public UserController(ApplicationDbContext context)
        {
            _context = context;
        }

        // Widok listy użytkowników
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var users = await _context.Users.ToListAsync();

            return View(users);
        }

        // Widok tworzenia użytkownika
        [HttpGet("create")]
        public IActionResult CreateUser()
        {
            return View(new User());
        }




        [HttpGet("oauth")]
        public IActionResult Login()
        {
            var redirectUrl = Url.Action("GoogleResponse", "User");
            var properties = new AuthenticationProperties
            {
                RedirectUri = redirectUrl,
                Items = { { "LoginProvider", "Google" } }
            };

            return Challenge(properties, GoogleDefaults.AuthenticationScheme);
        }

        [HttpGet("GoogleResponse")]
        public async Task<IActionResult> GoogleResponse()
        {
            var result = await HttpContext.AuthenticateAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            if (!result.Succeeded)
                return RedirectToAction("Index");

            var emailClaim = result.Principal.FindFirst(ClaimTypes.Email)?.Value;
            if (string.IsNullOrEmpty(emailClaim))
            {
                return RedirectToAction("LoginFailed");
            }

            var claims = result.Principal.Claims
               .Select(c => new { c.Type, c.Value });

            // Logowanie użytkownika w systemie
            await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, result.Principal);

            return View(claims);
        }

        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            //return Redirect("https://accounts.google.com/logout");

            return RedirectToAction("Index", "User");
        }


        [HttpPost("create")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateUser(User model)
        {
            if (ModelState.IsValid)
            {
                var user = new User
                {
                    Email = model.Email,
                    Password = model.Password
                };

                _context.Users.Add(user);
                await _context.SaveChangesAsync();

                return RedirectToAction(nameof(Index));
            }

            return View(model);
        }

        // Widok edycji użytkownika
        [HttpGet("edit/{id}")]
        public async Task<IActionResult> EditUser(int id)
        {
            var user = await _context.Users.FindAsync(id);  // Znajdujemy użytkownika po ID
            if (user == null)
                return NotFound();

            var model = new User
            {
                Id = user.Id,
                Email = user.Email,
                Password = user.Password
            };

            return View(model);
        }

        // Metoda edycji użytkownika
        [HttpPost("edit/{id}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditUser(int id, User model)
        {
            if (id != model.Id)
                return BadRequest();

            if (ModelState.IsValid)
            {
                var user = await _context.Users.FindAsync(id);  // Znajdujemy użytkownika po ID
                if (user == null)
                    return NotFound();

                user.Email = model.Email;
                user.Password = model.Password;

                _context.Users.Update(user);  // Aktualizujemy użytkownika w kontekście
                await _context.SaveChangesAsync();  // Zapisujemy zmiany do bazy danych

                return RedirectToAction(nameof(Index));
            }

            return View(model);
        }

        // Widok usuwania użytkownika
        [HttpGet("delete/{id}")]
        public async Task<IActionResult> DeleteUser(int id)
        {
            var user = await _context.Users.FindAsync(id);  // Znajdujemy użytkownika po ID
            if (user == null)
                return NotFound();

            return View(user);
        }

        // Metoda usuwania użytkownika
        [HttpPost("delete/{id}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var user = await _context.Users.FindAsync(id);  // Znajdujemy użytkownika po ID
            if (user == null)
                return NotFound();

            _context.Users.Remove(user);  // Usuwamy użytkownika z kontekstu
            await _context.SaveChangesAsync();  // Zapisujemy zmiany do bazy danych

            return RedirectToAction(nameof(Index));
        }
    }
}
