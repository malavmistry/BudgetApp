using System.ComponentModel.DataAnnotations;

namespace BudgetApp.ViewModels
{
    public class LoginViewModel
    {
        [Required]
        [MaxLength(25)]
        public string Username { get; set; } = string.Empty;
    }
}
