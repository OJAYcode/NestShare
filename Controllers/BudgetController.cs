using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Thrift.Models;
using Thrift.Models.ViewModels;
using Thrift.Repositories;

namespace Thrift.Controllers
{
    [Authorize]
    public class BudgetController : Controller
    {
        private readonly BudgetRepository _budgetRepository;
        private readonly ExpenseRepository _expenseRepository;
        private readonly UserManager<User> _userManager;

        public BudgetController(
            BudgetRepository budgetRepository,
            ExpenseRepository expenseRepository,
            UserManager<User> userManager)
        {
            _budgetRepository = budgetRepository;
            _expenseRepository = expenseRepository;
            _userManager = userManager;
        }

        // GET: Budget
        public async Task<IActionResult> Index()
        {
            var userId = _userManager.GetUserId(User);
            var budgets = await _budgetRepository.GetAllAsync(userId);
            return View(budgets);
        }

        // GET: Budget/Details/5
        public async Task<IActionResult> Details(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return NotFound();
            }

            var userId = _userManager.GetUserId(User);
            var budget = await _budgetRepository.GetByIdAsync(id);

            if (budget == null || budget.UserId != userId)
            {
                return NotFound();
            }            // Load expenses for this budget
            var expenses = await _expenseRepository.GetAllForBudgetAsync(id);
            budget.Expenses = expenses;

            return View(budget);
        }

        // GET: Budget/Create
        public IActionResult Create()
        {
            ViewData["Categories"] = new List<string> {
                "Housing", "Transportation", "Food", "Utilities", "Insurance",
                "Healthcare", "Debt", "Entertainment", "Clothing", "Education",
                "Gifts", "Personal", "Savings", "Other"
            };
            
            ViewData["RecurringPeriods"] = new List<string> {
                "Monthly", "Weekly", "Bi-weekly", "Quarterly", "Yearly"
            };
            
            return View(new BudgetViewModel());
        }        // POST: Budget/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(BudgetViewModel viewModel)
        {
            try
            {
                // Remove any validation errors for the Id field during create since it should be empty
                if (ModelState.ContainsKey("Id"))
                {
                    ModelState.Remove("Id");
                }
                
                if (ModelState.IsValid)
                {
                    var userId = _userManager.GetUserId(User);
                    
                    if (string.IsNullOrEmpty(userId))
                    {
                        ModelState.AddModelError("", "User not authenticated. Please log in and try again.");
                        return View(viewModel);
                    }
                    
                    // Map from ViewModel to Budget
                    var budget = new Budget
                    {
                        Name = viewModel.Name,
                        Month = viewModel.Month,
                        TotalIncome = viewModel.TotalIncome,
                        AdditionalIncome = viewModel.AdditionalIncome,
                        HousingExpenses = viewModel.HousingExpenses,
                        UtilitiesExpenses = viewModel.UtilitiesExpenses,
                        TransportationExpenses = viewModel.TransportationExpenses,
                        FoodExpenses = viewModel.FoodExpenses,
                        HealthcareExpenses = viewModel.HealthcareExpenses,
                        EntertainmentExpenses = viewModel.EntertainmentExpenses,
                        PersonalExpenses = viewModel.PersonalExpenses,
                        SavingsAllocation = viewModel.SavingsAllocation,
                        DebtPayment = viewModel.DebtPayment,
                        OtherExpenses = viewModel.OtherExpenses,
                        Notes = viewModel.Notes,
                        UserId = userId,
                        StartDate = DateTime.Now
                    };
                      await _budgetRepository.CreateAsync(budget);
                    TempData["SuccessMessage"] = "Budget created successfully!";
                    return RedirectToAction(nameof(Details), new { id = budget.Id });
                }
                else
                {
                    // Add debugging for validation errors
                    foreach (var modelError in ModelState.Values.SelectMany(v => v.Errors))
                    {
                        ModelState.AddModelError("", $"Validation Error: {modelError.ErrorMessage}");
                    }
                }
            }
            catch (Exception ex)
            {
                // Add more detailed error information
                ModelState.AddModelError("", $"An error occurred while creating the budget: {ex.Message}");
                if (ex.InnerException != null)
                {
                    ModelState.AddModelError("", $"Inner exception: {ex.InnerException.Message}");
                }
            }
            
            // Repopulate dropdown lists
            ViewData["Categories"] = new List<string> {
                "Housing", "Transportation", "Food", "Utilities", "Insurance",
                "Healthcare", "Debt", "Entertainment", "Clothing", "Education",
                "Gifts", "Personal", "Savings", "Other"
            };
            
            ViewData["RecurringPeriods"] = new List<string> {
                "Monthly", "Weekly", "Bi-weekly", "Quarterly", "Yearly"
            };            
            return View(viewModel);
        }

