using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using System.Linq;
using System.Security.Claims;
using OnlineLibrary.Models;

namespace OnlineLibrary.Controllers
{
    [Authorize]
    public class AccountController : Controller
    {
        private readonly UserManager<IdentityUser> _userManager;
        private readonly LibraryContext _context;

        public AccountController(UserManager<IdentityUser> userManager, LibraryContext context)
        {
            _userManager = userManager;
            _context = context;
        }

        public async Task<IActionResult> Profile()
        {
            // Get the current user
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return NotFound($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");
            }

            // Get user's borrowing history
            var borrowings = await _context.Borrowings
                .Include(b => b.Book)
                .Include(b => b.BookRequests)
                .Where(b => b.UserId == user.Id)
                .OrderByDescending(b => b.BorrowDate)
                .ToListAsync();

            // Calculate statistics
            var totalBorrowedBooks = borrowings.Count;
            var currentlyBorrowedBooks = borrowings.Count(b => b.ActualReturnDate == null);

            // Get user claims for additional profile information
            var claims = await _userManager.GetClaimsAsync(user);
            var firstName = claims.FirstOrDefault(c => c.Type == "FirstName")?.Value;
            var lastName = claims.FirstOrDefault(c => c.Type == "LastName")?.Value;
            var phoneNumber = user.PhoneNumber;
            var address = claims.FirstOrDefault(c => c.Type == "Address")?.Value;

            // Create view model
            var viewModel = new ProfileViewModel
            {
                User = user,
                FirstName = firstName ?? string.Empty,
                LastName = lastName ?? string.Empty,
                Email = user.Email,
                PhoneNumber = phoneNumber ?? string.Empty,
                Address = address ?? string.Empty,
                TotalBorrowedBooks = totalBorrowedBooks,
                CurrentlyBorrowedBooks = currentlyBorrowedBooks,
                CurrentBorrowings = borrowings.Where(b => b.ActualReturnDate == null).ToList(),
                BorrowingHistory = borrowings.Where(b => b.ActualReturnDate != null).ToList()
            };

            return View(viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateProfile(ProfileViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View("Profile", model);
            }

            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return NotFound($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");
            }

            // Update email if changed
            if (model.Email != user.Email)
            {
                var setEmailResult = await _userManager.SetEmailAsync(user, model.Email);
                if (!setEmailResult.Succeeded)
                {
                    foreach (var error in setEmailResult.Errors)
                    {
                        ModelState.AddModelError(string.Empty, error.Description);
                    }
                    return View("Profile", model);
                }
            }

            // Update phone number if changed
            if (model.PhoneNumber != user.PhoneNumber)
            {
                var setPhoneResult = await _userManager.SetPhoneNumberAsync(user, model.PhoneNumber);
                if (!setPhoneResult.Succeeded)
                {
                    foreach (var error in setPhoneResult.Errors)
                    {
                        ModelState.AddModelError(string.Empty, error.Description);
                    }
                    return View("Profile", model);
                }
            }

            // Update claims for first name, last name, and address
            var claims = await _userManager.GetClaimsAsync(user);

            await UpdateClaim(user, claims, "FirstName", model.FirstName);
            await UpdateClaim(user, claims, "LastName", model.LastName);
            await UpdateClaim(user, claims, "Address", model.Address);

            TempData["StatusMessage"] = "Your profile has been updated";
            return RedirectToAction(nameof(Profile));
        }

        private async Task UpdateClaim(IdentityUser user, IList<Claim> claims, string claimType, string claimValue)
        {
            var existingClaim = claims.FirstOrDefault(c => c.Type == claimType);

            if (existingClaim != null)
            {
                await _userManager.RemoveClaimAsync(user, existingClaim);
            }

            await _userManager.AddClaimAsync(user, new Claim(claimType, claimValue ?? string.Empty));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RequestReturn(int borrowingId)
        {
            var borrowing = await _context.Borrowings
                .Include(b => b.Book)
                .FirstOrDefaultAsync(b => b.BorrowingId == borrowingId);

            if (borrowing == null)
            {
                return NotFound();
            }

            // Check if the borrowing belongs to the current user
            if (borrowing.UserId != _userManager.GetUserId(User))
            {
                return Forbid();
            }

            // Create a return request
            var request = new BookRequest
            {
                BorrowingId = borrowingId,
                UserId = borrowing.UserId,
                RequestType = "Return",
                RequestDate = System.DateTime.Now,
                Status = "Pending"
            };

            _context.BookRequests.Add(request);
            await _context.SaveChangesAsync();

            TempData["StatusMessage"] = $"Return request for '{borrowing.Book.Name}' has been submitted and is pending approval.";
            return RedirectToAction(nameof(Profile));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RequestExtension(int borrowingId)
        {
            var borrowing = await _context.Borrowings
                .Include(b => b.Book)
                .FirstOrDefaultAsync(b => b.BorrowingId == borrowingId);

            if (borrowing == null)
            {
                return NotFound();
            }

            // Check if the borrowing belongs to the current user
            if (borrowing.UserId != _userManager.GetUserId(User))
            {
                return Forbid();
            }

            // Create an extension request
            var request = new BookRequest
            {
                BorrowingId = borrowingId,
                UserId = borrowing.UserId,
                RequestType = "Extension",
                RequestDate = System.DateTime.Now,
                Status = "Pending"
            };

            _context.BookRequests.Add(request);
            await _context.SaveChangesAsync();

            TempData["StatusMessage"] = $"Extension request for '{borrowing.Book.Name}' has been submitted and is pending approval.";
            return RedirectToAction(nameof(Profile));
        }

        public IActionResult Login()
        {
            return View();
        }

        public IActionResult Register()
        {
            return View();
        }

        public IActionResult ResetPassword()
        {
            return View();
        }
    }
}
