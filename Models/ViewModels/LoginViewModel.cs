using System.ComponentModel.DataAnnotations;

namespace Thrift.Models.ViewModels
{
    public class LoginViewModel
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; }

        [Required]
        [DataType(DataType.Password)]
        public string Password { get; set; }

        [Display(Name = "Remember me?")]
        public bool RememberMe { get; set; }
        
        // Add a message property to display registration success message
        public string Message { get; set; }
    }
}