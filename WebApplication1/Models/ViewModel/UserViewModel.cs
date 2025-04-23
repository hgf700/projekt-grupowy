using System.ComponentModel.DataAnnotations;

namespace WebApplication1.Models.ViewModel
{
    public class UserViewModel
    {
        public int? Id { get; set; }  // Nullable dla nowego użytkownika

        [Required(ErrorMessage = "Nazwa jest wymagana.")]
        public string Name { get; set; }

        [Required(ErrorMessage = "Email jest wymagany.")]
        public string Email { get; set; }

        [Required(ErrorMessage = "Hasło jest wymagane.")]
        public string Password { get; set; }
    }
}
