using System.Threading.Tasks;
using BudgetApp.Constants;
using BudgetApp.Services;
using BudgetApp.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;

namespace BudgetApp.Pages
{
    public class CategoryApiModel : PageModel
    {
        private readonly ICategoryService _categoryService;
        private readonly ILogger<CategoryApiModel> _logger;

        public CategoryApiModel(ICategoryService categoryService, ILogger<CategoryApiModel> logger)
        {
            _categoryService = categoryService;
            _logger = logger;
        }

        private bool IsAuthenticated =>
            HttpContext.Session.GetInt32(SessionKeys.LOGGED_IN_USER_ID).HasValue;

        public IActionResult OnGet() => RedirectToPage("/Index");

        public async Task<IActionResult> OnGetListAsync()
        {
            if (!IsAuthenticated)
                return Unauthorized();

            var categories = await _categoryService.GetAllCategoriesAsync();
            return new JsonResult(categories.Select(c => new
            {
                c.Id,
                c.Name,
                c.Description,
                c.IsActive
            }));
        }

        public async Task<IActionResult> OnPostSaveAsync([FromBody] CategoryViewModel model)
        {
            if (!IsAuthenticated)
                return Unauthorized();

            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            if (model.Id == 0)
            {
                var created = await _categoryService.CreateCategoryAsync(model.Name, model.Description);
                return new JsonResult(new { success = true, id = created.Id });
            }

            await _categoryService.UpdateCategoryAsync(model.Id, model.Name, model.Description, model.IsActive);
            return new JsonResult(new { success = true, id = model.Id });
        }
    }
}
