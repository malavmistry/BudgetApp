using System;
using System.Collections.Generic;
using BudgetApp.Enums;

namespace BudgetApp.Models
{
    public class BudgetItem
    {
        public int Id { get; set; }

        /// <summary>Primary owning budget.</summary>
        public int BudgetId { get; set; }

        public TransactionType Type { get; set; }

        public int ItemNameId { get; set; }

        public int CategoryId { get; set; }

        /// <summary>Max two decimal places, no rounding.</summary>
        public decimal Amount { get; set; }

        /// <summary>Transaction date stored in UTC.</summary>
        public DateTime TransactionDateUtc { get; set; }

        public string? Note { get; set; }

        /// <summary>
        /// When true, this item is automatically copied into every newly created
        /// time-bound (monthly) budget going forward.
        /// </summary>
        public bool IsRecurring { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        public Budget Budget { get; set; } = null!;

        public ItemName ItemName { get; set; } = null!;

        public Category Category { get; set; } = null!;

        public ICollection<BudgetItemLink> AdditionalLinks { get; set; } = new List<BudgetItemLink>();
    }
}
