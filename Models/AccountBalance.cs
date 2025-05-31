using System;
using System.Collections.Generic;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace Thrift.Models
{
    public class AccountBalance
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; } = string.Empty;
          [BsonElement("UserId")]
        public string UserId { get; set; } = string.Empty;
        
        [BsonElement("TotalAssets")]
        public decimal TotalAssets { get; set; } = 0;
        
        [BsonElement("TotalLiabilities")]
        public decimal TotalLiabilities { get; set; } = 0;
        
        [BsonIgnore]
        public decimal NetWorth => TotalAssets - TotalLiabilities;
        
        // Loan related balances
        [BsonElement("LoanBalance")]
        public decimal LoanBalance { get; set; } = 0;
        
        [BsonElement("MonthlyLoanPayments")]
        public decimal MonthlyLoanPayments { get; set; } = 0;
        
        // Savings related balances
        [BsonElement("SavingsBalance")]
        public decimal SavingsBalance { get; set; } = 0;
        
        [BsonElement("SavingsGoalTarget")]
        public decimal SavingsGoalTarget { get; set; } = 0;
        
        // Budget related balances
        [BsonElement("CurrentBudgetIncome")]
        public decimal CurrentBudgetIncome { get; set; } = 0;
        
        [BsonElement("CurrentBudgetExpenses")]
        public decimal CurrentBudgetExpenses { get; set; } = 0;
        
        [BsonElement("CurrentBudgetRemaining")]
        public decimal CurrentBudgetRemaining => CurrentBudgetIncome - CurrentBudgetExpenses;
        
        // Upcoming payments
        [BsonElement("UpcomingLoanPayments")]
        public decimal UpcomingLoanPayments { get; set; } = 0;
        
        [BsonElement("NextLoanPaymentDate")]
        public DateTime? NextLoanPaymentDate { get; set; }
        
        // Summary metrics
        [BsonElement("DebtToIncomeRatio")]
        public decimal DebtToIncomeRatio => CurrentBudgetIncome > 0 ? 
            Math.Round((MonthlyLoanPayments / CurrentBudgetIncome) * 100, 2) : 0;
        
        [BsonElement("SavingsRate")]
        public decimal SavingsRate => CurrentBudgetIncome > 0 ? 
            Math.Round((SavingsBalance / CurrentBudgetIncome) * 100, 2) : 0;
        
        [BsonElement("LastUpdated")]
        public DateTime LastUpdated { get; set; } = DateTime.UtcNow;
    }    public class AccountBalanceViewModel
    {
        public AccountBalance Balance { get; set; } = new AccountBalance();
        
        public List<SavingsGoal> TopSavingsGoals { get; set; } = new List<SavingsGoal>();
        
        public List<Loan> ActiveLoans { get; set; } = new List<Loan>();
        
        public Budget? CurrentBudget { get; set; } = null;
        
        public List<LoanPayment> UpcomingPayments { get; set; } = new List<LoanPayment>();
        
        public List<SavingsTransaction> RecentTransactions { get; set; } = new List<SavingsTransaction>();
    }
}
