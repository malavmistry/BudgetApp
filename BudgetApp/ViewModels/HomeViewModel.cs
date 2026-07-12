using System;
using System.Collections.Generic;

namespace BudgetApp.ViewModels
{
    public class HomeViewModel
    {
        public string Greeting { get; set; } = string.Empty;

        public string Username { get; set; } = string.Empty;

        public decimal YtdEarnings { get; set; }

        public decimal YtdExpense { get; set; }

        public decimal YtdNet => YtdEarnings - YtdExpense;

        public decimal CurrentMonthEarnings { get; set; }

        public decimal CurrentMonthExpense { get; set; }

        public decimal CurrentMonthNet => CurrentMonthEarnings - CurrentMonthExpense;

        public string CurrentMonthLabel { get; set; } = string.Empty;

        public List<BudgetSummaryViewModel> Budgets { get; set; } = new();

        public List<PieSliceViewModel> CurrentMonthExpensePie { get; set; } = new();
    }

    public class BudgetSummaryViewModel
    {
        public int Id { get; set; }

        public string Name { get; set; } = string.Empty;

        public bool IsTimeBound { get; set; }

        public decimal TotalEarnings { get; set; }

        public decimal TotalExpense { get; set; }

        public decimal Net => TotalEarnings - TotalExpense;
    }

    public class PieSliceViewModel
    {
        public string Label { get; set; } = string.Empty;

        public decimal Value { get; set; }
    }
}
