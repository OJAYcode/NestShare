using System;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;

namespace Thrift.Models
{
    public class UserProfile
    {
        public int Id { get; set; }
        
        [Required]
        public string UserId { get; set; }
        
        [Required]
        [StringLength(100)]
        public string FirstName { get; set; }
        
        [Required]
        [StringLength(100)]
        public string LastName { get; set; }
        
        [StringLength(20)]
        public string PhoneNumber { get; set; }
        
        [StringLength(200)]
        public string Address { get; set; }
        
        [StringLength(100)]
        public string City { get; set; }
        
        [StringLength(100)]
        public string State { get; set; }
        
        public DateTime? DateOfBirth { get; set; }
        
        public DateTime CreatedAt { get; set; }
          public DateTime UpdatedAt { get; set; }
        
        // Navigation property for the user
        public virtual User User { get; set; }
    }
}