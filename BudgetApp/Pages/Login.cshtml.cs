using System.Collections.Generic;
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
    public class LoginModel : PageModel
    {
        private readonly IUserService _userService;
        private readonly ILogger<LoginModel> _logger;

        public LoginModel(IUserService userService, ILogger<LoginModel> logger)
        {
            _userService = userService;
            _logger = logger;
        }

        [BindProperty]
        public LoginViewModel Input { get; set; } = new();

        public List<User> AllUsers { get; set; } = new();

        public string ErrorMessage { get; set; } = string.Empty;

        public string CreateMessage { get; set; } = string.Empty;

        public async Task<IActionResult> OnGetAsync(string? handler)
        {
            if (handler == "Logout")
            {
                HttpContext.Session.Clear();
                _logger.LogInformation("User logged out");
                return RedirectToPage("/Login");
            }

            if (HttpContext.Session.GetInt32(SessionKeys.LOGGED_IN_USER_ID).HasValue)
                return RedirectToPage("/Index");

            AllUsers = await _userService.GetAllUsersAsync();
            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            AllUsers = await _userService.GetAllUsersAsync();

            if (!ModelState.IsValid)
                return Page();

            var username = Input.Username?.Trim() ?? string.Empty;
            if (string.IsNullOrWhiteSpace(username))
            {
                ErrorMessage = "Please enter a username.";
                return Page();
            }

            var user = await _userService.FindByUsernameAsync(username);
            if (user is null)
            {
                ErrorMessage = $"Username '{Input.Username}' not found. Please create an account.";
                return Page();
            }

            HttpContext.Session.SetInt32(SessionKeys.LOGGED_IN_USER_ID, user.Id);
            HttpContext.Session.SetString(SessionKeys.LOGGED_IN_USERNAME, user.Username);

            _logger.LogInformation("User {Username} logged in", user.Username);
            return RedirectToPage("/Index");
        }

        public async Task<IActionResult> OnPostCreateUserAsync(string newUsername)
        {
            AllUsers = await _userService.GetAllUsersAsync();

            if (string.IsNullOrWhiteSpace(newUsername))
            {
                ErrorMessage = "Username cannot be empty.";
                return Page();
            }

            var trimmed = newUsername.Trim();
            var existing = await _userService.FindByUsernameAsync(trimmed);
            if (existing is not null)
            {
                ErrorMessage = $"Username '{trimmed}' is already taken.";
                return Page();
            }

            await _userService.CreateUserAsync(trimmed);
            CreateMessage = $"User '{trimmed}' created. You can now log in.";
            Input.Username = trimmed;
            AllUsers = await _userService.GetAllUsersAsync();
            return Page();
        }
    }
}
