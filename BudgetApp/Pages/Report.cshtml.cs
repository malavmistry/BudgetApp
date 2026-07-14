using System.Collections.Generic;
using System.Text.Json;
using System.Threading.Tasks;
using BudgetApp.Constants;
using BudgetApp.Models;
using BudgetApp.Services;
using BudgetApp.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;

namespace BudgetApp.Pages
{
    public class ReportModel : PageModel
    {
        private readonly IReportService _reportService;
        private readonly IBudgetService _budgetService;
        private readonly ICategoryService _categoryService;
        private readonly ILogger<ReportModel> _logger;

        public ReportModel(
            IReportService reportService,
            IBudgetService budgetService,
            ICategoryService categoryService,
            ILogger<ReportModel> logger)
        {
            _reportService = reportService;
            _budgetService = budgetService;
            _categoryService = categoryService;
            _logger = logger;
        }

        public List<Budget> Budgets { get; private set; } = new();

        public List<Category> Categories { get; private set; } = new();

        [BindProperty]
        public ReportViewModel Filter { get; set; } = new();

        public ReportResultViewModel? Result { get; private set; }

        public string ExpensePieJson { get; private set; } = "[]";

        public string EarningsPieJson { get; private set; } = "[]";

        public async Task<IActionResult> OnGetAsync()
        {
            var userId = HttpContext.Session.GetInt32(SessionKeys.LOGGED_IN_USER_ID);
            if (!userId.HasValue)
                return RedirectToPage("/Login");

            await LoadFilterOptionsAsync(userId.Value);
            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            var userId = HttpContext.Session.GetInt32(SessionKeys.LOGGED_IN_USER_ID);
            if (!userId.HasValue)
                return RedirectToPage("/Login");

            await LoadFilterOptionsAsync(userId.Value);

            var timeZoneId = Request.Cookies["userTimeZone"] ?? "UTC";
            Result = await _reportService.GenerateReportAsync(Filter, userId.Value, timeZoneId);
            ExpensePieJson = JsonSerializer.Serialize(Result.ExpensePie);
            EarningsPieJson = JsonSerializer.Serialize(Result.EarningsPie);

            return Page();
        }

        public async Task<IActionResult> OnPostExportAsync()
        {
            var userId = HttpContext.Session.GetInt32(SessionKeys.LOGGED_IN_USER_ID);
            if (!userId.HasValue)
                return RedirectToPage("/Login");

            var timeZoneId = Request.Cookies["userTimeZone"] ?? "UTC";
            var result = await _reportService.GenerateReportAsync(Filter, userId.Value, timeZoneId);
            var bytes = await _reportService.ExportToExcelAsync(result);

            return File(bytes,
                "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                "BudgetReport.xlsx");
        }

        private async Task LoadFilterOptionsAsync(int userId)
        {
            Budgets = await _budgetService.GetUserBudgetsAsync(userId);
            Categories = await _categoryService.GetAllCategoriesAsync();
        }
    }
}
