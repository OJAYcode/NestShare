using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Thrift.Areas.Identity.Pages.Account
{
    public class RegistrationSuccessModel : PageModel
    {
        public void OnGet()
        {
            // Get email from TempData and store in ViewData
            if (TempData["Email"] != null)
            {
                ViewData["Email"] = TempData["Email"].ToString();
            }
        }
    }
}