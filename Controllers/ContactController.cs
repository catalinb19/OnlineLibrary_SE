using Microsoft.AspNetCore.Mvc;

namespace OnlineLibrary.Controllers
{
    public class ContactController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
