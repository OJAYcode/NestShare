using System.ComponentModel.DataAnnotations;

namespace Thrift.Models.ViewModels
{
    public class ProfileViewModel
    {
        [Display(Name = "Email")]
        public string Email { get; set; }
        
        [Display(Name = "Username")]
        public string UserName { get; set; }
        
        // Add additional properties as needed, such as:
        // public string FirstName { get; set; }
        // public string LastName { get; set; }
        // public DateTime JoinDate { get; set; }
    }
}