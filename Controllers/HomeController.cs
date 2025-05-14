using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OnlineLibrary.Models;
using System.Linq;

namespace OnlineLibrary.Controllers
{
    public class HomeController : Controller
    {
        private readonly LibraryContext _context;

        public HomeController(LibraryContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            // Get all books including their reviews
            var allBooks = await _context.Books
                .Include(b => b.Reviews) // Ensure reviews are loaded
                .ToListAsync();

            // Filter popular books 
            var popularBooksFiltered = allBooks
                .Where(b => b.IsPopular)
               .OrderByDescending(b =>
               {
                   double averageRating = 0;

                   if (b.Reviews.Any())
                   {
                       averageRating = b.Reviews.Average(r => r.Rating);
                   }

                   return averageRating;
               })
                .Take(5)
                .ToList();

            // Get new books ordered by release date
            var newBooks = allBooks
                .OrderByDescending(b => b.ReleaseDate)
                .Take(5)
                .ToList();

            var ratingMap = popularBooksFiltered.ToDictionary(
                b => b.BookId,
                b => b.Reviews.Any() ? b.Reviews.Average(r => r.Rating) : 0
                );

            // Pass the filtered books to the view
            ViewBag.PopularBooks = popularBooksFiltered;
            ViewBag.NewBooks = newBooks;
            ViewBag.AverageRatings = ratingMap;

            return View();
        }




        public IActionResult SearchResults(string searchTerm)
        {
            if (string.IsNullOrEmpty(searchTerm))
            {
                ViewBag.Message = "Please enter a search term";
                return View(new List<Book>());
            }

            ViewBag.SearchTerm = searchTerm;

            // Filter books based on the search term (assuming you're searching by book name or author)
            var searchResults = _context.Books
               .Where(b => b.Name.ToLower().Contains(searchTerm.ToLower()) || b.Author.ToLower().Contains(searchTerm.ToLower()))
                .ToList();

            // Pass the filtered books to the view
            return View(searchResults);
        }
    }
}
