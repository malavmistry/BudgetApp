using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BudgetApp.Data;
using BudgetApp.Models;
using BudgetApp.ViewModels;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace BudgetApp.Services
{
    public class RecurringItemService : IRecurringItemService
    {
        private readonly AppDbContext _dbContext;
        private readonly IItemNameService _itemNameService;
        private readonly ILogger<RecurringItemService> _logger;

        public RecurringItemService(
            AppDbContext dbContext,
            IItemNameService itemNameService,
            ILogger<RecurringItemService> logger)
        {
            _dbContext = dbContext;
            _itemNameService = itemNameService;
            _logger = logger;
        }

        public async Task<List<RecurringItemViewModel>> GetUserRecurringItemsAsync(int userId)
        {
            return await _dbContext.RecurringItems
                .Include(r => r.ItemName)
                .Include(r => r.Category)
                .Where(r => r.UserId == userId)
                .OrderBy(r => r.ItemName.Name)
                .Select(r => new RecurringItemViewModel
                {
                    Id = r.Id,
                    Type = r.Type,
                    ItemNameId = r.ItemNameId,
                    ItemNameText = r.ItemName.Name,
                    CategoryId = r.CategoryId,
                    CategoryName = r.Category.Name,
                    Amount = r.Amount,
                    Note = r.Note,
                    DayOfMonth = r.DayOfMonth,
                    IsActive = r.IsActive
                })
                .ToListAsync();
        }

        public async Task<RecurringItemViewModel> SaveRecurringItemAsync(RecurringItemViewModel model, int userId)
        {
            var itemName = await _itemNameService.GetOrCreateAsync(model.ItemNameText);

            RecurringItem item;

            if (model.Id == 0)
            {
                item = new RecurringItem
                {
                    UserId = userId,
                    Type = model.Type,
                    ItemNameId = itemName.Id,
                    CategoryId = model.CategoryId,
                    Amount = Math.Truncate(model.Amount * 100) / 100,
                    Note = model.Note,
                    DayOfMonth = model.DayOfMonth,
                    IsActive = model.IsActive,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };
                _dbContext.RecurringItems.Add(item);
            }
            else
            {
                item = await _dbContext.RecurringItems
                    .FirstAsync(r => r.Id == model.Id && r.UserId == userId);

                item.Type = model.Type;
                item.ItemNameId = itemName.Id;
                item.CategoryId = model.CategoryId;
                item.Amount = Math.Truncate(model.Amount * 100) / 100;
                item.Note = model.Note;
                item.DayOfMonth = model.DayOfMonth;
                item.IsActive = model.IsActive;
                item.UpdatedAt = DateTime.UtcNow;
            }

            await _dbContext.SaveChangesAsync();

            _logger.LogInformation("Saved recurring item {Id} for user {UserId}", item.Id, userId);

            model.Id = item.Id;
            model.ItemNameId = itemName.Id;
            return model;
        }

        public async Task DeleteRecurringItemAsync(int id, int userId)
        {
            var item = await _dbContext.RecurringItems
                .FirstOrDefaultAsync(r => r.Id == id && r.UserId == userId);

            if (item is null) return;

            _dbContext.RecurringItems.Remove(item);
            await _dbContext.SaveChangesAsync();

            _logger.LogInformation("Deleted recurring item {Id} for user {UserId}", id, userId);
        }

        public async Task SetActiveAsync(int id, bool isActive, int userId)
        {
            var item = await _dbContext.RecurringItems
                .FirstOrDefaultAsync(r => r.Id == id && r.UserId == userId);

            if (item is null) return;

            item.IsActive = isActive;
            item.UpdatedAt = DateTime.UtcNow;
            await _dbContext.SaveChangesAsync();
        }
    }
}
