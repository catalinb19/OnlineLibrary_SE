using Microsoft.AspNetCore.Mvc;

namespace OnlineLibrary.Controllers
{
    public class BookManagementController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
