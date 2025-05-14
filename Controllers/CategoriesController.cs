using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OnlineLibrary.Models;
using System.Linq;
using System.Threading.Tasks;

namespace OnlineLibrary.Controllers
{
    public class CategoriesController : Controller
    {
        private readonly LibraryContext _context;

        public CategoriesController(LibraryContext context)
        {
            _context = context;
        }

        // GET: Categories
        public async Task<IActionResult> Index(string searchQuery)
        {
            // Get the books grouped by genre, with the count of books in each genre
            var categoryCounts = await _context.Books
                .GroupBy(b => b.Genre)  // Group by Genre (Category)
                .Select(g => new { Genre = g.Key, Count = g.Count() })  // Select genre and the number of books
                .ToListAsync();

            // If searchQuery is provided, filter by genre name (case-insensitive search)
            if (!string.IsNullOrEmpty(searchQuery))
            {
                categoryCounts = categoryCounts
                    .Where(c => c.Genre.Contains(searchQuery, System.StringComparison.OrdinalIgnoreCase))
                    .ToList();
            }

            // Hardcoded category descriptions and icons
            var categoryDetails = new Dictionary<string, (string Description, string Icon)>
            {
                { "Fiction", ("Novels, short stories, and other fictional works", "fas fa-book") },
                { "Science Fiction", ("Futuristic concepts, space travel, and technology", "fas fa-flask") },
                { "Fantasy", ("Magic, mythical creatures, and supernatural elements", "fas fa-hat-wizard") },
                { "History", ("Historical events, figures, and civilizations", "fas fa-landmark") },
                { "Science", ("Scientific discoveries, theories, and research", "fas fa-microscope") },
                { "Psychology", ("Human behavior, mental processes, and therapy", "fas fa-brain") },
                { "Business", ("Management, entrepreneurship, and economics", "fas fa-briefcase") },
                { "Cooking", ("Recipes, cooking techniques, and food culture", "fas fa-utensils") },
                { "Health & Fitness", ("Wellness, exercise, and nutrition", "fas fa-heartbeat") },
                { "Art & Photography", ("Visual arts, techniques, and famous artists", "fas fa-paint-brush") },
                { "Music", ("Musical theory, history, and biographies", "fas fa-music") },
                { "Children's Books", ("Stories and educational books for children", "fas fa-child") }
            };

            // Add description and icon to category counts
            var categoriesWithDetails = categoryCounts
                .Select(c => new
                {
                    c.Genre,
                    c.Count,
                    Description = categoryDetails.ContainsKey(c.Genre) ? categoryDetails[c.Genre].Description : "No description available",
                    Icon = categoryDetails.ContainsKey(c.Genre) ? categoryDetails[c.Genre].Icon : "fas fa-question-circle"
                })
                .ToList();

            // Return the view with the list of genres and their book counts
            return View(categoriesWithDetails);
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
