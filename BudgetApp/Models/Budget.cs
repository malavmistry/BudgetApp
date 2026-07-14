using System;
using System.Collections.Generic;

namespace BudgetApp.Models
{
    public class Budget
    {
        public int Id { get; set; }

        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// True = time-bound monthly budget (e.g. "January 26").
        /// False = custom named envelope (e.g. "Home Expenses").
        /// </summary>
        public bool IsTimeBound { get; set; }

        /// <summary>
        /// Only set when IsTimeBound = true. 1–12.
        /// </summary>
        public int? Month { get; set; }

        /// <summary>
        /// Only set when IsTimeBound = true. Two-digit year (e.g. 26 for 2026).
        /// </summary>
        public int? Year { get; set; }

        public int UserId { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public User User { get; set; } = null!;

        public ICollection<BudgetItem> BudgetItems { get; set; } = new List<BudgetItem>();

        public ICollection<BudgetItemLink> LinkedItems { get; set; } = new List<BudgetItemLink>();
    }
}
