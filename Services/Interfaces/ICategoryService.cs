using OnlineLibrary.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace OnlineLibrary.Services.Interfaces
{
    public interface ICategoryService
    {
        Task<IEnumerable<dynamic>> GetCategoriesWithCountsAsync(string searchQuery = null);
        Task<List<Book>> GetBooksByCategoryAsync(string genre);
        Dictionary<string, (string Description, string Icon)> GetCategoryDetails();
    }
}