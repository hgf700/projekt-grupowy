using Microsoft.AspNetCore.Identity;

namespace WebApplication1.Models.Identity
{
    public class RegisterModel
    {
        public string Email { get; set; }
        public string Password { get; set; }
        public string ConfirmPassword { get; set; }
        public bool IsOAuth { get; set; } = false;
        public string? GoogleId { get; set; }
    }

}
