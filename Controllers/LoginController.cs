using Microsoft.AspNetCore.Mvc;

namespace Thrift.Controllers
{
    public class LoginController : Controller
    {
        public IActionResult Index()
        {
            return RedirectToPage("/Account/Login", new { area = "Identity" });
        }
    }
}