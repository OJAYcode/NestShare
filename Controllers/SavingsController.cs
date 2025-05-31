using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using MongoDB.Bson;
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
    public class SavingsController : Controller
    {        private readonly SavingsGoalRepository _goalRepository;
        private readonly SavingsTransactionRepository _transactionRepository;
        private readonly UserManager<User> _userManager;

        public SavingsController(
            SavingsGoalRepository goalRepository,
            SavingsTransactionRepository transactionRepository,
            UserManager<User> userManager)
        {
            _goalRepository = goalRepository;
            _transactionRepository = transactionRepository;
            _userManager = userManager;
        }        // GET: Savings
        public async Task<IActionResult> Index()
        {
            try
            {
                var userId = _userManager.GetUserId(User);
                System.Diagnostics.Debug.WriteLine($"Index: User ID: {userId}");
                
                if (string.IsNullOrEmpty(userId))
                {
                    System.Diagnostics.Debug.WriteLine("Index: User ID is null, redirecting to login");
                    return RedirectToAction("Login", "Account");
                }
                
                var goals = await _goalRepository.GetAllAsync(userId);
                System.Diagnostics.Debug.WriteLine($"Index: Retrieved {goals.Count} goals");
                
                foreach (var goal in goals)
                {
                    System.Diagnostics.Debug.WriteLine($"Goal: {goal.Name}, ID: {goal.Id}, UserID: {goal.UserId}");
                }
                
                return View(goals);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Index error: {ex.Message}");
                TempData["ErrorMessage"] = $"Error loading savings goals: {ex.Message}";
                return View(new List<SavingsGoal>());
            }
        }

        // GET: Savings/Details/5
        public async Task<IActionResult> Details(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return NotFound();
            }

            var userId = _userManager.GetUserId(User);
            var goal = await _goalRepository.GetByIdAsync(id);

            if (goal == null || goal.UserId != userId)
            {
                return NotFound();
            }

            // Load transactions for this goal
            var transactions = await _transactionRepository.GetAllForGoalAsync(id);
            goal.Transactions = transactions;

            return View(goal);
        }        // GET: Savings/Create
        public IActionResult Create()
        {
            var categories = new List<string> { 
                "Emergency", "Retirement", "Education", "Travel", 
                "Home", "Vehicle", "Wedding", "Electronics", 
                "Investment", "Debt", "Other" 
            };
            
            var priorities = new List<string> { "Low", "Medium", "High", "Urgent" };
            
            ViewBag.Categories = categories.Select(c => new Microsoft.AspNetCore.Mvc.Rendering.SelectListItem { Value = c, Text = c }).ToList();
            ViewBag.Priorities = priorities.Select(p => new Microsoft.AspNetCore.Mvc.Rendering.SelectListItem { Value = p, Text = p }).ToList();
            
            ViewData["Icons"] = new List<string> { 
                "piggy-bank", "home", "car", "plane", "graduation-cap", 
                "ring", "laptop", "umbrella", "gift", "book" 
            };
            
            ViewData["Colors"] = new List<string> { 
                "purple", "blue", "green", "orange", "red", "pink", "teal", "indigo" 
            };
              return View();
        }

        // POST: Savings/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(SavingsGoal savingsGoal)
        {
            try
            {
                System.Diagnostics.Debug.WriteLine($"=== CREATE POST METHOD CALLED ===");
                System.Diagnostics.Debug.WriteLine($"ModelState.IsValid: {ModelState.IsValid}");
                System.Diagnostics.Debug.WriteLine($"Received goal: Name='{savingsGoal.Name}', TargetAmount={savingsGoal.TargetAmount}, Category='{savingsGoal.Category}'");
                
                // Log all form data received
                foreach (var key in Request.Form.Keys)
                {
                    System.Diagnostics.Debug.WriteLine($"Form[{key}] = '{Request.Form[key]}'");
                }                // Get user ID first
                var userId = _userManager.GetUserId(User);
                if (string.IsNullOrEmpty(userId))
                {
                    ModelState.AddModelError("", "User not found. Please log in again.");
                    System.Diagnostics.Debug.WriteLine("User ID is null or empty");
                    // Repopulate dropdown lists and return view
                    PopulateViewData();
                    return View(savingsGoal);
                }
                  // Set required fields that might be missing
                savingsGoal.UserId = userId;
                if (string.IsNullOrEmpty(savingsGoal.Id))
                    savingsGoal.Id = ObjectId.GenerateNewId().ToString();
                if (savingsGoal.CurrentAmount == 0)
                    savingsGoal.CurrentAmount = 0;
                if (string.IsNullOrEmpty(savingsGoal.Status))
                    savingsGoal.Status = "Active";
                if (string.IsNullOrEmpty(savingsGoal.Priority))
                    savingsGoal.Priority = "Medium";
                if (string.IsNullOrEmpty(savingsGoal.Icon))
                    savingsGoal.Icon = "piggy-bank";
                if (string.IsNullOrEmpty(savingsGoal.ColorTheme))
                    savingsGoal.ColorTheme = "purple";
                if (string.IsNullOrEmpty(savingsGoal.Color))
                    savingsGoal.Color = "#8B5CF6";
                
                // Clear validation errors for auto-generated/auto-set fields
                ModelState.Remove("UserId");
                ModelState.Remove("Id");
                
                if (!ModelState.IsValid)
                {
                    System.Diagnostics.Debug.WriteLine("=== MODEL STATE ERRORS ===");
                    var errorMessages = new List<string>();
                    foreach (var modelError in ModelState)
                    {
                        var key = modelError.Key;
                        var errors = modelError.Value.Errors;
                        if (errors.Count > 0)
                        {
                            var fieldErrors = string.Join(", ", errors.Select(e => e.ErrorMessage));
                            System.Diagnostics.Debug.WriteLine($"Field '{key}': {fieldErrors}");
                            errorMessages.Add($"{key}: {fieldErrors}");
                        }
                    }
                    
                    TempData["DetailedErrors"] = string.Join(" | ", errorMessages);
                    TempData["ErrorMessage"] = "Please fix the validation errors and try again.";
                    
                    // Repopulate dropdown lists
                    PopulateViewData();
                    return View(savingsGoal);
                }
                
                System.Diagnostics.Debug.WriteLine($"User ID: {userId}");
                System.Diagnostics.Debug.WriteLine($"About to save goal: {savingsGoal.Name}, Category: {savingsGoal.Category}, Target: {savingsGoal.TargetAmount}");
                  await _goalRepository.CreateAsync(savingsGoal);
                System.Diagnostics.Debug.WriteLine($"Goal saved successfully with ID: {savingsGoal.Id}");
                
                TempData["SuccessMessage"] = $"Savings goal '{savingsGoal.Name}' created successfully!";
                
                // Redirect to Details page to show the created goal
                return RedirectToAction("Details", new { id = savingsGoal.Id });
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", $"Error creating goal: {ex.Message}");
                System.Diagnostics.Debug.WriteLine($"Error creating savings goal: {ex}");                TempData["ErrorMessage"] = $"An error occurred while creating the goal: {ex.Message}";
                // Repopulate dropdown lists
                PopulateViewData();
                return View(savingsGoal);
            }
        }

        // GET: Savings/Edit/5
        public async Task<IActionResult> Edit(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return NotFound();
            }

            var userId = _userManager.GetUserId(User);
            var goal = await _goalRepository.GetByIdAsync(id);
            
            if (goal == null || goal.UserId != userId)
            {
                return NotFound();
            }

            // Use the PopulateViewData method to ensure consistency
            PopulateViewData();
            
            return View(goal);
        }

        // POST: Savings/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(string id, SavingsGoal savingsGoal)
        {
            if (id != savingsGoal.Id)
            {
                return NotFound();
            }

            var userId = _userManager.GetUserId(User);
            var existingGoal = await _goalRepository.GetByIdAsync(id);

            if (existingGoal == null || existingGoal.UserId != userId)
            {
                return NotFound();
            }

            try
            {
                if (ModelState.IsValid)
                {
                    // Keep the current amount and user ID
                    savingsGoal.CurrentAmount = existingGoal.CurrentAmount;
                    savingsGoal.UserId = userId;
                    
                    await _goalRepository.UpdateAsync(id, savingsGoal);
                    TempData["SuccessMessage"] = $"Savings goal '{savingsGoal.Name}' updated successfully!";
                    return RedirectToAction(nameof(Index));
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine("=== MODEL STATE ERRORS IN EDIT ===");
                    foreach (var modelError in ModelState)
                    {
                        var key = modelError.Key;
                        var errors = modelError.Value.Errors;
                        if (errors.Count > 0)
                        {
                            var fieldErrors = string.Join(", ", errors.Select(e => e.ErrorMessage));
                            System.Diagnostics.Debug.WriteLine($"Field '{key}': {fieldErrors}");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error updating savings goal: {ex}");
                TempData["ErrorMessage"] = $"An error occurred while updating the goal: {ex.Message}";
                ModelState.AddModelError("", $"Error updating goal: {ex.Message}");
            }
              // Use the PopulateViewData method to ensure consistency
            PopulateViewData();
            
            return View(savingsGoal);
        }

        // GET: Savings/Delete/5
        public async Task<IActionResult> Delete(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return NotFound();
            }

            var userId = _userManager.GetUserId(User);
            var goal = await _goalRepository.GetByIdAsync(id);

            if (goal == null || goal.UserId != userId)
            {
                return NotFound();
            }

            return View(goal);
        }

        // POST: Savings/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(string id)
        {
            var userId = _userManager.GetUserId(User);
            var goal = await _goalRepository.GetByIdAsync(id);

            if (goal == null || goal.UserId != userId)
            {
                return NotFound();
            }

            await _goalRepository.DeleteAsync(id);
            return RedirectToAction(nameof(Index));
        }        // POST: Savings/AddTransaction/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddTransaction(SavingsTransaction transaction)
        {
            if (transaction == null || string.IsNullOrEmpty(transaction.SavingsGoalId) || transaction.Amount <= 0)
            {
                TempData["ErrorMessage"] = "Invalid transaction data.";
                return RedirectToAction(nameof(Index));
            }

            var userId = _userManager.GetUserId(User);
            var goal = await _goalRepository.GetByIdAsync(transaction.SavingsGoalId);

            if (goal == null || goal.UserId != userId)
            {
                return NotFound();
            }            // Set transaction properties
            transaction.UserId = userId;
            transaction.TransactionDate = DateTime.Now;
            if (string.IsNullOrEmpty(transaction.Id))
            {
                transaction.Id = ObjectId.GenerateNewId().ToString();
            }

            try
            {
                // Save the transaction to database
                await _transactionRepository.CreateAsync(transaction);

                // Update the goal's current amount
                if (transaction.Type == "Deposit")
                {
                    goal.CurrentAmount += transaction.Amount;
                }
                else if (transaction.Type == "Withdrawal")
                {
                    goal.CurrentAmount = Math.Max(0, goal.CurrentAmount - transaction.Amount);
                }

                // Check if goal is completed
                if (goal.CurrentAmount >= goal.TargetAmount && goal.Status == "Active")
                {
                    goal.Status = "Completed";
                }

                await _goalRepository.UpdateAsync(transaction.SavingsGoalId, goal);
                
                TempData["SuccessMessage"] = $"Transaction of ${transaction.Amount:F2} added successfully!";
                return RedirectToAction(nameof(Details), new { id = transaction.SavingsGoalId });
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Error adding transaction: {ex.Message}";
                return RedirectToAction(nameof(AddTransaction), new { id = transaction.SavingsGoalId });
            }
        }
        
        // GET: Savings/Analytics
        public async Task<IActionResult> Analytics()
        {
            var userId = _userManager.GetUserId(User);
            if (string.IsNullOrEmpty(userId))
            {
                return RedirectToAction("Login", "Account");
            }
            
            var goals = await _goalRepository.GetAllAsync(userId);
            
            // Calculate analytics data
            var totalGoals = goals.Count;
            var activeGoals = goals.Count(g => g.Status == "Active");
            var completedGoals = goals.Count(g => g.Status == "Completed");
            var totalSaved = goals.Sum(g => g.CurrentAmount);
            var totalTarget = goals.Sum(g => g.TargetAmount);
            
            // Calculate monthly average from recent transactions
            var allTransactions = new List<SavingsTransaction>();
            foreach (var goal in goals)
            {
                var transactions = await _transactionRepository.GetAllForGoalAsync(goal.Id);
                allTransactions.AddRange(transactions);
            }
            
            var monthlyAverage = 0m;
            if (allTransactions.Any())
            {
                var recentTransactions = allTransactions
                    .Where(t => t.TransactionDate >= DateTime.Now.AddMonths(-12))
                    .Where(t => t.Type == "Deposit")
                    .ToList();
                
                if (recentTransactions.Any())
                {
                    monthlyAverage = recentTransactions.Sum(t => t.Amount) / 12m;
                }
            }
            
            // Calculate savings rate (simplified - would normally need income data)
            var savingsRate = totalSaved > 0 ? Math.Min((monthlyAverage * 12 / Math.Max(totalSaved, 1)) * 100, 100) : 0;
            
            ViewBag.TotalGoals = totalGoals;
            ViewBag.ActiveGoals = activeGoals;
            ViewBag.CompletedGoals = completedGoals;
            ViewBag.TotalSaved = totalSaved;
            ViewBag.TotalTarget = totalTarget;
            ViewBag.MonthlyAverage = monthlyAverage;
            ViewBag.SavingsRate = savingsRate;
            
            return View();
        }        // GET: Savings/AddTransaction/5
        public async Task<IActionResult> AddTransaction(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return NotFound();
            }

            var userId = _userManager.GetUserId(User);
            var goal = await _goalRepository.GetByIdAsync(id);

            if (goal == null || goal.UserId != userId)
            {
                return NotFound();
            }

            // Create a new SavingsTransaction model for the view
            var transaction = new SavingsTransaction
            {
                SavingsGoalId = id,
                UserId = userId,
                TransactionDate = DateTime.Now,
                Type = "Deposit"
            };

            ViewBag.GoalId = id;
            ViewBag.SavingsGoal = goal; // Pass the goal through ViewBag for display purposes
            return View(transaction);
        }// GET: Savings/Notifications
        public async Task<IActionResult> Notifications()
        {
            var userId = _userManager.GetUserId(User);
            if (string.IsNullOrEmpty(userId))
            {
                return RedirectToAction("Login", "Account");
            }
            
            var goals = await _goalRepository.GetAllAsync(userId);
            
            // Generate notifications based on goals status
            var notifications = new List<object>();
            
            foreach (var goal in goals)
            {
                // Check for completed goals
                if (goal.Status == "Completed")
                {
                    notifications.Add(new {
                        Type = "achievement",
                        Title = "Goal Completed!",
                        Message = $"Congratulations! You've completed your '{goal.Name}' goal.",
                        Icon = "trophy",
                        Time = "Recently"
                    });
                }
                
                // Check for milestone achievements (50%, 75%, 90%)
                var progressPercentage = goal.TargetAmount > 0 ? (goal.CurrentAmount / goal.TargetAmount) * 100 : 0;
                if (progressPercentage >= 50 && progressPercentage < 75)
                {
                    notifications.Add(new {
                        Type = "milestone",
                        Title = "50% Milestone Reached!",
                        Message = $"You're halfway to your '{goal.Name}' goal!",
                        Icon = "star",
                        Time = "This week"
                    });
                }
                
                // Check for deadline warnings
                if (goal.TargetDate.HasValue && goal.TargetDate.Value <= DateTime.Now.AddDays(30) && goal.Status == "Active")
                {
                    notifications.Add(new {
                        Type = "reminder",
                        Title = "Goal Deadline Approaching",
                        Message = $"Your '{goal.Name}' goal deadline is in {(goal.TargetDate.Value - DateTime.Now).Days} days.",
                        Icon = "exclamation-triangle",
                        Time = "Today"
                    });
                }
            }
            
            ViewBag.Notifications = notifications;
            return View();
        }        // GET: Savings/Challenges
        public async Task<IActionResult> Challenges()
        {
            var userId = _userManager.GetUserId(User);
            if (string.IsNullOrEmpty(userId))
            {
                return RedirectToAction("Login", "Account");
            }
            
            var goals = await _goalRepository.GetAllAsync(userId);
            var transactions = await _transactionRepository.GetAllForUserAsync(userId);
            
            // Calculate player stats
            var totalSaved = goals.Sum(g => g.CurrentAmount);
            var completedGoals = goals.Count(g => g.Status == "Completed");
            var activeGoals = goals.Count(g => g.Status == "Active");
            var totalTransactions = transactions.Count();
            
            // Calculate XP based on activities
            var xp = (completedGoals * 500) + (totalTransactions * 50) + (int)(totalSaved / 10);
            var level = Math.Max(1, (int)Math.Floor(xp / 1000m) + 1);
            var currentLevelXP = xp % 1000;
            var nextLevelXP = 1000;
            
            // Calculate savings streak (consecutive days with transactions)
            var streak = CalculateSavingsStreak(transactions);
            
            // Player stats for ViewBag
            ViewBag.PlayerStats = new {
                XP = xp,
                Level = level,
                CurrentLevelXP = currentLevelXP,
                NextLevelXP = nextLevelXP,
                Streak = streak,
                CompletedGoals = completedGoals,
                TotalSaved = totalSaved
            };
            
            // Generate active challenges
            var activeChallenges = GenerateActiveChallenges(goals, transactions, streak);
            ViewBag.ActiveChallenges = activeChallenges;
            
            // Generate available challenges
            var availableChallenges = GenerateAvailableChallenges(goals, transactions);
            ViewBag.AvailableChallenges = availableChallenges;
            
            // Generate achievements
            var achievements = GenerateAchievements(completedGoals, totalSaved, streak, totalTransactions);
            ViewBag.Achievements = achievements;
            
            // Generate leaderboard (mock data for demonstration)
            var leaderboard = GenerateLeaderboard(level, xp);
            ViewBag.Leaderboard = leaderboard;
            
            return View();
        }
        
        private int CalculateSavingsStreak(IEnumerable<SavingsTransaction> transactions)
        {
            if (!transactions.Any()) return 0;
            
            var sortedTransactions = transactions.OrderByDescending(t => t.TransactionDate).ToList();
            var streak = 0;
            var currentDate = DateTime.Now.Date;
            
            // Check if there's a transaction today or yesterday to start the streak
            var hasRecentTransaction = sortedTransactions.Any(t => 
                t.TransactionDate.Date == currentDate || 
                t.TransactionDate.Date == currentDate.AddDays(-1));
            
            if (!hasRecentTransaction) return 0;
            
            // Count consecutive days with transactions
            for (int i = 0; i < 30; i++) // Check last 30 days
            {
                var checkDate = currentDate.AddDays(-i);
                if (sortedTransactions.Any(t => t.TransactionDate.Date == checkDate))
                {
                    streak++;
                }
                else
                {
                    break;
                }
            }
            
            return streak;
        }
        
        private List<object> GenerateActiveChallenges(IEnumerable<SavingsGoal> goals, IEnumerable<SavingsTransaction> transactions, int streak)
        {
            var challenges = new List<object>();
            
            // Daily Saver Challenge
            var todayTransactions = transactions.Count(t => t.TransactionDate.Date == DateTime.Now.Date);
            if (todayTransactions == 0)
            {
                challenges.Add(new {
                    Title = "Daily Saver",
                    Description = "Make a savings transaction today",
                    Progress = 0,
                    Target = 1,
                    Reward = "50 XP + 1 Day Streak",
                    TimeLeft = "23h 45m",
                    Type = "daily"
                });
            }
            
            // Weekly Warrior Challenge
            var weekStart = DateTime.Now.StartOfWeek(DayOfWeek.Monday);
            var weekTransactions = transactions.Count(t => t.TransactionDate >= weekStart);
            if (weekTransactions < 5)
            {
                challenges.Add(new {
                    Title = "Weekly Warrior",
                    Description = "Make 5 savings transactions this week",
                    Progress = weekTransactions,
                    Target = 5,
                    Reward = "200 XP + Streak Boost",
                    TimeLeft = "4d 12h",
                    Type = "weekly"
                });
            }
            
            // Goal Crusher Challenge
            var activeGoals = goals.Where(g => g.Status == "Active").ToList();
            if (activeGoals.Any())
            {                var nearCompletionGoals = activeGoals.Count(g => 
                    g.TargetAmount > 0 && (g.CurrentAmount / g.TargetAmount) >= 0.8m);
                
                if (nearCompletionGoals > 0)
                {
                    challenges.Add(new {
                        Title = "Goal Crusher",
                        Description = "Complete a savings goal that's 80%+ done",
                        Progress = 0,
                        Target = 1,
                        Reward = "500 XP + Achievement Badge",
                        TimeLeft = "No limit",
                        Type = "goal"
                    });
                }
            }
            
            return challenges;
        }
        
        private List<object> GenerateAvailableChallenges(IEnumerable<SavingsGoal> goals, IEnumerable<SavingsTransaction> transactions)
        {
            var challenges = new List<object>
            {
                new {
                    Title = "Savings Marathon",
                    Description = "Save money for 30 consecutive days",
                    Difficulty = "Hard",
                    Reward = "1000 XP + Marathon Badge",
                    Requirements = "Maintain daily savings streak",
                    EstimatedTime = "30 days"
                },
                new {
                    Title = "Goal Master",
                    Description = "Create and complete 5 savings goals",
                    Difficulty = "Medium",
                    Reward = "750 XP + Master Badge",
                    Requirements = "Complete 5 goals total",
                    EstimatedTime = "2-3 months"
                },
                new {
                    Title = "Penny Pincher",
                    Description = "Save $1000 in total across all goals",
                    Difficulty = "Medium",
                    Reward = "500 XP + Savings Badge",
                    Requirements = "Accumulate $1000 total savings",
                    EstimatedTime = "1-2 months"
                },
                new {
                    Title = "Quick Starter",
                    Description = "Create your first savings goal",
                    Difficulty = "Easy",
                    Reward = "100 XP + Starter Badge",
                    Requirements = "Create any savings goal",
                    EstimatedTime = "5 minutes"
                },
                new {
                    Title = "Consistency King",
                    Description = "Make 100 savings transactions",
                    Difficulty = "Hard",
                    Reward = "800 XP + Consistency Badge",
                    Requirements = "Complete 100 transactions",
                    EstimatedTime = "3-4 months"
                },
                new {
                    Title = "Emergency Fund Builder",
                    Description = "Build an emergency fund of $5000",
                    Difficulty = "Hard",
                    Reward = "1500 XP + Emergency Badge",
                    Requirements = "Save $5000 in emergency fund goal",
                    EstimatedTime = "6-12 months"
                }
            };
            
            return challenges;
        }
        
        private List<object> GenerateAchievements(int completedGoals, decimal totalSaved, int streak, int totalTransactions)
        {
            var achievements = new List<object>();
            
            // Earned achievements based on actual progress
            if (completedGoals >= 1)
            {
                achievements.Add(new {
                    Name = "First Goal",
                    Description = "Complete your first savings goal",
                    Icon = "trophy",
                    Color = "from-yellow-400 to-orange-500",
                    Earned = true,
                    DateEarned = "2 weeks ago"
                });
            }
            
            if (streak >= 7)
            {
                achievements.Add(new {
                    Name = "Week Warrior",
                    Description = "Save money for 7 consecutive days",
                    Icon = "fire",
                    Color = "from-red-400 to-pink-500",
                    Earned = true,
                    DateEarned = "1 week ago"
                });
            }
            
            if (totalSaved >= 1000)
            {
                achievements.Add(new {
                    Name = "Grand Saver",
                    Description = "Save $1000 across all goals",
                    Icon = "coins",
                    Color = "from-green-400 to-emerald-500",
                    Earned = true,
                    DateEarned = "3 days ago"
                });
            }
            
            if (totalTransactions >= 50)
            {
                achievements.Add(new {
                    Name = "Frequent Saver",
                    Description = "Make 50 savings transactions",
                    Icon = "chart-line",
                    Color = "from-blue-400 to-indigo-500",
                    Earned = true,
                    DateEarned = "5 days ago"
                });
            }
            
            // Locked achievements
            var lockedAchievements = new List<object>
            {
                new {
                    Name = "Goal Master",
                    Description = "Complete 10 savings goals",
                    Icon = "crown",
                    Color = "from-purple-400 to-indigo-500",
                    Earned = false,
                    Progress = $"{completedGoals}/10"
                },
                new {
                    Name = "Streak Legend",
                    Description = "Save money for 30 consecutive days",
                    Icon = "flame",
                    Color = "from-orange-400 to-red-500",
                    Earned = false,
                    Progress = $"{streak}/30"
                },
                new {
                    Name = "Millionaire Mind",
                    Description = "Save $10,000 total",
                    Icon = "gem",
                    Color = "from-emerald-400 to-teal-500",
                    Earned = false,
                    Progress = $"${totalSaved:F0}/$10,000"
                },
                new {
                    Name = "Transaction Hero",
                    Description = "Make 500 savings transactions",
                    Icon = "bolt",
                    Color = "from-yellow-400 to-orange-500",
                    Earned = false,
                    Progress = $"{totalTransactions}/500"
                }
            };
            
            achievements.AddRange(lockedAchievements);
            return achievements;
        }
        
        private List<object> GenerateLeaderboard(int userLevel, int userXP)
        {
            // Mock leaderboard data for demonstration
            var leaderboard = new List<object>
            {
                new { Rank = 1, Name = "Sarah Chen", Level = 12, XP = 12450, Avatar = "👩‍💼", IsCurrentUser = false },
                new { Rank = 2, Name = "Mike Johnson", Level = 11, XP = 11200, Avatar = "👨‍💻", IsCurrentUser = false },
                new { Rank = 3, Name = "Emma Davis", Level = 10, XP = 10800, Avatar = "👩‍🎨", IsCurrentUser = false },
                new { Rank = 4, Name = "You", Level = userLevel, XP = userXP, Avatar = "👤", IsCurrentUser = true },
                new { Rank = 5, Name = "Alex Rodriguez", Level = userLevel - 1, XP = userXP - 500, Avatar = "👨‍🚀", IsCurrentUser = false },
                new { Rank = 6, Name = "Lisa Wang", Level = userLevel - 1, XP = userXP - 750, Avatar = "👩‍🔬", IsCurrentUser = false },
                new { Rank = 7, Name = "David Kim", Level = userLevel - 2, XP = userXP - 1200, Avatar = "👨‍🎓", IsCurrentUser = false }
            };
            
            return leaderboard;
        }        // GET: Savings/Recommendations
        public async Task<IActionResult> Recommendations()
        {
            var userId = _userManager.GetUserId(User);
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized();
            }
            
            var goals = await _goalRepository.GetAllAsync(userId);
            var transactions = await _transactionRepository.GetAllForUserAsync(userId);
            
            // Generate personalized recommendations based on user data
            var recommendations = GenerateRecommendations(goals, transactions);
            
            ViewBag.Recommendations = recommendations;
            ViewBag.Goals = goals;
            ViewBag.Transactions = transactions;
            
            return View();
        }
        
        private List<object> GenerateRecommendations(IEnumerable<SavingsGoal> goals, IEnumerable<SavingsTransaction> transactions)
        {
            var recommendations = new List<object>();
            var totalSaved = goals.Sum(g => g.CurrentAmount);
            var activeGoals = goals.Where(g => g.Status == "Active").ToList();
            var completedGoals = goals.Where(g => g.Status == "Completed").ToList();
            
            // Calculate average monthly savings
            var monthlyTransactions = transactions
                .Where(t => t.TransactionDate >= DateTime.Now.AddMonths(-3))
                .GroupBy(t => new { t.TransactionDate.Year, t.TransactionDate.Month })
                .Select(g => g.Sum(t => t.Amount))
                .ToList();
            
            var avgMonthlySavings = monthlyTransactions.Any() ? monthlyTransactions.Average() : 0;
            
            // Recommendation 1: Emergency Fund
            if (!goals.Any(g => g.Name.ToLower().Contains("emergency") || g.Name.ToLower().Contains("fund")))
            {
                recommendations.Add(new {
                    Type = "essential",
                    Title = "Build an Emergency Fund",
                    Description = "Start with $1,000 as your initial emergency fund. This will protect you from unexpected expenses.",
                    Action = "Create Emergency Fund Goal",
                    Priority = "High",
                    Icon = "shield-alt",
                    Color = "from-red-500 to-pink-500",
                    EstimatedTime = "3-6 months",
                    SuggestedAmount = 1000
                });
            }
            
            // Recommendation 2: Increase Savings Rate
            if (avgMonthlySavings > 0 && avgMonthlySavings < 500)
            {
                var suggestedIncrease = Math.Min(avgMonthlySavings * 0.2m, 100);
                recommendations.Add(new {
                    Type = "optimization",
                    Title = "Boost Your Savings Rate",
                    Description = $"Consider increasing your monthly savings by ${suggestedIncrease:F0}. Small increases compound over time.",
                    Action = "Set Up Automatic Transfer",
                    Priority = "Medium",
                    Icon = "chart-line",
                    Color = "from-green-500 to-emerald-500",
                    EstimatedTime = "Immediate",
                    SuggestedAmount = (double)suggestedIncrease
                });
            }
            
            // Recommendation 3: Goal Completion Strategy
            var nearCompletionGoals = activeGoals.Where(g => 
                g.TargetAmount > 0 && (g.CurrentAmount / g.TargetAmount) >= 0.8m).ToList();
            
            if (nearCompletionGoals.Any())
            {
                var goal = nearCompletionGoals.First();
                var remaining = goal.TargetAmount - goal.CurrentAmount;
                recommendations.Add(new {
                    Type = "completion",
                    Title = $"Complete '{goal.Name}' Goal",
                    Description = $"You're ${remaining:F0} away from completing this goal. Consider allocating extra funds to finish it.",
                    Action = "Add Extra Payment",
                    Priority = "High",
                    Icon = "trophy",
                    Color = "from-yellow-500 to-orange-500",
                    EstimatedTime = "This month",
                    SuggestedAmount = (double)remaining
                });
            }
            
            // Recommendation 4: Retirement Savings
            if (!goals.Any(g => g.Name.ToLower().Contains("retirement") || g.Name.ToLower().Contains("401k")))
            {
                recommendations.Add(new {
                    Type = "longterm",
                    Title = "Start Retirement Planning",
                    Description = "The earlier you start, the more time your money has to grow. Consider starting with 10% of your income.",
                    Action = "Create Retirement Goal",
                    Priority = "Medium",
                    Icon = "hourglass-half",
                    Color = "from-blue-500 to-indigo-500",
                    EstimatedTime = "Ongoing",
                    SuggestedAmount = (double)(avgMonthlySavings * 12 * 0.1m)
                });
            }
            
            // Recommendation 5: Diversify Goals
            if (activeGoals.Count < 3 && totalSaved > 1000)
            {
                recommendations.Add(new {
                    Type = "diversification",
                    Title = "Diversify Your Savings Goals",
                    Description = "Consider creating multiple savings goals for different purposes: short-term, medium-term, and long-term objectives.",
                    Action = "Create Additional Goals",
                    Priority = "Low",
                    Icon = "layer-group",
                    Color = "from-purple-500 to-pink-500",
                    EstimatedTime = "15 minutes",
                    SuggestedAmount = 0
                });
            }
            
            // Recommendation 6: Automate Savings
            var lastWeekTransactions = transactions.Where(t => t.TransactionDate >= DateTime.Now.AddDays(-7)).Count();
            if (lastWeekTransactions == 0)
            {
                recommendations.Add(new {
                    Type = "automation",
                    Title = "Automate Your Savings",
                    Description = "Set up automatic transfers to make saving effortless and consistent. Even $25/week adds up to $1,300/year.",
                    Action = "Set Up Auto-Transfer",
                    Priority = "Medium",
                    Icon = "robot",
                    Color = "from-cyan-500 to-blue-500",
                    EstimatedTime = "10 minutes",
                    SuggestedAmount = 25
                });
            }
            
            return recommendations;
        }

        // Test method to verify database connectivity
        public async Task<IActionResult> TestConnection()
        {
            try
            {
                var userId = _userManager.GetUserId(User);
                if (string.IsNullOrEmpty(userId))
                {
                    return Json(new { success = false, message = "User not authenticated" });
                }
                
                // Try to get all goals (this will test the connection)
                var goals = await _goalRepository.GetAllAsync(userId);
                
                return Json(new { 
                    success = true, 
                    message = $"Database connection successful. Found {goals.Count} goals for user {userId}",
                    goals = goals.Select(g => new { g.Id, g.Name, g.Category, g.TargetAmount })
                });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = $"Database error: {ex.Message}" });
            }
        }

        // Simple test method for debugging form submission
        [HttpPost]
        public async Task<IActionResult> TestCreate(string name, decimal targetAmount, string category, string priority)
        {
            try
            {
                System.Diagnostics.Debug.WriteLine($"TestCreate called with: name={name}, targetAmount={targetAmount}, category={category}, priority={priority}");
                
                var userId = _userManager.GetUserId(User);
                System.Diagnostics.Debug.WriteLine($"User ID: {userId}");
                
                if (string.IsNullOrEmpty(userId))
                {
                    return Json(new { success = false, message = "User not authenticated" });
                }
                
                var goal = new SavingsGoal
                {
                    Id = ObjectId.GenerateNewId().ToString(),
                    Name = name ?? "Test Goal",
                    TargetAmount = targetAmount > 0 ? targetAmount : 1000,
                    Category = category ?? "Other",
                    Priority = priority ?? "Medium",
                    UserId = userId,
                    CurrentAmount = 0,
                    StartDate = DateTime.Now,
                    Status = "Active",
                    Icon = "piggy-bank",
                    ColorTheme = "purple",
                    Color = "#8B5CF6"
                };
                  System.Diagnostics.Debug.WriteLine($"About to create goal: {goal.Name}");
                await _goalRepository.CreateAsync(goal);
                System.Diagnostics.Debug.WriteLine($"Goal created successfully with ID: {goal.Id}");
                
                // Test redirect behavior
                if (Request.Headers["Accept"].ToString().Contains("application/json"))
                {
                    return Json(new { success = true, message = "Goal created successfully", goalId = goal.Id, redirectUrl = Url.Action("Details", new { id = goal.Id }) });
                }
                else
                {
                    TempData["SuccessMessage"] = "Goal created successfully!";
                    return RedirectToAction("Details", new { id = goal.Id });
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error in TestCreate: {ex.Message}");
                return Json(new { success = false, message = ex.Message });
            }
        }

        // Simple test method for creating a goal programmatically
        [HttpGet]
        public async Task<IActionResult> CreateTestGoal()
        {
            try
            {
                var userId = _userManager.GetUserId(User);
                if (string.IsNullOrEmpty(userId))
                {
                    return Json(new { success = false, message = "User not authenticated" });
                }

                var testGoal = new SavingsGoal
                {
                    Id = ObjectId.GenerateNewId().ToString(),
                    Name = "Test Goal - " + DateTime.Now.ToString("HH:mm:ss"),
                    Description = "Test goal created programmatically",
                    TargetAmount = 1000m,
                    CurrentAmount = 0,
                    Category = "Test",
                    Priority = "Medium",
                    UserId = userId,
                    StartDate = DateTime.Now,
                    Status = "Active",
                    Icon = "piggy-bank",
                    ColorTheme = "purple",
                    Color = "#8B5CF6"
                };

                System.Diagnostics.Debug.WriteLine($"Creating test goal: {testGoal.Name}");
                await _goalRepository.CreateAsync(testGoal);
                System.Diagnostics.Debug.WriteLine($"Test goal created with ID: {testGoal.Id}");

                // Verify it was saved by trying to retrieve it
                var savedGoal = await _goalRepository.GetByIdAsync(testGoal.Id);
                
                return Json(new { 
                    success = true, 
                    message = "Test goal created successfully", 
                    goalId = testGoal.Id,
                    goalFound = savedGoal != null,
                    goalName = savedGoal?.Name
                });
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error creating test goal: {ex}");
                return Json(new { success = false, message = ex.Message, stackTrace = ex.StackTrace });
            }
        }

        // Simple test page for debugging
        public IActionResult SimpleTest()
        {
            return View();
        }
          private void PopulateViewData()
        {
            var categories = new List<string> { 
                "Emergency", "Retirement", "Education", "Travel", 
                "Home", "Vehicle", "Wedding", "Electronics", 
                "Investment", "Debt", "Other" 
            };
            
            var priorities = new List<string> { "Low", "Medium", "High", "Urgent" };
            var statuses = new List<string> { "Active", "Paused", "Completed", "Cancelled" };
            
            ViewBag.Categories = categories.Select(c => new Microsoft.AspNetCore.Mvc.Rendering.SelectListItem { Value = c, Text = c }).ToList();
            ViewBag.Priorities = priorities.Select(p => new Microsoft.AspNetCore.Mvc.Rendering.SelectListItem { Value = p, Text = p }).ToList();
            ViewBag.Statuses = statuses.Select(s => new Microsoft.AspNetCore.Mvc.Rendering.SelectListItem { Value = s, Text = s }).ToList();
            
            ViewData["Icons"] = new List<string> { 
                "piggy-bank", "home", "car", "plane", "graduation-cap", 
                "ring", "laptop", "umbrella", "gift", "book" 
            };
            
            ViewData["Colors"] = new List<string> { 
                "purple", "blue", "green", "orange", "red", "pink", "teal", "indigo" 
            };
        }
    }
}

// Extension method for week start calculation
public static class DateTimeExtensions
{
    public static DateTime StartOfWeek(this DateTime dt, DayOfWeek startOfWeek)
    {
        int diff = (7 + (dt.DayOfWeek - startOfWeek)) % 7;
        return dt.AddDays(-1 * diff).Date;
    }
}

public class AddFundsViewModel
{
    public string? GoalId { get; set; }
    public string? GoalName { get; set; }
    public decimal CurrentAmount { get; set; }
    public decimal TargetAmount { get; set; }
    public decimal Amount { get; set; }
    public string? Note { get; set; }
    public decimal ProgressPercentage { get; set; }
}