        // GET: Budget/Edit/5
        public async Task<IActionResult> Edit(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return NotFound();
            }

            var userId = _userManager.GetUserId(User);
            var budget = await _budgetRepository.GetByIdAsync(id);

            if (budget == null || budget.UserId != userId)
            {
                return NotFound();
            }

            ViewData["Categories"] = new List<string> {
                "Housing", "Transportation", "Food", "Utilities", "Insurance",
                "Healthcare", "Debt", "Entertainment", "Clothing", "Education",
                "Gifts", "Personal", "Savings", "Other"
            };
            
            ViewData["RecurringPeriods"] = new List<string> {
                "Monthly", "Weekly", "Bi-weekly", "Quarterly", "Yearly"
            };
            
            // Map Budget to BudgetViewModel
            var viewModel = new BudgetViewModel
            {
                Id = budget.Id,
                Name = budget.Name,
                Month = budget.Month,
                TotalIncome = budget.TotalIncome,
                AdditionalIncome = budget.AdditionalIncome,
                HousingExpenses = budget.HousingExpenses,
                UtilitiesExpenses = budget.UtilitiesExpenses,
                TransportationExpenses = budget.TransportationExpenses,
                FoodExpenses = budget.FoodExpenses,
                HealthcareExpenses = budget.HealthcareExpenses,
                EntertainmentExpenses = budget.EntertainmentExpenses,
                PersonalExpenses = budget.PersonalExpenses,
                SavingsAllocation = budget.SavingsAllocation,
                DebtPayment = budget.DebtPayment,
                OtherExpenses = budget.OtherExpenses,
                Notes = budget.Notes
            };            
            return View(viewModel);
        }

        // POST: Budget/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(string id, BudgetViewModel viewModel)
        {
            if (id != viewModel.Id)
            {
                return NotFound();
            }

            var userId = _userManager.GetUserId(User);
            var existingBudget = await _budgetRepository.GetByIdAsync(id);

            if (existingBudget == null || existingBudget.UserId != userId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                // Update the budget with values from the view model
                existingBudget.Name = viewModel.Name;
                existingBudget.Month = viewModel.Month;
                existingBudget.TotalIncome = viewModel.TotalIncome;
                existingBudget.AdditionalIncome = viewModel.AdditionalIncome;
                existingBudget.HousingExpenses = viewModel.HousingExpenses;
                existingBudget.UtilitiesExpenses = viewModel.UtilitiesExpenses;
                existingBudget.TransportationExpenses = viewModel.TransportationExpenses;
                existingBudget.FoodExpenses = viewModel.FoodExpenses;
                existingBudget.HealthcareExpenses = viewModel.HealthcareExpenses;
                existingBudget.EntertainmentExpenses = viewModel.EntertainmentExpenses;
                existingBudget.PersonalExpenses = viewModel.PersonalExpenses;
                existingBudget.SavingsAllocation = viewModel.SavingsAllocation;
                existingBudget.DebtPayment = viewModel.DebtPayment;
                existingBudget.OtherExpenses = viewModel.OtherExpenses;
                existingBudget.Notes = viewModel.Notes;
                existingBudget.UpdatedAt = DateTime.Now;
                
                await _budgetRepository.UpdateAsync(id, existingBudget);
                return RedirectToAction(nameof(Index));
            }
            
            // Repopulate dropdown lists
            ViewData["Categories"] = new List<string> {
                "Housing", "Transportation", "Food", "Utilities", "Insurance",
                "Healthcare", "Debt", "Entertainment", "Clothing", "Education",
                "Gifts", "Personal", "Savings", "Other"
            };
            
            ViewData["RecurringPeriods"] = new List<string> {
                "Monthly", "Weekly", "Bi-weekly", "Quarterly", "Yearly"
            };
            
            return View(viewModel);
        }

        // GET: Budget/Delete/5
        public async Task<IActionResult> Delete(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return NotFound();
            }

            var userId = _userManager.GetUserId(User);
            var budget = await _budgetRepository.GetByIdAsync(id);

            if (budget == null || budget.UserId != userId)
            {
                return NotFound();
            }

            return View(budget);
        }

        // POST: Budget/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(string id)
        {
            var userId = _userManager.GetUserId(User);
            var budget = await _budgetRepository.GetByIdAsync(id);

            if (budget == null || budget.UserId != userId)
            {
                return NotFound();
            }

            await _budgetRepository.DeleteAsync(id);
            return RedirectToAction(nameof(Index));
        }

        // POST: Budget/AddExpense/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddExpense(string budgetId, string description, decimal amount, string category, string vendor)
        {
            if (string.IsNullOrEmpty(budgetId) || amount <= 0)
            {
                return BadRequest();
            }

            var userId = _userManager.GetUserId(User);
            var budget = await _budgetRepository.GetByIdAsync(budgetId);

            if (budget == null || budget.UserId != userId)
            {
                return NotFound();
            }

            // Create the expense
            var expense = new Expense
            {
                BudgetId = budgetId,
                Description = description,
                Amount = amount,
                Category = category,
                Vendor = vendor,
                Date = DateTime.Now,
                UserId = userId
            };            await _expenseRepository.CreateAsync(expense);

            return RedirectToAction(nameof(Details), new { id = budgetId });
        }

        // GET: Budget/Calculator/5
        public async Task<IActionResult> Calculator(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return NotFound();
            }

            var userId = _userManager.GetUserId(User);
            var budget = await _budgetRepository.GetByIdAsync(id);

            if (budget == null || budget.UserId != userId)
            {
                return NotFound();
            }
            
            // Map Budget to BudgetViewModel
            var budgetViewModel = new BudgetViewModel
            {
                Id = budget.Id,
                Name = budget.Name,
                Month = budget.Month,
                TotalIncome = budget.TotalIncome,
                AdditionalIncome = budget.AdditionalIncome,
                HousingExpenses = budget.HousingExpenses,
                UtilitiesExpenses = budget.UtilitiesExpenses,
                TransportationExpenses = budget.TransportationExpenses,
                FoodExpenses = budget.FoodExpenses,
                HealthcareExpenses = budget.HealthcareExpenses,
                EntertainmentExpenses = budget.EntertainmentExpenses,
                PersonalExpenses = budget.PersonalExpenses,
                SavingsAllocation = budget.SavingsAllocation,
                DebtPayment = budget.DebtPayment,
                OtherExpenses = budget.OtherExpenses,
                Notes = budget.Notes
            };

            return View(budgetViewModel);
        }

        // POST: Budget/Calculator/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Calculator(string id, BudgetViewModel model)
        {
            if (string.IsNullOrEmpty(id))
            {
                return NotFound();
            }

            var userId = _userManager.GetUserId(User);
            var budget = await _budgetRepository.GetByIdAsync(id);

            if (budget == null || budget.UserId != userId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                // Update the budget with values from the view model
                budget.Name = model.Name;
                budget.Month = model.Month;
                budget.TotalIncome = model.TotalIncome;
                budget.AdditionalIncome = model.AdditionalIncome;
                budget.HousingExpenses = model.HousingExpenses;
                budget.UtilitiesExpenses = model.UtilitiesExpenses;
                budget.TransportationExpenses = model.TransportationExpenses;
                budget.FoodExpenses = model.FoodExpenses;
                budget.HealthcareExpenses = model.HealthcareExpenses;
                budget.EntertainmentExpenses = model.EntertainmentExpenses;
                budget.PersonalExpenses = model.PersonalExpenses;
                budget.SavingsAllocation = model.SavingsAllocation;
                budget.DebtPayment = model.DebtPayment;
                budget.OtherExpenses = model.OtherExpenses;
                budget.Notes = model.Notes;
                budget.UpdatedAt = DateTime.Now;

                await _budgetRepository.UpdateAsync(id, budget);
                return RedirectToAction(nameof(Details), new { id });
            }

            return View(model);
        }
    }
}