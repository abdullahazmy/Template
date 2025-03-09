using System.ComponentModel.DataAnnotations;

namespace Template.ViewModels
{
    public class RegisterViewModel
    {
        public string FirstName { get; set; }

        public string LastName { get; set; }


        [Required]
        [EmailAddress]
        [Display(Name = "Email")]
        public string Email { get; set; }

        //[Required]
        //[Display(Name = "Username")]
        //public string UserName { get; set; }

        [Required]
        [MinLength(6)]
        public string Password { get; set; }


        [Phone]
        [Display(Name = "Phone Number")]
        public string PhoneNumber { get; set; }

        [Display(Name = "Profile Picture")]

        public IFormFile? ProfilePicture { get; set; }

    }
}