using Microsoft.AspNetCore.Identity;
using OnlineLibrary.Models;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace OnlineLibrary.Models
{
    public class ProfileViewModel
    {
        public IdentityUser User { get; set; }

        [Display(Name = "First Name")]
        public string FirstName { get; set; }

        [Display(Name = "Last Name")]
        public string LastName { get; set; }

        [Required]
        [EmailAddress]
        public string Email { get; set; }

        [Phone]
        [Display(Name = "Phone Number")]
        public string PhoneNumber { get; set; }

        [Display(Name = "Address")]
        public string Address { get; set; }

        public int TotalBorrowedBooks { get; set; }

        public int CurrentlyBorrowedBooks { get; set; }

        public List<Borrowing> CurrentBorrowings { get; set; }

        public List<Borrowing> BorrowingHistory { get; set; }
    }
}
