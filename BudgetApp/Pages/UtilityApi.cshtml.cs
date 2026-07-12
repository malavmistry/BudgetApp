using System.Threading.Tasks;
using BudgetApp.Constants;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;

namespace BudgetApp.Pages
{
    public class UtilityApiModel : PageModel
    {
        private readonly ILogger<UtilityApiModel> _logger;

        public UtilityApiModel(ILogger<UtilityApiModel> logger)
        {
            _logger = logger;
        }

        public IActionResult OnGet() => RedirectToPage("/Index");

        /// <summary>
        /// Placeholder import handler. File structure TBD — returns accepted status.
        /// </summary>
        public async Task<IActionResult> OnPostImportAsync(IFormFile file)
        {
            if (!HttpContext.Session.GetInt32(SessionKeys.LOGGED_IN_USER_ID).HasValue)
                return Unauthorized();

            if (file is null || file.Length == 0)
                return BadRequest(new { error = "No file provided." });

            _logger.LogInformation(
                "Import file received: {FileName}, size: {Size} bytes", file.FileName, file.Length);

            // File processing logic will be implemented once file structure is provided
            await Task.CompletedTask;

            return new JsonResult(new
            {
                success = true,
                message = $"File '{file.FileName}' received ({file.Length} bytes). Processing not yet configured — file structure pending."
            });
        }
    }
}
