using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace OnlineLibrary.Models    
{
    public class Book
    {
        [Key]
        public int BookId { get; set; }

        [Required, StringLength(200)]
        public string Name { get; set; }

        [Required, StringLength(100)]
        public string Author { get; set; }

        [DataType(DataType.Date)]
        public DateTime ReleaseDate { get; set; }

        [StringLength(20)]
        public string ISBN { get; set; }

        [StringLength(50)]
        public string Genre { get; set; }

        [StringLength(50)]
        public string Language { get; set; }

        public int Pages { get; set; }

        [StringLength(500)]
        public string Description { get; set; }

        public string CoverImageUrl { get; set; }

        public int AvailableCopies { get; set; }

        [NotMapped]
        public bool IsPopular => Reviews != null && Reviews.Count >= 2;

        // Navigation property for reviews
        public ICollection<Review> Reviews { get; set; } = new List<Review>();
        public ICollection<Borrowing> Borrowings { get; set; } = new List<Borrowing>();
    }
}
