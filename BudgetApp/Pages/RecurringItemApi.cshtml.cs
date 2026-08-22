using System;
using System.Linq;
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
        private readonly ICategoryService _categoryService;
        private readonly ILogger<RecurringItemApiModel> _logger;

        public RecurringItemApiModel(
            IRecurringItemService recurringItemService,
            ICategoryService categoryService,
            ILogger<RecurringItemApiModel> logger)
        {
            _recurringItemService = recurringItemService;
            _categoryService = categoryService;
            _logger = logger;
        }

        private int? CurrentUserId =>
            HttpContext.Session.GetInt32(SessionKeys.LOGGED_IN_USER_ID);

        public IActionResult OnGet() => RedirectToPage("/Index");

        public async Task<IActionResult> OnGetListAsync()
        {
            if (!CurrentUserId.HasValue) return Unauthorized();
            var items = await _recurringItemService.GetUserRecurringItemsAsync(CurrentUserId.Value);
            return new JsonResult(items);
        }

        public async Task<IActionResult> OnGetCategoriesAsync()
        {
            if (!CurrentUserId.HasValue) return Unauthorized();
            var categories = await _categoryService.GetActiveCategoriesAsync();
            return new JsonResult(categories.Select(c => new { c.Id, c.Name }));
        }

        public async Task<IActionResult> OnPostSaveAsync([FromBody] RecurringItemViewModel model)
        {
            if (!CurrentUserId.HasValue) return Unauthorized();
            if (!ModelState.IsValid) return BadRequest(ModelState);

            try
            {
                var result = await _recurringItemService.SaveRecurringItemAsync(model, CurrentUserId.Value);
                return new JsonResult(new { success = true, id = result.Id });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to save recurring item");
                return new JsonResult(new { success = false, error = ex.Message });
            }
        }

        public async Task<IActionResult> OnPostDeleteAsync([FromBody] DeleteRecurringRequest request)
        {
            if (!CurrentUserId.HasValue) return Unauthorized();
            await _recurringItemService.DeleteRecurringItemAsync(request.Id, CurrentUserId.Value);
            return new JsonResult(new { success = true });
        }

        public async Task<IActionResult> OnPostSetActiveAsync([FromBody] SetActiveRequest request)
        {
            if (!CurrentUserId.HasValue) return Unauthorized();
            await _recurringItemService.SetActiveAsync(request.Id, request.IsActive, CurrentUserId.Value);
            return new JsonResult(new { success = true });
        }
    }
}
