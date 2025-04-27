using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;
using OnlineLibrary.Models;
using System.Reflection.Metadata;

namespace OnlineLibrary.Models
{
    public class LibraryContext : DbContext
    {
        public LibraryContext(DbContextOptions<LibraryContext> options)
        : base(options)

        { }

        public DbSet<Book>? Books { get; set; }

    }
}
