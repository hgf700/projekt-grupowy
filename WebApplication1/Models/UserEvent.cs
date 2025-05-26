using WebApplication1.Models.Identity;

namespace WebApplication1.Models
{
    public class UserEvent
    {
        public string UserId { get; set; }
        public ApplicationUser User { get; set; }

        public int EventId { get; set; }
        public Event Event { get; set; }
    }

}
