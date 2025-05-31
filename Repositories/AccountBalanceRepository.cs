using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Thrift.Data;
using Thrift.Models;

namespace Thrift.Repositories
{
    public class AccountBalanceRepository
    {
        private readonly IMongoCollection<AccountBalance> _collection;
        private readonly LoanRepository _loanRepository;
        private readonly SavingsGoalRepository _savingsGoalRepository;
        private readonly BudgetRepository _budgetRepository;
        private readonly LoanPaymentRepository _loanPaymentRepository;
        private readonly SavingsTransactionRepository _savingsTransactionRepository;
        
        public AccountBalanceRepository(
            MongoDbContext context,
            LoanRepository loanRepository,
            SavingsGoalRepository savingsGoalRepository,
            BudgetRepository budgetRepository,
            LoanPaymentRepository loanPaymentRepository,
            SavingsTransactionRepository savingsTransactionRepository)
        {
            _collection = context.GetCollection<AccountBalance>("AccountBalances");
            _loanRepository = loanRepository;
            _savingsGoalRepository = savingsGoalRepository;
            _budgetRepository = budgetRepository;
            _loanPaymentRepository = loanPaymentRepository;
            _savingsTransactionRepository = savingsTransactionRepository;
        }
        
        public async Task<AccountBalance> GetByUserIdAsync(string userId)
        {
            var balance = await _collection.Find(x => x.UserId == userId).FirstOrDefaultAsync();
            
            if (balance == null)
            {
                balance = new AccountBalance { UserId = userId };
                await _collection.InsertOneAsync(balance);
            }
            
            return balance;
        }
        
        public async Task UpdateAsync(AccountBalance balance)
        {
            balance.LastUpdated = DateTime.UtcNow;
            await _collection.ReplaceOneAsync(x => x.Id == balance.Id, balance);
        }
        
        public async Task<AccountBalance> CalculateAccountBalanceAsync(string userId)
        {
            // Get the current balance or create a new one
            var balance = await GetByUserIdAsync(userId);
            
            // Get loans
            var loans = await _loanRepository.GetAllAsync(userId);
            var activeLoans = loans.Where(l => l.Status == LoanStatus.Active).ToList();
            
            // Get savings goals
            var savingsGoals = await _savingsGoalRepository.GetAllAsync(userId);
            var activeSavingsGoals = savingsGoals.Where(sg => sg.Status == "Active").ToList();
            
            // Get budgets
            var budgets = await _budgetRepository.GetAllAsync(userId);
            var currentBudget = budgets.OrderByDescending(b => b.CreatedAt).FirstOrDefault();
            
            // Get upcoming payments
            var upcomingPayments = await _loanPaymentRepository.GetUpcomingPaymentsAsync(userId, 30);
            
            // Calculate loan balances
            balance.LoanBalance = activeLoans.Sum(l => l.RemainingBalance);
            balance.MonthlyLoanPayments = activeLoans.Sum(l => l.MonthlyPayment);
            balance.TotalLiabilities = balance.LoanBalance;
            
            // Calculate savings balances
            balance.SavingsBalance = activeSavingsGoals.Sum(sg => sg.CurrentAmount);
            balance.SavingsGoalTarget = activeSavingsGoals.Sum(sg => sg.TargetAmount);
            balance.TotalAssets = balance.SavingsBalance;
            
            // Calculate budget figures
            if (currentBudget != null)
            {
                balance.CurrentBudgetIncome = currentBudget.TotalIncome + currentBudget.AdditionalIncome;
                balance.CurrentBudgetExpenses = currentBudget.TotalExpenses;
            }
            
            // Calculate upcoming payments
            balance.UpcomingLoanPayments = upcomingPayments.Sum(p => p.PaymentAmount);
            balance.NextLoanPaymentDate = upcomingPayments.OrderBy(p => p.DueDate).FirstOrDefault()?.DueDate;
            
            // Update the balance
            await UpdateAsync(balance);
            
            return balance;
        }
        
        public async Task<AccountBalanceViewModel> GetAccountBalanceViewModelAsync(string userId)
        {
            // Calculate the latest account balance
            var balance = await CalculateAccountBalanceAsync(userId);
            
            // Get additional data for the view model
            var activeLoans = await _loanRepository.GetActiveLoansByUserAsync(userId);
            var topSavingsGoals = await _savingsGoalRepository.GetActiveGoalsAsync(userId);
            var currentBudget = (await _budgetRepository.GetAllAsync(userId))
                .OrderByDescending(b => b.CreatedAt)
                .FirstOrDefault();
            var upcomingPayments = await _loanPaymentRepository.GetUpcomingPaymentsAsync(userId, 30);
            var recentTransactions = await _savingsTransactionRepository.GetRecentTransactionsAsync(userId, 5);
            
            return new AccountBalanceViewModel
            {
                Balance = balance,
                ActiveLoans = activeLoans,
                TopSavingsGoals = topSavingsGoals,
                CurrentBudget = currentBudget,
                UpcomingPayments = upcomingPayments,
                RecentTransactions = recentTransactions
            };
        }
    }
}
