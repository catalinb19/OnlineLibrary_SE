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
    public class BorrowingHistoryModel : PageModel
    {
        private readonly UserManager<IdentityUser> _userManager;
        private readonly LibraryContext _context;

        public BorrowingHistoryModel(
            UserManager<IdentityUser> userManager,
            LibraryContext context)
        {
            _userManager = userManager;
            _context = context;
        }

        [TempData]
        public string StatusMessage { get; set; }

        public List<Borrowing> BorrowingHistory { get; set; }

        public async Task<IActionResult> OnGetAsync()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return NotFound($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");
            }

            // Get user's borrowing history
            BorrowingHistory = await _context.Borrowings
                .Include(b => b.Book)
                .Where(b => b.UserId == user.Id && b.ActualReturnDate != null)
                .OrderByDescending(b => b.ActualReturnDate)
                .ToListAsync();

            return Page();
        }
    }
}
