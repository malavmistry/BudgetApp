using System.Threading.Tasks;
using BudgetApp.Constants;
using BudgetApp.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;

namespace BudgetApp.Pages
{
    public class ItemNameApiModel : PageModel
    {
        private readonly IItemNameService _itemNameService;
        private readonly ILogger<ItemNameApiModel> _logger;

        public ItemNameApiModel(IItemNameService itemNameService, ILogger<ItemNameApiModel> logger)
        {
            _itemNameService = itemNameService;
            _logger = logger;
        }

        public IActionResult OnGet() => RedirectToPage("/Index");

        public async Task<IActionResult> OnGetSearchAsync(string q)
        {
            if (!HttpContext.Session.GetInt32(SessionKeys.LOGGED_IN_USER_ID).HasValue)
                return Unauthorized();

            if (string.IsNullOrWhiteSpace(q))
                return new JsonResult(new object[] { });

            var results = await _itemNameService.SearchAsync(q.Trim());
            return new JsonResult(results.Select(n => new { n.Id, n.Name }));
        }
    }
}
