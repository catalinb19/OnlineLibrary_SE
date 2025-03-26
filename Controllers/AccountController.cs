using Microsoft.AspNetCore.Mvc;

namespace OnlineLibrary.Controllers
{
    public class AccountController : Controller
    {
        public IActionResult Profile()
        {
            return View();
        }
    }
}
