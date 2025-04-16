using Microsoft.AspNetCore.Mvc;

namespace OnlineLibrary.Controllers
{
    public class BookManagementController : Controller
    {
        public IActionResult BookPage()
        {
            return View();
        }
        public IActionResult BorrowPage()
        {
            return View();
        }
    }
}
