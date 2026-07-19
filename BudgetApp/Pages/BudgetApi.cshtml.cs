using System.Collections.Generic;
using System.Globalization;
using System.Text.Json;
using System.Threading.Tasks;
using BudgetApp.Constants;
using BudgetApp.Data;
using BudgetApp.Services;
using BudgetApp.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace BudgetApp.Pages
{
    public class BudgetApiModel : PageModel
    {
        private readonly IBudgetService _budgetService;
        private readonly ICategoryService _categoryService;
        private readonly AppDbContext _dbContext;
        private readonly ILogger<BudgetApiModel> _logger;

        public BudgetApiModel(
            IBudgetService budgetService,
            ICategoryService categoryService,
            AppDbContext dbContext,
            ILogger<BudgetApiModel> logger)
        {
            _budgetService = budgetService;
            _categoryService = categoryService;
            _dbContext = dbContext;
            _logger = logger;
        }

        private int? CurrentUserId =>
            HttpContext.Session.GetInt32(SessionKeys.LOGGED_IN_USER_ID);

        public IActionResult OnGet() => RedirectToPage("/Index");

        public async Task<IActionResult> OnGetListAsync()
        {
            if (!CurrentUserId.HasValue)
                return Unauthorized();

            var budgets = await _budgetService.GetUserBudgetsAsync(CurrentUserId.Value);
            var result = budgets.Select(b => new
            {
                b.Id,
                b.Name,
                b.IsTimeBound,
                b.Month,
                b.Year
            });

            return new JsonResult(result);
        }

        public async Task<IActionResult> OnGetDetailAsync(int budgetId)
        {
            if (!CurrentUserId.HasValue)
                return Unauthorized();

            var timeZoneId = Request.Cookies["userTimeZone"] ?? "UTC";
            var vm = await _budgetService.BuildBudgetViewModelAsync(budgetId, CurrentUserId.Value, timeZoneId);

            return new JsonResult(vm, new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            });
        }

        public async Task<IActionResult> OnPostSaveItemAsync([FromBody] BudgetItemViewModel model)
        {
            if (!CurrentUserId.HasValue)
                return Unauthorized();

            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                var timeZoneId = Request.Cookies["userTimeZone"] ?? "UTC";
                var item = await _budgetService.SaveBudgetItemAsync(model, CurrentUserId.Value, timeZoneId);
                return new JsonResult(new { success = true, id = item.Id });
            }
            catch (ArgumentException ex)
            {
                return new JsonResult(new { success = false, error = ex.Message });
            }
        }

        public async Task<IActionResult> OnPostDeleteItemAsync([FromBody] DeleteRequest request)
        {
            if (!CurrentUserId.HasValue)
                return Unauthorized();

            await _budgetService.DeleteBudgetItemAsync(request.Id);
            return new JsonResult(new { success = true });
        }

        public async Task<IActionResult> OnPostCreateBudgetAsync([FromBody] CreateBudgetRequest request)
        {
            if (!CurrentUserId.HasValue)
                return Unauthorized();

            var existingBudgets = await _budgetService.GetUserBudgetsAsync(CurrentUserId.Value);
            if (existingBudgets.Any(b => b.Name.Equals(request.Name, StringComparison.OrdinalIgnoreCase)))
                return new JsonResult(new { success = false, error = $"Budget '{request.Name}' already exists." });

            var budget = await _budgetService.CreateBudgetAsync(
                request.Name, request.IsTimeBound, request.Month, request.Year, CurrentUserId.Value);

            return new JsonResult(new { success = true, id = budget.Id, name = budget.Name });
        }

        public async Task<IActionResult> OnPostRenameBudgetAsync([FromBody] RenameBudgetRequest request)
        {
            if (!CurrentUserId.HasValue)
                return Unauthorized();

            var (success, error) = await _budgetService.RenameBudgetAsync(
                request.Id, request.Name, CurrentUserId.Value);

            return success
                ? new JsonResult(new { success = true })
                : new JsonResult(new { success = false, error });
        }

        public async Task<IActionResult> OnGetCategoriesAsync()
        {
            if (!CurrentUserId.HasValue)
                return Unauthorized();

            var categories = await _categoryService.GetActiveCategoriesAsync();
            return new JsonResult(categories.Select(c => new { c.Id, c.Name }));
        }

        public async Task<IActionResult> OnGetAllBudgetsAsync()
        {
            if (!CurrentUserId.HasValue)
                return Unauthorized();

            var budgets = await _budgetService.GetUserBudgetsAsync(CurrentUserId.Value);
            return new JsonResult(budgets.Select(b => new { b.Id, b.Name }));
        }

        public async Task<IActionResult> OnGetResolveBudgetByDateAsync(string date)
        {
            if (!CurrentUserId.HasValue)
                return Unauthorized();

            if (!DateTime.TryParseExact(date, AppConstants.DATE_FORMAT, CultureInfo.InvariantCulture,
                    DateTimeStyles.None, out var localDate))
                return BadRequest(new { error = "Invalid date format." });

            var timeZoneId = Request.Cookies["userTimeZone"] ?? "UTC";
            var timeZone = TimeZoneInfo.FindSystemTimeZoneById(timeZoneId);
            var utcDate = TimeZoneInfo.ConvertTimeToUtc(localDate, timeZone);
            var twoDigitYear = utcDate.Year % 100;

            var budget = await _budgetService.EnsureTimeBoundBudgetAsync(utcDate.Month, twoDigitYear, CurrentUserId.Value);
            return new JsonResult(new { id = budget.Id, name = budget.Name });
        }
    }

    public class DeleteRequest
    {
        public int Id { get; set; }
    }

    public class CreateBudgetRequest
    {
        public string Name { get; set; } = string.Empty;
        public bool IsTimeBound { get; set; }
        public int? Month { get; set; }
        public int? Year { get; set; }
    }

    public class RenameBudgetRequest
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
    }
}
