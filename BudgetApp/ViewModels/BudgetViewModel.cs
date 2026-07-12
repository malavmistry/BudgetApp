using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using BudgetApp.Enums;

namespace BudgetApp.ViewModels
{
    public class BudgetViewModel
    {
        public int Id { get; set; }

        [Required]
        [MaxLength(25)]
        public string Name { get; set; } = string.Empty;

        public bool IsTimeBound { get; set; }

        public int? Month { get; set; }

        public int? Year { get; set; }

        public decimal TotalEarnings { get; set; }

        public decimal TotalExpense { get; set; }

        public decimal Net => TotalEarnings - TotalExpense;

        public List<BudgetItemViewModel> Items { get; set; } = new();
    }

    public class BudgetItemViewModel
    {
        public int Id { get; set; }

        public int BudgetId { get; set; }

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

        /// <summary>Displayed and entered in local timezone. Format: MM-dd-yyyy.</summary>
        [Required]
        public string TransactionDate { get; set; } = string.Empty;

        [MaxLength(500)]
        public string? Note { get; set; }

        public bool IsRecurring { get; set; }

        public List<int> LinkedBudgetIds { get; set; } = new();

        public List<string> LinkedBudgetNames { get; set; } = new();

        /// <summary>Name of the budget this item was originally created in.</summary>
        public string PrimaryBudgetName { get; set; } = string.Empty;

        /// <summary>True when item is linked to more than its primary budget.</summary>
        public bool HasAdditionalLinks => LinkedBudgetIds.Count > 0;
    }
}
