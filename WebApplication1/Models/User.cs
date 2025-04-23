using Microsoft.Extensions.Logging;

namespace WebApplication1.Models
{
    public class User
    {
        public int Id { get; set; }  // Unikalny identyfikator użytkownika
        public string Name { get; set; }  // Nazwa użytkownika
        public string Email { get; set; }  // Email użytkownika
        public string Password { get; set; }  // Email użytkownika
        public ICollection<UserEvent> UserEvents { get; set; }
    }
}
