using System.ComponentModel.DataAnnotations;
using BudgetApp.Enums;

namespace BudgetApp.ViewModels
{
    public class RecurringItemViewModel
    {
        public int Id { get; set; }

        [Required]
        public TransactionType Type { get; set; }

        public int ItemNameId { get; set; }

        [Required]
        [MaxLength(25)]
        public string ItemNameText { get; set; } = string.Empty;

        [Required]
        public int CategoryId { get; set; }

        public string CategoryName { get; set; } = string.Empty;

        [Required]
        [Range(0.01, double.MaxValue)]
        public decimal Amount { get; set; }

        [MaxLength(500)]
        public string? Note { get; set; }

        [Range(1, 31)]
        public int DayOfMonth { get; set; }

        public bool IsActive { get; set; } = true;
    }
}