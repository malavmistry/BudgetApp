using System;
using System.Collections.Generic;
using BudgetApp.Enums;

namespace BudgetApp.Models
{
    public class RecurringItem
    {
        public int Id { get; set; }

        public int UserId { get; set; }

        public TransactionType Type { get; set; }

        public int ItemNameId { get; set; }

        public int CategoryId { get; set; }

        public decimal Amount { get; set; }

        public string? Note { get; set; }

        public int DayOfMonth { get; set; }

        public bool IsActive { get; set; } = true;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        public User User { get; set; } = null!;

        public ItemName ItemName { get; set; } = null!;

        public Category Category { get; set; } = null!;

        public ICollection<BudgetItem> BudgetItems { get; set; } = new List<BudgetItem>();
    }
}