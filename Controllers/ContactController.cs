using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using OnlineLibrary.Models;
using System;
using System.Threading.Tasks;
using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using Microsoft.AspNetCore.Authorization;


namespace OnlineLibrary.Controllers
{
    public class ContactController : Controller
    {
        private readonly LibraryContext _context;
        private readonly UserManager<IdentityUser> _userManager;

        public ContactController(LibraryContext context, UserManager<IdentityUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public async Task<IActionResult> Index()
        {
            // Check if user is logged in
            if (User.Identity.IsAuthenticated)
            {
                var user = await _userManager.GetUserAsync(User);
                if (user != null)
                {
                    // Get user claims for additional profile information
                    var claims = await _userManager.GetClaimsAsync(user);
                    var firstName = claims.FirstOrDefault(c => c.Type == "FirstName")?.Value;
                    var lastName = claims.FirstOrDefault(c => c.Type == "LastName")?.Value;

                    // Pass user information to the view
                    ViewBag.UserName = string.IsNullOrEmpty(firstName) && string.IsNullOrEmpty(lastName)
                        ? user.UserName
                        : $"{firstName} {lastName}".Trim();
                    ViewBag.UserEmail = user.Email;
                    ViewBag.UserPhone = user.PhoneNumber;
                }
            }

            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SubmitMessage(string name, string email, string phone,
            string subject, string message, bool newsletter = false)
        {
            if (string.IsNullOrEmpty(name) || string.IsNullOrEmpty(email) ||
                string.IsNullOrEmpty(subject) || string.IsNullOrEmpty(message))
            {
                TempData["ErrorMessage"] = "Please fill in all required fields.";
                return RedirectToAction(nameof(Index));
            }

            // Create new contact message
            var contactMessage = new ContactMessage
            {
                Name = name,
                Email = email,
                Phone = phone ?? string.Empty,
                Subject = subject,
                Message = message,
                SubscribeToNewsletter = newsletter,
                SubmissionDate = DateTime.Now,
                IsRead = false
            };

            // If user is logged in, associate the message with their account
            if (User.Identity.IsAuthenticated)
            {
                contactMessage.UserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            }

            _context.ContactMessages.Add(contactMessage);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Your message has been sent successfully. We'll get back to you soon!";
            return RedirectToAction(nameof(Index));
        }

        [Authorize]
        public async Task<IActionResult> MyMessages()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var messages = await _context.ContactMessages
                .Where(m => m.UserId == userId)
                .OrderByDescending(m => m.SubmissionDate)
                .ToListAsync();

            return View(messages);
        }

        [Authorize]
        public async Task<IActionResult> MessageDetails(int id)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var message = await _context.ContactMessages
                .FirstOrDefaultAsync(m => m.ContactMessageId == id && m.UserId == userId);

            if (message == null)
            {
                return NotFound();
            }

            return View(message);
        }
    }
}
