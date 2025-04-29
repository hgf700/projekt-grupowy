using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using WebApplication1.Models;
using WebApplication1.Models.ViewModel;
using System.Linq;

namespace WebApplication1.Controllers
{
    [ApiController]
    [Route("user")]
    public class UserController : Controller
    {
        private readonly event_base _context;

        public UserController(event_base context)
        {
            _context = context;
        }

        // Widok listy użytkowników
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var users = await _context.Users  // Używamy DbSet<User> z kontekstu
                                          .OrderBy(u => u.Name)
                                          .ToListAsync();

            return View(users);
        }

        // Widok tworzenia użytkownika
        [HttpGet("create")]
        public IActionResult CreateUser()
        {
            return View(new UserViewModel());
        }

        [HttpPost("create")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateUser([FromForm] UserViewModel model)
        {
            if (ModelState.IsValid)
            {
                var user = new User
                {
                    Name = model.Name,
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

            var model = new UserViewModel
            {
                Id = user.Id,
                Name = user.Name,
                Email = user.Email,
                Password = user.Password
            };

            return View(model);
        }

        // Metoda edycji użytkownika
        [HttpPost("edit/{id}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditUser(int id, UserViewModel model)
        {
            if (id != model.Id)
                return BadRequest();

            if (ModelState.IsValid)
            {
                var user = await _context.Users.FindAsync(id);  // Znajdujemy użytkownika po ID
                if (user == null)
                    return NotFound();

                user.Name = model.Name;
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
