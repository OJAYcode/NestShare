using System.Collections.Generic;
using Thrift.Models;

namespace Thrift.ViewModels
{
    public class DashboardViewModel
    {
        public decimal TotalSavings { get; set; }
        public decimal SavingsTarget { get; set; }
        public decimal SavingsProgress { get; set; }
        public decimal ThisMonthIncome { get; set; }
        public decimal ThisMonthExpenses { get; set; }
        public int ActiveSavingsGoals { get; set; }
        public int CompletedSavingsGoals { get; set; }
        
        public List<SavingsGoal> SavingsGoals { get; set; }
        public List<SavingsTransaction> RecentTransactions { get; set; }
        public List<dynamic> MonthlySavingsData { get; set; }
        public Dictionary<string, decimal> ExpenseCategoriesData { get; set; }
        public List<Expense> RecentExpenses { get; set; }
    }
}