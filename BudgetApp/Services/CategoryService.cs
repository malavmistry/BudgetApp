using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using BudgetApp.Data;
using BudgetApp.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace BudgetApp.Services
{
    public class CategoryService : ICategoryService
    {
        private readonly AppDbContext _dbContext;
        private readonly ILogger<CategoryService> _logger;

        public CategoryService(AppDbContext dbContext, ILogger<CategoryService> logger)
        {
            _dbContext = dbContext;
            _logger = logger;
        }

        public async Task<List<Category>> GetAllCategoriesAsync()
        {
            return await _dbContext.Categories
                .OrderBy(c => c.Name)
                .ToListAsync();
        }

        public async Task<List<Category>> GetActiveCategoriesAsync()
        {
            return await _dbContext.Categories
                .Where(c => c.IsActive)
                .OrderBy(c => c.Name)
                .ToListAsync();
        }

        public async Task<Category?> GetByIdAsync(int id)
        {
            return await _dbContext.Categories.FindAsync(id);
        }

        public async Task<Category> CreateCategoryAsync(string name, string? description)
        {
            var category = new Category
            {
                Name = name,
                Description = description,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };

            _dbContext.Categories.Add(category);
            await _dbContext.SaveChangesAsync();

            _logger.LogInformation("Created category {CategoryName}", name);
            return category;
        }

        public async Task UpdateCategoryAsync(int id, string name, string? description, bool isActive)
        {
            var category = await _dbContext.Categories.FindAsync(id);
            if (category is null)
            {
                _logger.LogWarning("Category {CategoryId} not found for update", id);
                return;
            }

            category.Name = name;
            category.Description = description;
            category.IsActive = isActive;

            await _dbContext.SaveChangesAsync();
            _logger.LogInformation("Updated category {CategoryId}", id);
        }
    }
}
