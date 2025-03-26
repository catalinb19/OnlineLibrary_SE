using Microsoft.AspNetCore.Mvc;

namespace OnlineLibrary.Controllers
{
    public class CategoriesController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
