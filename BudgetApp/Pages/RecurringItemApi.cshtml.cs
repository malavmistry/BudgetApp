using System.Threading.Tasks;
using BudgetApp.Constants;
using BudgetApp.Services;
using BudgetApp.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;

namespace BudgetApp.Pages
{
    public class RecurringItemApiModel : PageModel
    {
        private readonly IRecurringItemService _recurringItemService;
        private readonly ILogger<RecurringItemApiModel> _logger;

        public RecurringItemApiModel(
            IRecurringItemService recurringItemService,
            ILogger<RecurringItemApiModel> logger)
        {
            _recurringItemService = recurringItemService;
            _logger = logger;
        }

        private int? CurrentUserId =>
            HttpContext.Session.GetInt32(SessionKeys.LOGGED_IN_USER_ID);

        public IActionResult OnGet() => RedirectToPage("/Index");

        public async Task<IActionResult> OnGetListAsync()
        {
            if (!CurrentUserId.HasValue)
                return Unauthorized();

            var recurringItems = await _recurringItemService.GetRecurringItemsAsync(CurrentUserId.Value);
            return new JsonResult(recurringItems);
        }

        public async Task<IActionResult> OnPostSaveAsync([FromBody] RecurringItemViewModel model)
        {
            if (!CurrentUserId.HasValue)
                return Unauthorized();

            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var recurringItem = await _recurringItemService.SaveRecurringItemAsync(model, CurrentUserId.Value);
            return new JsonResult(new { success = true, id = recurringItem.Id });
        }
    }
}