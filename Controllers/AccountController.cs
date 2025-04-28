using Microsoft.AspNetCore.Mvc;

namespace OnlineLibrary.Controllers
{
    public class AccountController : Controller
    {
        public IActionResult Profile()
        {
            return View();
        }
        public IActionResult Login()
        {
            return View();
        }

        public IActionResult Register()
        {
            return View();
        }
        public IActionResult ResetPassword()
        {
            return View();
        }
    }
}
