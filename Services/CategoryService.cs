using Microsoft.EntityFrameworkCore;
using OnlineLibrary.Models;
using OnlineLibrary.Services;
using OnlineLibrary.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace OnlineLibrary.Services.Implementations
{
    public class CategoryService : ICategoryService
    {
        private readonly LibraryContext _context;

        public CategoryService(LibraryContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<dynamic>> GetCategoriesWithCountsAsync(string searchQuery = null)
        {
            // Get the books grouped by genre, with the count of books in each genre
            var categoryCounts = await _context.Books
                .GroupBy(b => b.Genre)  // Group by Genre (Category)
                .Select(g => new { Genre = g.Key, Count = g.Count() })  // Select genre and the number of books
                .ToListAsync();

            // If searchQuery is provided, filter by genre name (case-insensitive search)
            if (!string.IsNullOrEmpty(searchQuery))
            {
                categoryCounts = categoryCounts
                    .Where(c => c.Genre.Contains(searchQuery, StringComparison.OrdinalIgnoreCase))
                    .ToList();
            }

            // Add description and icon to category counts
            var categoryDetails = GetCategoryDetails();
            var categoriesWithDetails = categoryCounts
                .Select(c => new
                {
                    c.Genre,
                    c.Count,
                    Description = categoryDetails.ContainsKey(c.Genre) ? categoryDetails[c.Genre].Description : "No description available",
                    Icon = categoryDetails.ContainsKey(c.Genre) ? categoryDetails[c.Genre].Icon : "fas fa-question-circle"
                })
                .ToList();

            return categoriesWithDetails;
        }

        public async Task<List<Book>> GetBooksByCategoryAsync(string genre)
        {
            if (string.IsNullOrEmpty(genre))
                return new List<Book>();

            // Get all books matching the genre
            return await _context.Books
                .Where(b => b.Genre == genre)
                .ToListAsync();
        }

        public Dictionary<string, (string Description, string Icon)> GetCategoryDetails()
        {
            // Hardcoded category descriptions and icons
            return new Dictionary<string, (string Description, string Icon)>
            {
                { "Fiction", ("Novels, short stories, and other fictional works", "fas fa-book") },
                { "Science Fiction", ("Futuristic concepts, space travel, and technology", "fas fa-flask") },
                { "Fantasy", ("Magic, mythical creatures, and supernatural elements", "fas fa-hat-wizard") },
                { "History", ("Historical events, figures, and civilizations", "fas fa-landmark") },
                { "Science", ("Scientific discoveries, theories, and research", "fas fa-microscope") },
                { "Psychology", ("Human behavior, mental processes, and therapy", "fas fa-brain") },
                { "Business", ("Management, entrepreneurship, and economics", "fas fa-briefcase") },
                { "Cooking", ("Recipes, cooking techniques, and food culture", "fas fa-utensils") },
                { "Health & Fitness", ("Wellness, exercise, and nutrition", "fas fa-heartbeat") },
                { "Art & Photography", ("Visual arts, techniques, and famous artists", "fas fa-paint-brush") },
                { "Music", ("Musical theory, history, and biographies", "fas fa-music") },
                { "Children's Books", ("Stories and educational books for children", "fas fa-child") }
            };
        }
    }
}