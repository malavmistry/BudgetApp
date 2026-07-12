using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using BudgetApp.Data;
using BudgetApp.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace BudgetApp.Services
{
    public class ItemNameService : IItemNameService
    {
        private readonly AppDbContext _dbContext;
        private readonly ILogger<ItemNameService> _logger;

        public ItemNameService(AppDbContext dbContext, ILogger<ItemNameService> logger)
        {
            _dbContext = dbContext;
            _logger = logger;
        }

        public async Task<List<ItemName>> SearchAsync(string query)
        {
            return await _dbContext.ItemNames
                .Where(n => n.Name.Contains(query))
                .OrderBy(n => n.Name)
                .Take(20)
                .ToListAsync();
        }

        public async Task<ItemName> GetOrCreateAsync(string name)
        {
            var existing = await _dbContext.ItemNames
                .FirstOrDefaultAsync(n => n.Name == name);

            if (existing is not null)
                return existing;

            var itemName = new ItemName
            {
                Name = name,
                CreatedAt = DateTime.UtcNow
            };

            _dbContext.ItemNames.Add(itemName);
            await _dbContext.SaveChangesAsync();

            _logger.LogDebug("Auto-created item name {Name}", name);
            return itemName;
        }
    }
}
