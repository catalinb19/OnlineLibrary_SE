using Microsoft.AspNetCore.Mvc;
using OnlineLibrary.Models;
using OnlineLibrary.Services;
using OnlineLibrary.Services.Interfaces;
using System.Linq;
using System.Threading.Tasks;

namespace OnlineLibrary.Controllers
{
    public class CategoriesController : Controller
    {

        private readonly ICategoryService _categoryService;

        public CategoriesController(ICategoryService categoryService)
        {
            _categoryService = categoryService;
        }

        // GET: Categories
        public async Task<IActionResult> Index(string searchQuery)

        {
            var categoriesWithDetails = await _categoryService.GetCategoriesWithCountsAsync(searchQuery);
            return View(categoriesWithDetails);
        }

        // GET: Categories/Details?genre=Fiction
        public async Task<IActionResult> Details(string genre)
        {
            if (string.IsNullOrEmpty(genre))
            {
                return NotFound();
            }

            var booksInCategory = await _categoryService.GetBooksByCategoryAsync(genre);

            if (!booksInCategory.Any())
            {
                ViewBag.Genre = genre;
                return View("Empty"); // Optional: show a message saying no books found
            }

            ViewBag.Genre = genre;
            return View(booksInCategory);

        }

        // GET: Categories/Details?genre=Fiction
        public async Task<IActionResult> Details(string genre)
        {
            if (string.IsNullOrEmpty(genre))
            {
                return NotFound();
            }

            // Get all books matching the genre
            var booksInCategory = await _context.Books
                .Where(b => b.Genre == genre)
                .ToListAsync();

            if (!booksInCategory.Any())
            {
                ViewBag.Genre = genre;
                return View("Empty"); // Optional: show a message saying no books found
            }

            ViewBag.Genre = genre;
            return View(booksInCategory);
        }

    }
}