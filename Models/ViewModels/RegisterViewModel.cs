using System;
using System.ComponentModel.DataAnnotations;

namespace Thrift.Models.ViewModels
{
    public class RegisterViewModel
    {
        [Required]
        [EmailAddress]
        [Display(Name = "Email")]
        public string Email { get; set; }

        [Required]
        [StringLength(100, ErrorMessage = "The {0} must be at least {2} and at max {1} characters long.", MinimumLength = 6)]
        [DataType(DataType.Password)]
        [Display(Name = "Password")]
        public string Password { get; set; }

        [DataType(DataType.Password)]
        [Display(Name = "Confirm password")]
        [Compare("Password", ErrorMessage = "The password and confirmation password do not match.")]
        public string ConfirmPassword { get; set; }
        
        // Additional profile fields
        [Required]
        [StringLength(100)]
        [Display(Name = "First Name")]
        public string FirstName { get; set; }
        
        [Required]
        [StringLength(100)]
        [Display(Name = "Last Name")]
        public string LastName { get; set; }
        
        [StringLength(20)]
        [Display(Name = "Phone Number")]
        [DataType(DataType.PhoneNumber)]
        public string PhoneNumber { get; set; }
        
        [StringLength(200)]
        [Display(Name = "Address")]
        public string Address { get; set; }
        
        [StringLength(100)]
        [Display(Name = "City")]
        public string City { get; set; }
        
        [StringLength(100)]
        [Display(Name = "State/Province")]
        public string State { get; set; }
        
        [StringLength(20)]
        [Display(Name = "Zip/Postal Code")]
        public string ZipCode { get; set; }
        
        [StringLength(100)]
        [Display(Name = "Country")]
        public string Country { get; set; }
        
        [Display(Name = "Date of Birth")]
        [DataType(DataType.Date)]
        public DateTime? DateOfBirth { get; set; }

        [Required]
        [Display(Name = "Username")]
        [StringLength(100, ErrorMessage = "The {0} must be at least {2} and at max {1} characters long.", MinimumLength = 3)]
        public string Username { get; set; }
    }
}