using OnlineLibrary.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace OnlineLibrary.Services.Interfaces
{
    public interface IBookService
    {
        Task<List<Book>> GetAllBooksAsync();
        Task<Book> GetBookByIdAsync(int? id);
        Task<Book> GetBookWithReviewsAsync(int? id);
        Task AddBookAsync(Book book);
        Task<Book> GetBookForEditAsync(int? id);
        Task UpdateBookAsync(Book book);
        Task<Book> GetBookForDeleteAsync(int? id);
        Task DeleteBookAsync(int id);
        bool BookExists(int id);
    }
}