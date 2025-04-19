using Microsoft.Extensions.Logging;

namespace WebApplication1.Models
{
    public class User
    {
        public int Id { get; set; }  // Unikalny identyfikator użytkownika
        public string Name { get; set; }  // Nazwa użytkownika
        public string Email { get; set; }  // Email użytkownika

        // Relacja 1 do wielu z Event
        public ICollection<Event> Events { get; set; }
    }

}
