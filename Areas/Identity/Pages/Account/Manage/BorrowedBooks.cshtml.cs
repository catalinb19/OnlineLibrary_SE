using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using OnlineLibrary.Models;

namespace OnlineLibrary.Areas.Identity.Pages.Account.Manage
{
    public class BorrowedBooksModel : PageModel
    {
        private readonly UserManager<IdentityUser> _userManager;
        private readonly LibraryContext _context;

        public BorrowedBooksModel(
            UserManager<IdentityUser> userManager,
            LibraryContext context)
        {
            _userManager = userManager;
            _context = context;
        }

        [TempData]
        public string StatusMessage { get; set; }

        public List<Borrowing> CurrentBorrowings { get; set; }

        public async Task<IActionResult> OnGetAsync()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return NotFound($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");
            }

            // Get user's current borrowings and include related BookRequests
            CurrentBorrowings = await _context.Borrowings
                .Include(b => b.Book)
                .Include(b => b.BookRequests)  // Include BookRequests
                .Where(b => b.UserId == user.Id && b.ActualReturnDate == null)
                .OrderByDescending(b => b.BorrowDate)
                .ToListAsync();

            return Page();
        }

        public async Task<IActionResult> OnPostRequestReturnAsync(int borrowingId)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return NotFound($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");
            }

            var borrowing = await _context.Borrowings
                .Include(b => b.Book)
                .FirstOrDefaultAsync(b => b.BorrowingId == borrowingId);

            if (borrowing == null)
            {
                return NotFound();
            }

            // Check if the borrowing belongs to the current user
            if (borrowing.UserId != user.Id)
            {
                return Forbid();
            }

            // Create a return request
            var request = new BookRequest
            {
                BorrowingId = borrowingId,
                UserId = user.Id,
                RequestType = "Return",
                RequestDate = DateTime.Now,
                Status = "Pending"
            };

            _context.BookRequests.Add(request);
            await _context.SaveChangesAsync();

            StatusMessage = $"Return request for '{borrowing.Book.Name}' has been submitted and is pending approval.";
            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostRequestExtensionAsync(int borrowingId)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return NotFound($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");
            }

            var borrowing = await _context.Borrowings
                .Include(b => b.Book)
                .FirstOrDefaultAsync(b => b.BorrowingId == borrowingId);

            if (borrowing == null)
            {
                return NotFound();
            }

            // Check if the borrowing belongs to the current user
            if (borrowing.UserId != user.Id)
            {
                return Forbid();
            }

            // Create an extension request
            var request = new BookRequest
            {
                BorrowingId = borrowingId,
                UserId = user.Id,
                RequestType = "Extension",
                RequestDate = DateTime.Now,
                Status = "Pending"
            };

            _context.BookRequests.Add(request);
            await _context.SaveChangesAsync();

            StatusMessage = $"Extension request for '{borrowing.Book.Name}' has been submitted and is pending approval.";
            return RedirectToPage();
        }
    }

}
