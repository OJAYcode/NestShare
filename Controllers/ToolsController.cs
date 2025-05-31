using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;

namespace Thrift.Controllers
{
    [Authorize] // Optionally require login
    public class ToolsController : Controller
    {
        public IActionResult BudgetCalculator()
        {
            return View();
        }

        public IActionResult SavingsGoalTracker()
        {
            return View();
        }

        public IActionResult FinancialCalendar()
        {
            return View();
        }

        public IActionResult DebtPayoffPlanner()
        {
            return View();
        }
    }
}