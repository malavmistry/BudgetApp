using System;
using System.Collections.Generic;
using BudgetApp.Enums;

namespace BudgetApp.ViewModels
{
    public class ReportViewModel
    {
        public ReportPeriod Period { get; set; } = ReportPeriod.ThisMonth;

        public DateTime? CustomStart { get; set; }

        public DateTime? CustomEnd { get; set; }

        public int? BudgetId { get; set; }

        public int? CategoryId { get; set; }

        public TransactionType? Type { get; set; }
    }

    public class ReportResultViewModel
    {
        public List<PieSliceViewModel> ExpensePie { get; set; } = new();

        public List<PieSliceViewModel> EarningsPie { get; set; } = new();

        public List<ReportRowViewModel> Rows { get; set; } = new();

        public decimal TotalEarnings { get; set; }

        public decimal TotalExpense { get; set; }

        public decimal Net => TotalEarnings - TotalExpense;

        public string PeriodLabel { get; set; } = string.Empty;

        /// <summary>"Name" when grouped by item name, "Category" otherwise.</summary>
        public string PieGroupLabel { get; set; } = "Category";
    }

    public class ReportRowViewModel
    {
        public string ItemName { get; set; } = string.Empty;

        public string Category { get; set; } = string.Empty;

        public string Type { get; set; } = string.Empty;

        public decimal Amount { get; set; }

        public string Date { get; set; } = string.Empty;

        public string? Note { get; set; }

        public string Budget { get; set; } = string.Empty;
    }
}
