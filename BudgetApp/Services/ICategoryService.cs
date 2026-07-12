using System.Collections.Generic;
using System.Threading.Tasks;
using BudgetApp.Models;

namespace BudgetApp.Services
{
    public interface ICategoryService
    {
        Task<List<Category>> GetAllCategoriesAsync();

        Task<List<Category>> GetActiveCategoriesAsync();

        Task<Category?> GetByIdAsync(int id);

        Task<Category> CreateCategoryAsync(string name, string? description);

        Task UpdateCategoryAsync(int id, string name, string? description, bool isActive);
    }
}
