using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using BudgetApp.Models;
using BudgetApp.ViewModels;

namespace BudgetApp.Services
{
    public interface IBudgetService
    {
        Task<List<Budget>> GetUserBudgetsAsync(int userId);

        Task<Budget?> GetBudgetDetailAsync(int budgetId, int userId);

        Task<Budget> CreateBudgetAsync(string name, bool isTimeBound, int? month, int? year, int userId);

        Task<Budget> EnsureTimeBoundBudgetAsync(int month, int year, int userId);

        Task<BudgetItem> SaveBudgetItemAsync(BudgetItemViewModel viewModel, int userId, string userTimeZoneId);

        Task DeleteBudgetItemAsync(int itemId);

        Task<HomeViewModel> BuildHomeViewModelAsync(int userId, string userTimeZoneId);

        Task<BudgetViewModel> BuildBudgetViewModelAsync(int budgetId, int userId, string userTimeZoneId);

        Task<(bool Success, string? Error)> RenameBudgetAsync(int budgetId, string newName, int userId);
    }
}
