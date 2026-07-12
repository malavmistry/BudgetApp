using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using BudgetApp.Constants;
using BudgetApp.Data;
using BudgetApp.Enums;
using BudgetApp.Models;
using BudgetApp.ViewModels;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace BudgetApp.Services
{
    public class BudgetService : IBudgetService
    {
        private readonly AppDbContext _dbContext;
        private readonly IItemNameService _itemNameService;
        private readonly ILogger<BudgetService> _logger;

        public BudgetService(
            AppDbContext dbContext,
            IItemNameService itemNameService,
            ILogger<BudgetService> logger)
        {
            _dbContext = dbContext;
            _itemNameService = itemNameService;
            _logger = logger;
        }

        public async Task<List<Budget>> GetUserBudgetsAsync(int userId)
        {
            return await _dbContext.Budgets
                .Where(b => b.UserId == userId)
                .OrderByDescending(b => b.IsTimeBound)
                .ThenByDescending(b => b.Year)
                .ThenByDescending(b => b.Month)
                .ThenBy(b => b.Name)
                .ToListAsync();
        }

        public async Task<Budget?> GetBudgetDetailAsync(int budgetId, int userId)
        {
            return await _dbContext.Budgets
                .Include(b => b.BudgetItems)
                    .ThenInclude(bi => bi.ItemName)
                .Include(b => b.BudgetItems)
                    .ThenInclude(bi => bi.Category)
                .Include(b => b.BudgetItems)
                    .ThenInclude(bi => bi.AdditionalLinks)
                        .ThenInclude(l => l.LinkedBudget)
                .Include(b => b.LinkedItems)
                    .ThenInclude(l => l.BudgetItem)
                        .ThenInclude(bi => bi.ItemName)
                .Include(b => b.LinkedItems)
                    .ThenInclude(l => l.BudgetItem)
                        .ThenInclude(bi => bi.Category)
                .Include(b => b.LinkedItems)
                    .ThenInclude(l => l.BudgetItem)
                        .ThenInclude(bi => bi.Budget)
                .Include(b => b.LinkedItems)
                    .ThenInclude(l => l.BudgetItem)
                        .ThenInclude(bi => bi.AdditionalLinks)
                            .ThenInclude(l => l.LinkedBudget)
                .FirstOrDefaultAsync(b => b.Id == budgetId && b.UserId == userId);
        }

        public async Task<Budget> CreateBudgetAsync(
            string name, bool isTimeBound, int? month, int? year, int userId)
        {
            var budget = new Budget
            {
                Name = name,
                IsTimeBound = isTimeBound,
                Month = month,
                Year = year,
                UserId = userId,
                CreatedAt = DateTime.UtcNow
            };

            _dbContext.Budgets.Add(budget);
            await _dbContext.SaveChangesAsync();

            _logger.LogInformation("Created budget {BudgetName} for user {UserId}", name, userId);
            return budget;
        }

        public async Task<Budget> EnsureTimeBoundBudgetAsync(int month, int year, int userId)
        {
            var existing = await _dbContext.Budgets
                .FirstOrDefaultAsync(b =>
                    b.UserId == userId &&
                    b.IsTimeBound &&
                    b.Month == month &&
                    b.Year == year);

            if (existing is not null)
                return existing;

            var monthName = CultureInfo.CurrentCulture.DateTimeFormat.GetMonthName(month);
            var twoDigitYear = year % 100;
            var budgetName = $"{monthName} {twoDigitYear:D2}";

            _logger.LogInformation(
                "Auto-creating time-bound budget {BudgetName} for user {UserId}", budgetName, userId);

            var newBudget = await CreateBudgetAsync(budgetName, true, month, twoDigitYear, userId);
            await CopyRecurringItemsAsync(newBudget, userId);
            return newBudget;
        }

        public async Task<BudgetItem> SaveBudgetItemAsync(
            BudgetItemViewModel viewModel, int userId, string userTimeZoneId)
        {
            var itemName = await _itemNameService.GetOrCreateAsync(viewModel.ItemNameText);

            var localDate = DateTime.ParseExact(
                viewModel.TransactionDate,
                AppConstants.DATE_FORMAT,
                CultureInfo.InvariantCulture);

            // Validate local date against budget month/year for time-bound budgets
            var primaryBudget = await _dbContext.Budgets.FindAsync(viewModel.BudgetId);
            if (primaryBudget is { IsTimeBound: true } &&
                primaryBudget.Month.HasValue && primaryBudget.Year.HasValue)
            {
                var budgetFullYear = 2000 + primaryBudget.Year.Value;
                if (localDate.Month != primaryBudget.Month.Value || localDate.Year != budgetFullYear)
                {
                    var mName = CultureInfo.CurrentCulture.DateTimeFormat
                                           .GetMonthName(primaryBudget.Month.Value);
                    throw new ArgumentException(
                        $"Transaction date must be within {mName} {budgetFullYear} for this budget.");
                }
            }

            var timeZone = GetTimeZone(userTimeZoneId);
            var utcDate = TimeZoneInfo.ConvertTimeToUtc(localDate, timeZone);

            var timeBoundBudget = await EnsureTimeBoundBudgetAsync(
                utcDate.Month, utcDate.Year % 100 == 0 ? utcDate.Year / 100 : utcDate.Year % 100, userId);

            BudgetItem item;

            if (viewModel.Id == 0)
            {
                item = new BudgetItem
                {
                    BudgetId = viewModel.BudgetId,
                    Type = viewModel.Type,
                    ItemNameId = itemName.Id,
                    CategoryId = viewModel.CategoryId,
                    Amount = Math.Truncate(viewModel.Amount * 100) / 100,
                    TransactionDateUtc = utcDate,
                    Note = viewModel.Note,
                    IsRecurring = viewModel.IsRecurring,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };

                _dbContext.BudgetItems.Add(item);
                await _dbContext.SaveChangesAsync();
            }
            else
            {
                item = await _dbContext.BudgetItems
                    .Include(bi => bi.AdditionalLinks)
                    .FirstAsync(bi => bi.Id == viewModel.Id);

                item.Type = viewModel.Type;
                item.ItemNameId = itemName.Id;
                item.CategoryId = viewModel.CategoryId;
                item.Amount = Math.Truncate(viewModel.Amount * 100) / 100;
                item.TransactionDateUtc = utcDate;
                item.Note = viewModel.Note;
                item.IsRecurring = viewModel.IsRecurring;
                item.UpdatedAt = DateTime.UtcNow;

                _dbContext.BudgetItemLinks.RemoveRange(item.AdditionalLinks);
                await _dbContext.SaveChangesAsync();
            }

            // Build a deduplicated set of desired linked budget IDs.
            // Using HashSet eliminates duplicates when auto-link == a user-selected link.
            var desiredLinks = new HashSet<int>(
                viewModel.LinkedBudgetIds.Where(id => id != item.BudgetId));

            if (item.BudgetId != timeBoundBudget.Id)
                desiredLinks.Add(timeBoundBudget.Id);

            foreach (var linkedBudgetId in desiredLinks)
            {
                _dbContext.BudgetItemLinks.Add(new BudgetItemLink
                {
                    BudgetItemId = item.Id,
                    LinkedBudgetId = linkedBudgetId
                });
            }

            await _dbContext.SaveChangesAsync();
            return item;
        }

        public async Task DeleteBudgetItemAsync(int itemId)
        {
            var item = await _dbContext.BudgetItems.FindAsync(itemId);
            if (item is null)
                return;

            _dbContext.BudgetItems.Remove(item);
            await _dbContext.SaveChangesAsync();

            _logger.LogInformation("Deleted budget item {ItemId}", itemId);
        }

        public async Task<(bool Success, string? Error)> RenameBudgetAsync(int budgetId, string newName, int userId)
        {
            var budget = await _dbContext.Budgets
                .FirstOrDefaultAsync(b => b.Id == budgetId && b.UserId == userId);

            if (budget is null)
                return (false, "Budget not found.");

            var nameExists = await _dbContext.Budgets
                .AnyAsync(b => b.UserId == userId && b.Name == newName && b.Id != budgetId);

            if (nameExists)
                return (false, $"Budget '{newName}' already exists.");

            budget.Name = newName;
            await _dbContext.SaveChangesAsync();

            _logger.LogInformation("Renamed budget {BudgetId} to {NewName}", budgetId, newName);
            return (true, null);
        }

        public async Task<HomeViewModel> BuildHomeViewModelAsync(int userId, string userTimeZoneId)
        {
            var timeZone = GetTimeZone(userTimeZoneId);
            var now = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, timeZone);
            var currentYear = now.Year;
            var currentMonth = now.Month;
            var ytdStart = new DateTime(currentYear, 1, 1, 0, 0, 0, DateTimeKind.Utc);
            var monthStart = new DateTime(currentYear, currentMonth, 1, 0, 0, 0, DateTimeKind.Utc);
            var monthEnd = monthStart.AddMonths(1);

            var user = await _dbContext.Users.FindAsync(userId);

            var allItems = await _dbContext.BudgetItems
                .Include(bi => bi.Budget)
                .Include(bi => bi.ItemName)
                .Include(bi => bi.Category)
                .Include(bi => bi.AdditionalLinks)
                .Where(bi => bi.Budget.UserId == userId &&
                             bi.TransactionDateUtc >= ytdStart)
                .ToListAsync();

            var ytdEarnings = allItems
                .Where(bi => bi.Type == TransactionType.Earnings)
                .Sum(bi => bi.Amount);

            var ytdExpense = allItems
                .Where(bi => bi.Type == TransactionType.Expense)
                .Sum(bi => bi.Amount);

            var monthItems = allItems
                .Where(bi => bi.TransactionDateUtc >= monthStart && bi.TransactionDateUtc < monthEnd)
                .ToList();

            var monthEarnings = monthItems
                .Where(bi => bi.Type == TransactionType.Earnings)
                .Sum(bi => bi.Amount);

            var monthExpense = monthItems
                .Where(bi => bi.Type == TransactionType.Expense)
                .Sum(bi => bi.Amount);

            var expensePie = monthItems
                .Where(bi => bi.Type == TransactionType.Expense)
                .GroupBy(bi => bi.Category.Name)
                .Select(g => new PieSliceViewModel
                {
                    Label = g.Key,
                    Value = g.Sum(bi => bi.Amount)
                })
                .OrderByDescending(p => p.Value)
                .ToList();

            var budgets = await GetUserBudgetsAsync(userId);
            var budgetSummaries = new List<BudgetSummaryViewModel>();

            foreach (var budget in budgets)
            {
                var budgetItems = await _dbContext.BudgetItems
                    .Where(bi => bi.BudgetId == budget.Id)
                    .ToListAsync();

                var linkedItems = await _dbContext.BudgetItemLinks
                    .Include(l => l.BudgetItem)
                    .Where(l => l.LinkedBudgetId == budget.Id)
                    .Select(l => l.BudgetItem)
                    .ToListAsync();

                var allBudgetItems = budgetItems.Concat(linkedItems).ToList();

                budgetSummaries.Add(new BudgetSummaryViewModel
                {
                    Id = budget.Id,
                    Name = budget.Name,
                    IsTimeBound = budget.IsTimeBound,
                    TotalEarnings = allBudgetItems
                        .Where(bi => bi.Type == TransactionType.Earnings)
                        .Sum(bi => bi.Amount),
                    TotalExpense = allBudgetItems
                        .Where(bi => bi.Type == TransactionType.Expense)
                        .Sum(bi => bi.Amount)
                });
            }

            var monthName = CultureInfo.CurrentCulture.DateTimeFormat.GetMonthName(currentMonth);

            return new HomeViewModel
            {
                Greeting = GetGreeting(now.Hour),
                Username = user?.Username ?? string.Empty,
                YtdEarnings = ytdEarnings,
                YtdExpense = ytdExpense,
                CurrentMonthEarnings = monthEarnings,
                CurrentMonthExpense = monthExpense,
                CurrentMonthLabel = $"{monthName} {currentYear}",
                Budgets = budgetSummaries,
                CurrentMonthExpensePie = expensePie
            };
        }

        public async Task<BudgetViewModel> BuildBudgetViewModelAsync(
            int budgetId, int userId, string userTimeZoneId)
        {
            var budget = await GetBudgetDetailAsync(budgetId, userId);
            if (budget is null)
                return new BudgetViewModel();

            var timeZone = GetTimeZone(userTimeZoneId);

            var ownItems = budget.BudgetItems
                .Select(bi => MapToItemViewModel(bi, timeZone, budget.Name))
                .ToList();

            var linkedItems = budget.LinkedItems
                .Select(l => MapToItemViewModel(l.BudgetItem, timeZone, l.BudgetItem.Budget?.Name ?? string.Empty))
                .ToList();

            var allItems = ownItems.Concat(linkedItems).ToList();

            return new BudgetViewModel
            {
                Id = budget.Id,
                Name = budget.Name,
                IsTimeBound = budget.IsTimeBound,
                Month = budget.Month,
                Year = budget.Year,
                TotalEarnings = allItems
                    .Where(i => i.Type == TransactionType.Earnings)
                    .Sum(i => i.Amount),
                TotalExpense = allItems
                    .Where(i => i.Type == TransactionType.Expense)
                    .Sum(i => i.Amount),
                Items = allItems
                    .OrderByDescending(i => i.TransactionDate)
                    .ToList()
            };
        }

        private static BudgetItemViewModel MapToItemViewModel(BudgetItem bi, TimeZoneInfo timeZone, string primaryBudgetName)
        {
            var localDate = TimeZoneInfo.ConvertTimeFromUtc(bi.TransactionDateUtc, timeZone);

            return new BudgetItemViewModel
            {
                Id = bi.Id,
                BudgetId = bi.BudgetId,
                Type = bi.Type,
                ItemNameId = bi.ItemNameId,
                ItemNameText = bi.ItemName?.Name ?? string.Empty,
                CategoryId = bi.CategoryId,
                CategoryName = bi.Category?.Name ?? string.Empty,
                Amount = bi.Amount,
                TransactionDate = localDate.ToString(AppConstants.DATE_FORMAT),
                Note = bi.Note,
                PrimaryBudgetName = primaryBudgetName,
                LinkedBudgetIds = bi.AdditionalLinks.Select(l => l.LinkedBudgetId).ToList(),
                LinkedBudgetNames = bi.AdditionalLinks.Select(l => l.LinkedBudget?.Name ?? string.Empty).ToList(),
                IsRecurring = bi.IsRecurring
            };
        }

        /// <summary>
        /// Copies all unique recurring items from the user's existing budgets into
        /// the newly created time-bound budget, using the 1st of the new month as
        /// the transaction date. Deduplicates by (ItemNameId, CategoryId, Type).
        /// </summary>
        private async Task CopyRecurringItemsAsync(Budget newBudget, int userId)
        {
            if (!newBudget.Month.HasValue || !newBudget.Year.HasValue)
                return;

            var fullYear = 2000 + newBudget.Year.Value;
            var firstDayUtc = new DateTime(fullYear, newBudget.Month.Value, 1, 0, 0, 0, DateTimeKind.Utc);

            var allRecurring = await _dbContext.BudgetItems
                .Include(bi => bi.Budget)
                .Where(bi => bi.Budget.UserId == userId &&
                             bi.IsRecurring &&
                             bi.BudgetId != newBudget.Id)
                .OrderByDescending(bi => bi.UpdatedAt)
                .ToListAsync();

            var seen = new HashSet<(int ItemNameId, int CategoryId, TransactionType Type)>();

            foreach (var source in allRecurring)
            {
                var key = (source.ItemNameId, source.CategoryId, source.Type);
                if (!seen.Add(key))
                    continue;

                _dbContext.BudgetItems.Add(new BudgetItem
                {
                    BudgetId = newBudget.Id,
                    Type = source.Type,
                    ItemNameId = source.ItemNameId,
                    CategoryId = source.CategoryId,
                    Amount = source.Amount,
                    TransactionDateUtc = firstDayUtc,
                    Note = source.Note,
                    IsRecurring = true,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                });
            }

            if (seen.Count > 0)
            {
                await _dbContext.SaveChangesAsync();
                _logger.LogInformation(
                    "Copied {Count} recurring items into budget {BudgetName}",
                    seen.Count, newBudget.Name);
            }
        }

        private static string GetGreeting(int hour)
        {
            return hour switch
            {
                >= AppConstants.MORNING_HOUR_START and < AppConstants.AFTERNOON_HOUR_START
                    => AppConstants.GREETING_MORNING,
                >= AppConstants.AFTERNOON_HOUR_START and < AppConstants.EVENING_HOUR_START
                    => AppConstants.GREETING_AFTERNOON,
                _ => AppConstants.GREETING_EVENING
            };
        }

        private static TimeZoneInfo GetTimeZone(string timeZoneId)
        {
            try
            {
                return TimeZoneInfo.FindSystemTimeZoneById(timeZoneId);
            }
            catch
            {
                return TimeZoneInfo.Utc;
            }
        }
    }
}
