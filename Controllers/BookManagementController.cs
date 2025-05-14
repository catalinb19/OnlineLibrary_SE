using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using OnlineLibrary.Models;

namespace OnlineLibrary.Controllers
{
    [Authorize] // This ensures only logged-in users can access these actions
    public class BookManagementController : Controller
    {
        private readonly LibraryContext _context;

        public BookManagementController(LibraryContext context)
        {
            _context = context;
        }

        // GET: BookManagement/BookPage/5
        [AllowAnonymous]
        public async Task<IActionResult> BookPage(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var book = await _context.Books
                .Include(b => b.Reviews) // Assuming you have a Reviews navigation property
                .FirstOrDefaultAsync(m => m.BookId == id);

            if (book == null)
            {
                return NotFound();
            }

            // Calculate average rating if there are reviews
            if (book.Reviews != null && book.Reviews.Any())
            {
                ViewBag.AverageRating = book.Reviews.Average(r => r.Rating);
                ViewBag.ReviewCount = book.Reviews.Count;
            }
            else
            {
                ViewBag.AverageRating = 0;
                ViewBag.ReviewCount = 0;
            }

            // Get similar books (books with the same genre or by the same author)
            // This is a simple example - you might want to implement a more sophisticated recommendation system
            var similarBooks = await _context.Books
                .Where(b => b.BookId != id && (b.Author == book.Author || b.Genre == book.Genre))
                .Take(4)
                .ToListAsync();

            ViewBag.SimilarBooks = similarBooks;

            return View(book);
        }

        // GET: BookManagement/BorrowPage/5
        [Authorize] //Can be accessed only if the user is connected
        public async Task<IActionResult> BorrowPage(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var book = await _context.Books
                .FirstOrDefaultAsync(m => m.BookId == id);

            if (book == null)
            {
                return NotFound();
            }

            // Check if the book is available
            if (book.AvailableCopies <= 0)
            {
                TempData["ErrorMessage"] = "This book is not available for borrowing.";
                return RedirectToAction(nameof(BookPage), new { id = book.BookId });
            }

            // Set default borrowing dates
            ViewBag.BorrowDate = DateTime.Now.ToString("yyyy-MM-dd");
            ViewBag.ReturnDate = DateTime.Now.AddDays(14).ToString("yyyy-MM-dd");

            return View(book);
        }

        // POST: BookManagement/AddReview
        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddReview(int bookId, string reviewerName, int rating, string reviewText)
        {
            if (ModelState.IsValid)
            {
                var review = new Review
                {
                    BookId = bookId,
                    ReviewerName = reviewerName,
                    Rating = rating,
                    ReviewText = reviewText,
                    ReviewDate = DateTime.Now
                };

                _context.Reviews.Add(review);
                await _context.SaveChangesAsync();

                return RedirectToAction(nameof(BookPage), new { id = bookId });
            }

            return RedirectToAction(nameof(BookPage), new { id = bookId });
        }

        // POST: BookManagement/ProcessBorrow
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ProcessBorrow(int bookId, string borrowDate, string returnDate,
            string borrowingMethod, string deliveryAddress, string notes, string termsAgreement)
        {
            // Check if terms are agreed
            bool isTermsAgreed = termsAgreement != null && termsAgreement.ToLower() == "true";

            if (!isTermsAgreed)
            {
                TempData["ErrorMessage"] = "You must agree to the terms and conditions.";
                return RedirectToAction(nameof(BorrowPage), new { id = bookId });
            }

            var book = await _context.Books.FindAsync(bookId);
            if (book == null)
            {
                return NotFound();
            }

            if (book.AvailableCopies <= 0)
            {
                TempData["ErrorMessage"] = "This book is no longer available for borrowing.";
                return RedirectToAction(nameof(BookPage), new { id = bookId });
            }

            try
            {
                // Ensure borrowingMethod is not null
                borrowingMethod = string.IsNullOrEmpty(borrowingMethod) ? "pickup" : borrowingMethod;

                // Set default values for null fields
                notes = notes ?? string.Empty;

                // Only require delivery address if method is delivery
                if (borrowingMethod == "delivery" && string.IsNullOrEmpty(deliveryAddress))
                {
                    TempData["ErrorMessage"] = "Delivery address is required for home delivery.";
                    return RedirectToAction(nameof(BorrowPage), new { id = bookId });
                }

                var borrowing = new Borrowing
                {
                    BookId = bookId,
                    UserId = GetCurrentUserId(),
                    BorrowDate = DateTime.Parse(borrowDate),
                    ExpectedReturnDate = DateTime.Parse(returnDate),
                    BorrowingMethod = borrowingMethod,
                    DeliveryAddress = borrowingMethod == "delivery" ? deliveryAddress : string.Empty,
                    Notes = notes,
                    Status = "Borrowed"
                };

                book.AvailableCopies--;

                _context.Borrowings.Add(borrowing);
                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = "Book borrowed successfully!";
                return RedirectToAction("BorrowingConfirmation", new { id = borrowing.BorrowingId });
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"An error occurred: {ex.Message}";
                return RedirectToAction(nameof(BorrowPage), new { id = bookId });
            }
        }

        private string GetCurrentUserId()
        {
            return User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "anonymous";
        }

        // GET: BookManagement/BorrowingConfirmation/5
        public async Task<IActionResult> BorrowingConfirmation(int id)
        {
            var borrowing = await _context.Borrowings
                .Include(b => b.Book)
                .FirstOrDefaultAsync(m => m.BorrowingId == id);

            if (borrowing == null)
            {
                return NotFound();
            }

            return View(borrowing);
        }
    }
}
