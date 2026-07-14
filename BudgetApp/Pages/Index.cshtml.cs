using System.Text.Json;
using System.Threading.Tasks;
using BudgetApp.Constants;
using BudgetApp.Services;
using BudgetApp.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;

namespace BudgetApp.Pages
{
    public class IndexModel : PageModel
    {
        private readonly IBudgetService _budgetService;
        private readonly ILogger<IndexModel> _logger;

        public IndexModel(IBudgetService budgetService, ILogger<IndexModel> logger)
        {
            _budgetService = budgetService;
            _logger = logger;
        }

        public HomeViewModel HomeData { get; private set; } = new();

        public string PieChartJson { get; private set; } = "[]";

        public async Task<IActionResult> OnGetAsync()
        {
            var userId = HttpContext.Session.GetInt32(SessionKeys.LOGGED_IN_USER_ID);
            if (!userId.HasValue)
                return RedirectToPage("/Login");

            var timeZoneId = Request.Cookies["userTimeZone"] ?? "UTC";
            HomeData = await _budgetService.BuildHomeViewModelAsync(userId.Value, timeZoneId);
            PieChartJson = JsonSerializer.Serialize(HomeData.CurrentMonthExpensePie);

            return Page();
        }
    }
}
