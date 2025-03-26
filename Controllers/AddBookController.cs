using Microsoft.AspNetCore.Mvc;

namespace OnlineLibrary.Controllers
{
    public class AddBookController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
