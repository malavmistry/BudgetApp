using System.ComponentModel.DataAnnotations;
using BudgetApp.Enums;

namespace BudgetApp.ViewModels
{
    public class RecurringItemViewModel
    {
        public int Id { get; set; }

        [Required]
        public TransactionType Type { get; set; }

        [Required]
        public int ItemNameId { get; set; }

        public string ItemNameText { get; set; } = string.Empty;

        [Required]
        public int CategoryId { get; set; }

        public string CategoryName { get; set; } = string.Empty;

        [Required]
        [Range(0.01, double.MaxValue)]
        public decimal Amount { get; set; }

        [MaxLength(500)]
        public string? Note { get; set; }

        /// <summary>Day of month (1–31) to use when auto-copying into new monthly budgets.</summary>
        [Required]
        [Range(1, 31)]
        public int DayOfMonth { get; set; }

        public bool IsActive { get; set; } = true;
    }
}
