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

        public async Task<List<RecurringItemViewModel>> GetRecurringItemsAsync(int userId)
        {
            return await _dbContext.RecurringItems
                .Include(ri => ri.ItemName)
                .Include(ri => ri.Category)
                .Where(ri => ri.UserId == userId)
                .OrderBy(ri => ri.ItemName.Name)
                .ThenBy(ri => ri.DayOfMonth)
                .Select(ri => new RecurringItemViewModel
                {
                    Id = ri.Id,
                    Type = ri.Type,
                    ItemNameId = ri.ItemNameId,
                    ItemNameText = ri.ItemName.Name,
                    CategoryId = ri.CategoryId,
                    CategoryName = ri.Category.Name,
                    Amount = ri.Amount,
                    Note = ri.Note,
                    DayOfMonth = ri.DayOfMonth,
                    IsActive = ri.IsActive
                })
                .ToListAsync();
        }

        public async Task<RecurringItemViewModel> SaveRecurringItemAsync(RecurringItemViewModel model, int userId)
        {
            var itemName = await _itemNameService.GetOrCreateAsync(model.ItemNameText);
            var amount = Math.Truncate(model.Amount * 100) / 100;
            var dayOfMonth = Math.Clamp(model.DayOfMonth, 1, 31);

            RecurringItem entity;
            if (model.Id == 0)
            {
                entity = new RecurringItem
                {
                    UserId = userId,
                    Type = model.Type,
                    ItemNameId = itemName.Id,
                    CategoryId = model.CategoryId,
                    Amount = amount,
                    Note = model.Note,
                    DayOfMonth = dayOfMonth,
                    IsActive = model.IsActive,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };

                _dbContext.RecurringItems.Add(entity);
            }
            else
            {
                entity = await _dbContext.RecurringItems
                    .FirstAsync(ri => ri.Id == model.Id && ri.UserId == userId);

                entity.Type = model.Type;
                entity.ItemNameId = itemName.Id;
                entity.CategoryId = model.CategoryId;
                entity.Amount = amount;
                entity.Note = model.Note;
                entity.DayOfMonth = dayOfMonth;
                entity.IsActive = model.IsActive;
                entity.UpdatedAt = DateTime.UtcNow;
            }

            await _dbContext.SaveChangesAsync();

            _logger.LogInformation(
                "Saved recurring item {RecurringItemId} for user {UserId}",
                entity.Id,
                userId);

            model.Id = entity.Id;
            model.ItemNameId = itemName.Id;
            return model;
        }
    }
}