using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Thrift.Models;

namespace Thrift.ViewModels
{    public class SavingsIndexViewModel
    {
        public List<SavingsGoal> SavingsGoals { get; set; } = new();
        public SavingsSummaryViewModel Summary { get; set; } = new();
    }
    
    public class SavingsSummaryViewModel
    {
        public decimal TotalSaved { get; set; }
        public int TotalGoals { get; set; }
        public int ActiveGoals { get; set; }
        public int CompletedGoals { get; set; }
        public SavingsGoal? NextGoalToComplete { get; set; }
    }
    
    public class AddFundsViewModel
    {
        public required string GoalId { get; set; }
        public required string GoalName { get; set; }
        public decimal CurrentAmount { get; set; }
        public decimal TargetAmount { get; set; }
        public double ProgressPercentage { get; set; }
        
        [Required(ErrorMessage = "Amount is required")]
        [Range(0.01, double.MaxValue, ErrorMessage = "Amount must be greater than 0")]
        [DataType(DataType.Currency)]
        [Display(Name = "Amount to Add")]
        public decimal Amount { get; set; }
        
        [Display(Name = "Note (Optional)")]
        [StringLength(500, ErrorMessage = "Note cannot exceed 500 characters")]
        public string? Note { get; set; }
    }
}