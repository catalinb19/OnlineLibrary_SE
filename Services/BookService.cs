using Microsoft.EntityFrameworkCore;
using OnlineLibrary.Models;
using OnlineLibrary.Services;
using OnlineLibrary.Services.Interfaces;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace OnlineLibrary.Services.Implementations
{
    public class BookService : IBookService
    {
        private readonly LibraryContext _context;

        public BookService(LibraryContext context)
        {
            _context = context;
        }

        public async Task<List<Book>> GetAllBooksAsync()
        {
            return await _context.Books.ToListAsync();
        }

        public async Task<Book> GetBookByIdAsync(int? id)
        {
            if (id == null)
                return null;

            return await _context.Books.FindAsync(id);
        }

        public async Task<Book> GetBookWithReviewsAsync(int? id)
        {
            if (id == null)
                return null;

            return await _context.Books
                .Include(b => b.Reviews)
                .FirstOrDefaultAsync(m => m.BookId == id);
        }

        public async Task AddBookAsync(Book book)
        {
            _context.Add(book);
            await _context.SaveChangesAsync();
        }

        public async Task<Book> GetBookForEditAsync(int? id)
        {
            if (id == null)
                return null;

            return await _context.Books.FindAsync(id);
        }

        public async Task UpdateBookAsync(Book book)
        {
            _context.Update(book);
            await _context.SaveChangesAsync();
        }

        public async Task<Book> GetBookForDeleteAsync(int? id)
        {
            if (id == null)
                return null;

            return await _context.Books
                .FirstOrDefaultAsync(m => m.BookId == id);
        }

        public async Task DeleteBookAsync(int id)
        {
            var book = await _context.Books.FindAsync(id);
            if (book != null)
            {
                _context.Books.Remove(book);
                await _context.SaveChangesAsync();
            }
        }

        public bool BookExists(int id)
        {
            return _context.Books.Any(e => e.BookId == id);
        }
    }
}