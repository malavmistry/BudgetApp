using System;
using System.Collections.Generic;

namespace BudgetApp.Models
{
    public class User
    {
        public int Id { get; set; }

        public string Username { get; set; } = string.Empty;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public ICollection<Budget> Budgets { get; set; } = new List<Budget>();
    }
}
