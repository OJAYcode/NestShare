using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;

namespace Thrift.Controllers
{
    public class FeaturesController : Controller
    {
        public IActionResult AiBudgetAssistant()
        {
            return View();
        }

        public IActionResult MobileIntegration()
        {
            return View();
        }

        public IActionResult GroupSavings()
        {
            return View();
        }
    }
}