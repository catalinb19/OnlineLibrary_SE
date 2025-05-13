using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using OnlineLibrary.Models;
using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;


namespace OnlineLibrary.Controllers
{
    [Authorize(Roles = "Administrator")]
    public class AdminController : Controller
    {
        private readonly LibraryContext _context;
        private readonly UserManager<IdentityUser> _userManager;

        public AdminController(LibraryContext context, UserManager<IdentityUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public async Task<IActionResult> BookRequests()
        {
            var pendingRequests = await _context.BookRequests
                .Include(r => r.Borrowing)
                .ThenInclude(b => b.Book)
                .Where(r => r.Status == "Pending")
                .OrderByDescending(r => r.RequestDate)
                .ToListAsync();

            // Get user information for each request
            var userIds = pendingRequests.Select(r => r.UserId).Distinct().ToList();
            var users = await _userManager.Users
                .Where(u => userIds.Contains(u.Id))
                .ToDictionaryAsync(u => u.Id, u => u);

            ViewBag.Users = users;

            return View(pendingRequests);
        }

        public async Task<IActionResult> ProcessedRequests()
        {
            var processedRequests = await _context.BookRequests
                .Include(r => r.Borrowing)
                .ThenInclude(b => b.Book)
                .Where(r => r.Status != "Pending")
                .OrderByDescending(r => r.ProcessedDate)
                .ToListAsync();

            // Get user information for each request
            var userIds = processedRequests.Select(r => r.UserId).Distinct().ToList();
            var users = await _userManager.Users
                .Where(u => userIds.Contains(u.Id))
                .ToDictionaryAsync(u => u.Id, u => u);

            ViewBag.Users = users;

            return View(processedRequests);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ApproveRequest(int requestId, string adminNotes)
        {
            var request = await _context.BookRequests
                .Include(r => r.Borrowing)
                .ThenInclude(b => b.Book)
                .FirstOrDefaultAsync(r => r.BookRequestId == requestId);

            if (request == null)
            {
                return NotFound();
            }

            // Process the request based on its type
            if (request.RequestType == "Return")
            {
                // Mark the book as returned
                request.Borrowing.ActualReturnDate = DateTime.Now;
                request.Borrowing.Status = "Returned";

                // Increase available copies
                request.Borrowing.Book.AvailableCopies++;
            }
            else if (request.RequestType == "Extension")
            {
                // Extend the borrowing period by 7 days
                request.Borrowing.ExpectedReturnDate = request.Borrowing.ExpectedReturnDate.AddDays(7);
                request.Borrowing.Status = "Extended";
            }

            // Update the request
            request.Status = "Approved";
            request.ProcessedDate = DateTime.Now;
            request.AdminNotes = adminNotes;

            await _context.SaveChangesAsync();

            TempData["StatusMessage"] = $"Request has been approved successfully.";
            return RedirectToAction(nameof(BookRequests));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DenyRequest(int requestId, string adminNotes)
        {
            var request = await _context.BookRequests
                .FirstOrDefaultAsync(r => r.BookRequestId == requestId);

            if (request == null)
            {
                return NotFound();
            }

            // Update the request
            request.Status = "Denied";
            request.ProcessedDate = DateTime.Now;
            request.AdminNotes = adminNotes;

            await _context.SaveChangesAsync();

            TempData["StatusMessage"] = $"Request has been denied.";
            return RedirectToAction(nameof(BookRequests));
        }

        // Contact Messages
        public async Task<IActionResult> ContactMessages()
        {
            var messages = await _context.ContactMessages
                .OrderByDescending(m => m.SubmissionDate)
                .ToListAsync();

            // Get user information for messages with UserId
            var userIds = messages.Where(m => !string.IsNullOrEmpty(m.UserId))
                .Select(m => m.UserId)
                .Distinct()
                .ToList();

            var users = new Dictionary<string, IdentityUser>();
            if (userIds.Any())
            {
                users = await _userManager.Users
                    .Where(u => userIds.Contains(u.Id))
                    .ToDictionaryAsync(u => u.Id, u => u);
            }

            ViewBag.Users = users;

            return View(messages);
        }

        public async Task<IActionResult> ContactMessageDetails(int id)
        {
            var message = await _context.ContactMessages
                .FirstOrDefaultAsync(m => m.ContactMessageId == id);

            if (message == null)
            {
                return NotFound();
            }

            // Mark as read if not already
            if (!message.IsRead)
            {
                message.IsRead = true;
                await _context.SaveChangesAsync();
            }

            // Get user information if available
            if (!string.IsNullOrEmpty(message.UserId))
            {
                var user = await _userManager.FindByIdAsync(message.UserId);
                if (user != null)
                {
                    ViewBag.User = user;

                    // Get user claims for additional profile information
                    var claims = await _userManager.GetClaimsAsync(user);
                    ViewBag.FirstName = claims.FirstOrDefault(c => c.Type == "FirstName")?.Value;
                    ViewBag.LastName = claims.FirstOrDefault(c => c.Type == "LastName")?.Value;
                    ViewBag.Address = claims.FirstOrDefault(c => c.Type == "Address")?.Value;
                }
            }

            return View(message);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RespondToMessage(int messageId, string adminResponse)
        {
            var message = await _context.ContactMessages
                .FirstOrDefaultAsync(m => m.ContactMessageId == messageId);

            if (message == null)
            {
                return NotFound();
            }

            message.AdminResponse = adminResponse;
            message.ResponseDate = DateTime.Now;

            await _context.SaveChangesAsync();

            TempData["StatusMessage"] = "Response has been saved.";
            return RedirectToAction(nameof(ContactMessageDetails), new { id = messageId });
        }
    }
}
