using System.ComponentModel.DataAnnotations;

namespace WebApplication1.Models.Identity
{
    public class EditUserViewModel
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; }

        [Phone]
        [Display(Name = "Numer telefonu")]
        public string PhoneNumber { get; set; }
    }
}
