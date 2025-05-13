using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace OnlineLibrary.Models
{
    public class BookRequest
    {
        public int BookRequestId { get; set; }

        public int BorrowingId { get; set; }

        [Required]
        public string UserId { get; set; }

        [Required]
        [StringLength(50)]
        public string RequestType { get; set; } // "Return" or "Extension"

        public DateTime RequestDate { get; set; }

        public DateTime? ProcessedDate { get; set; }

        [Required]
        [StringLength(50)]
        public string Status { get; set; } // "Pending", "Approved", "Denied"

        [StringLength(500)]
        public string? AdminNotes { get; set; }

        // Navigation properties
        [ForeignKey("BorrowingId")]
        public Borrowing Borrowing { get; set; }
    }
}
