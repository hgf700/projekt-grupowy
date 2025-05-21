using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using WebApplication1.Areas.Identity.Data;

namespace WebApplication1.Models.Identity
{
    public class AccountService
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IUserStore<ApplicationUser> _userStore;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly ILogger<AccountService> _logger;
        private readonly IEmailSender _emailSender;

        public AccountService(
            UserManager<ApplicationUser> userManager,
            IUserStore<ApplicationUser> userStore,
            SignInManager<ApplicationUser> signInManager,
            ILogger<AccountService> logger,
            IEmailSender emailSender,
            ApplicationDbContext context)
        {
            _userManager = userManager;
            _userStore = userStore;
            _signInManager = signInManager;
            _logger = logger;
            _emailSender = emailSender;
            _context = context;
        }

        public async Task RegisterAsync(RegisterModel model)
        {
            if (model.Password != model.ConfirmPassword)
            {
                throw new Exception("Hasła się nie zgadzają.");
            }

            var user = new ApplicationUser
            {
                UserName = model.Email,
                Email = model.Email,
                IsOAuth = model.IsOAuth,
                GoogleId = model.GoogleId
            };

            var result = await _userManager.CreateAsync(user, model.Password);

            if (!result.Succeeded)
            {
                var errors = string.Join(", ", result.Errors.Select(e => e.Description));
                throw new Exception($"Błąd rejestracji: {errors}");
            }

            await _signInManager.SignInAsync(user, isPersistent: false);
            _logger.LogInformation("Użytkownik zarejestrowany pomyślnie.");
        }

    }

}
