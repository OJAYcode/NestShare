using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Thrift.Models;
using Thrift.Repositories;

namespace Thrift.Controllers
{
    [Authorize]
    public class AccountBalanceController : Controller
    {
        private readonly AccountBalanceRepository _accountBalanceRepository;
        
        public AccountBalanceController(AccountBalanceRepository accountBalanceRepository)
        {
            _accountBalanceRepository = accountBalanceRepository;
        }
          // GET: AccountBalance
        public async Task<IActionResult> Index()
        {
            try
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.Identity?.Name ?? "";
                
                if (string.IsNullOrEmpty(userId))
                {
                    TempData["ErrorMessage"] = "User not authenticated. Please log in again.";
                    return RedirectToAction("Login", "Account");
                }
                
                var viewModel = await _accountBalanceRepository.GetAccountBalanceViewModelAsync(userId);
                
                if (viewModel == null)
                {
                    TempData["ErrorMessage"] = "Unable to load account balance data.";
                    // Create empty view model to prevent null reference
                    viewModel = new AccountBalanceViewModel
                    {
                        Balance = new AccountBalance { UserId = userId },
                        ActiveLoans = new List<Loan>(),
                        TopSavingsGoals = new List<SavingsGoal>(),
                        CurrentBudget = null,
                        UpcomingPayments = new List<LoanPayment>(),
                        RecentTransactions = new List<SavingsTransaction>()
                    };
                }
                
                return View(viewModel);
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"An error occurred while loading account balance: {ex.Message}";
                // Create empty view model to prevent page crash
                var emptyViewModel = new AccountBalanceViewModel
                {
                    Balance = new AccountBalance { UserId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "" },
                    ActiveLoans = new List<Loan>(),
                    TopSavingsGoals = new List<SavingsGoal>(),
                    CurrentBudget = null,
                    UpcomingPayments = new List<LoanPayment>(),
                    RecentTransactions = new List<SavingsTransaction>()
                };
                return View(emptyViewModel);
            }
        }
        
        // GET: AccountBalance/Details
        public async Task<IActionResult> Details()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.Identity?.Name ?? "";
            var viewModel = await _accountBalanceRepository.GetAccountBalanceViewModelAsync(userId);
            
            return View(viewModel);
        }
        
        // GET: AccountBalance/Refresh
        public async Task<IActionResult> Refresh()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.Identity?.Name ?? "";
            await _accountBalanceRepository.CalculateAccountBalanceAsync(userId);
            
            TempData["SuccessMessage"] = "Account balance has been refreshed!";
            return RedirectToAction(nameof(Index));
        }
    }
}
