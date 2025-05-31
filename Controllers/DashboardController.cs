using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Thrift.Models;
using Thrift.Repositories;
using Thrift.ViewModels;

namespace Thrift.Controllers
{
    [Authorize]
    public class DashboardController : Controller
    {
        private readonly UserManager<User> _userManager;
        private readonly SavingsGoalRepository _savingsGoalRepository;
        private readonly SavingsTransactionRepository _savingsTransactionRepository;
        private readonly BudgetRepository _budgetRepository;
        private readonly ExpenseRepository _expenseRepository;

        public DashboardController(
            UserManager<User> userManager,
            SavingsGoalRepository savingsGoalRepository,
            SavingsTransactionRepository savingsTransactionRepository,
            BudgetRepository budgetRepository,
            ExpenseRepository expenseRepository)
        {
            _userManager = userManager;
            _savingsGoalRepository = savingsGoalRepository;
            _savingsTransactionRepository = savingsTransactionRepository;
            _budgetRepository = budgetRepository;
            _expenseRepository = expenseRepository;
        }        public async Task<IActionResult> Index()
        {
            var userId = _userManager.GetUserId(User);
            
            // Handle case where user ID is null (should not happen with [Authorize] attribute)
            if (string.IsNullOrEmpty(userId))
            {
                return RedirectToAction("Login", "Account");
            }
            
            // Get savings goals
            var savingsGoals = await _savingsGoalRepository.GetAllAsync(userId);
            
            // Get all transactions
            var savingsTransactions = await _savingsTransactionRepository.GetAllForUserAsync(userId);
            
            // Get recent budgets
            var budgets = await _budgetRepository.GetAllAsync(userId);
            var currentBudget = budgets.OrderByDescending(b => b.CreatedAt).FirstOrDefault();
            
            // Get recent expenses
            var expenses = await _expenseRepository.GetRecentExpensesAsync(userId, 5);
              // Calculate statistics
            decimal totalSavings = savingsGoals.Sum(g => g.CurrentAmount);
            decimal savingsTarget = savingsGoals.Sum(g => g.TargetAmount);
            decimal savingsProgress = savingsTarget > 0 ? (totalSavings / savingsTarget) * 100 : 0;
            
            // Handle case where there's no budget
            decimal thisMonthIncome = currentBudget?.TotalIncome ?? 0;
            decimal thisMonthExpenses = currentBudget?.TotalExpenses ?? 0;
            
            // Ensure we have at least empty collections for the charts if no data exists
            if (!savingsTransactions.Any())
            {
                savingsTransactions = new List<SavingsTransaction>();
            }
            
            if (savingsGoals == null)
            {
                savingsGoals = new List<SavingsGoal>();
            }
              // Monthly savings data for chart
            var monthlySavingsData = GetMonthlySavingsData(savingsTransactions);
              // Expense categories data for chart
            var expenseCategories = GetExpenseCategoriesFromBudget(currentBudget);
            
            var viewModel = new DashboardViewModel
            {
                TotalSavings = totalSavings,
                SavingsTarget = savingsTarget,
                SavingsProgress = savingsProgress,
                ThisMonthIncome = thisMonthIncome,
                ThisMonthExpenses = thisMonthExpenses,
                ActiveSavingsGoals = savingsGoals.Count(g => g.Status == "Active"),
                CompletedSavingsGoals = savingsGoals.Count(g => g.Status == "Completed"),
                SavingsGoals = savingsGoals.OrderByDescending(g => g.Priority == "High").Take(3).ToList(),
                RecentTransactions = savingsTransactions.OrderByDescending(t => t.TransactionDate).Take(5).ToList(),
                MonthlySavingsData = monthlySavingsData,
                ExpenseCategoriesData = expenseCategories,
                RecentExpenses = expenses
            };
            
            return View(viewModel);
        }
        
        /// <summary>
        /// Helper method to extract expense categories from a budget
        /// </summary>
        private Dictionary<string, decimal> GetExpenseCategoriesFromBudget(Budget budget)
        {
            if (budget == null)
            {
                return new Dictionary<string, decimal>();
            }
            
            var categories = new Dictionary<string, decimal>
            {
                { "Housing", budget.HousingExpenses },
                { "Utilities", budget.UtilitiesExpenses },
                { "Transportation", budget.TransportationExpenses },
                { "Food", budget.FoodExpenses },
                { "Healthcare", budget.HealthcareExpenses },
                { "Entertainment", budget.EntertainmentExpenses },
                { "Personal", budget.PersonalExpenses },
                { "Savings", budget.SavingsAllocation },
                { "Debt", budget.DebtPayment },
                { "Other", budget.OtherExpenses }
            };
            
            // Only return categories with values > 0
            return categories.Where(e => e.Value > 0).ToDictionary(k => k.Key, v => v.Value);
        }
        
        /// <summary>
        /// Helper method to calculate monthly savings data for the chart
        /// </summary>
        private List<dynamic> GetMonthlySavingsData(List<SavingsTransaction> transactions)
        {
            if (transactions == null || !transactions.Any())
            {
                return new List<dynamic>();
            }
            
            var monthlySavings = transactions
                .Where(t => t.Type == "Deposit" && t.TransactionDate >= DateTime.Now.AddMonths(-6))
                .GroupBy(t => new { Month = t.TransactionDate.Month, Year = t.TransactionDate.Year })
                .Select(g => new { 
                    Month = new DateTime(g.Key.Year, g.Key.Month, 1).ToString("MMM yyyy"),
                    Amount = g.Sum(t => t.Amount)
                })
                .OrderBy(x => new DateTime(int.Parse(x.Month.Split(' ')[1]), DateTime.ParseExact(x.Month.Split(' ')[0], "MMM", null).Month, 1))
                .ToList();

            // Convert to List<dynamic> to match the ViewModel property type
            List<dynamic> result = new List<dynamic>();
            foreach (var item in monthlySavings)
            {
                dynamic dynamicItem = new System.Dynamic.ExpandoObject();
                ((IDictionary<string, object>)dynamicItem).Add("Month", item.Month);
                ((IDictionary<string, object>)dynamicItem).Add("Amount", item.Amount);
                result.Add(dynamicItem);
            }
            
            return result;
        }
    }
}