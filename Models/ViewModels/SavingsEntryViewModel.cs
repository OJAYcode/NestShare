using System;
using System.ComponentModel.DataAnnotations;

namespace Thrift.Models.ViewModels
{
    public class SavingsEntryViewModel
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Please enter an amount")]
        [Range(0.01, double.MaxValue, ErrorMessage = "Amount must be greater than zero")]
        public decimal Amount { get; set; }

        [Required(ErrorMessage = "Please select a category")]
        public string Category { get; set; }

        [StringLength(200, ErrorMessage = "Description cannot exceed 200 characters")]
        public string Description { get; set; }

        [Required(ErrorMessage = "Please select a date")]
        [DataType(DataType.Date)]
        public DateTime Date { get; set; }
    }
}