using OnlineLibrary.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace OnlineLibrary.Models
{
    public class Borrowing
    {
        public int BorrowingId { get; set; }

        public int BookId { get; set; }

        [Required]
        [StringLength(128)]
        public string UserId { get; set; }

        [DataType(DataType.Date)]
        public DateTime BorrowDate { get; set; }

        [DataType(DataType.Date)]
        public DateTime ExpectedReturnDate { get; set; }

        [DataType(DataType.Date)]
        public DateTime? ActualReturnDate { get; set; }

        [Required]
        [StringLength(50)]
        public string BorrowingMethod { get; set; }

        [StringLength(500)]
        public string DeliveryAddress { get; set; }

        [StringLength(1000)]
        public string Notes { get; set; }

        [Required]
        [StringLength(50)]
        public string Status { get; set; } // e.g., "Borrowed", "Returned", "Overdue"

        // Navigation properties
        [ForeignKey("BookId")]
        public Book Book { get; set; }

        // Add navigation property for book requests
        public ICollection<BookRequest> BookRequests { get; set; }
    }
}
