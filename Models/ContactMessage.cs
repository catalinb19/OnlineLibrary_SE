using System;
using System.ComponentModel.DataAnnotations;

namespace OnlineLibrary.Models
{
    public class ContactMessage
    {
        public int ContactMessageId { get; set; }

        [Required]
        [StringLength(100)]
        public string Name { get; set; }

        [Required]
        [EmailAddress]
        [StringLength(100)]
        public string Email { get; set; }

        [StringLength(20)]
        public string Phone { get; set; }

        [Required]
        [StringLength(50)]
        public string Subject { get; set; }

        [Required]
        [StringLength(2000)]
        public string Message { get; set; }

        public bool SubscribeToNewsletter { get; set; }

        public DateTime SubmissionDate { get; set; }

        public string UserId { get; set; }

        public bool IsRead { get; set; }

        public string? AdminResponse { get; set; }

        public DateTime? ResponseDate { get; set; }
    }
}
