using System;
using System.ComponentModel.DataAnnotations;

namespace Thrift.Models.ViewModels
{    public class BudgetViewModel
    {
        public string Id { get; set; }
        
        [Required(ErrorMessage = "Budget name is required")]
        [StringLength(100, ErrorMessage = "Budget name cannot exceed 100 characters")]
        public string Name { get; set; }

        [Required(ErrorMessage = "Month is required")]
        public string Month { get; set; }

        [Range(0, double.MaxValue, ErrorMessage = "Total income must be a positive number")]
        public decimal TotalIncome { get; set; }

        [Range(0, double.MaxValue, ErrorMessage = "Additional income must be a positive number")]
        public decimal AdditionalIncome { get; set; }

        [Range(0, double.MaxValue, ErrorMessage = "Housing expenses must be a positive number")]
        public decimal HousingExpenses { get; set; }

        [Range(0, double.MaxValue, ErrorMessage = "Utilities expenses must be a positive number")]
        public decimal UtilitiesExpenses { get; set; }

        [Range(0, double.MaxValue, ErrorMessage = "Transportation expenses must be a positive number")]
        public decimal TransportationExpenses { get; set; }

        [Range(0, double.MaxValue, ErrorMessage = "Food expenses must be a positive number")]
        public decimal FoodExpenses { get; set; }

        [Range(0, double.MaxValue, ErrorMessage = "Healthcare expenses must be a positive number")]
        public decimal HealthcareExpenses { get; set; }

        [Range(0, double.MaxValue, ErrorMessage = "Entertainment expenses must be a positive number")]
        public decimal EntertainmentExpenses { get; set; }

        [Range(0, double.MaxValue, ErrorMessage = "Personal expenses must be a positive number")]
        public decimal PersonalExpenses { get; set; }

        [Range(0, double.MaxValue, ErrorMessage = "Savings allocation must be a positive number")]
        public decimal SavingsAllocation { get; set; }

        [Range(0, double.MaxValue, ErrorMessage = "Debt payment must be a positive number")]
        public decimal DebtPayment { get; set; }

        [Range(0, double.MaxValue, ErrorMessage = "Other expenses must be a positive number")]
        public decimal OtherExpenses { get; set; }

        public string Notes { get; set; }

        // Calculated properties
        public decimal TotalExpenses => 
            HousingExpenses + UtilitiesExpenses + TransportationExpenses + 
            FoodExpenses + HealthcareExpenses + EntertainmentExpenses + 
            PersonalExpenses + SavingsAllocation + DebtPayment + OtherExpenses;

        public decimal RemainingBudget => (TotalIncome + AdditionalIncome) - TotalExpenses;
    }
}