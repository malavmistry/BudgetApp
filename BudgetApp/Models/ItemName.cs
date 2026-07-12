using System;
using System.Collections.Generic;

namespace BudgetApp.Models
{
    /// <summary>
    /// Stores previously entered transaction names for quick autocomplete selection.
    /// Replaces manual re-entry — names auto-save on first use.
    /// </summary>
    public class ItemName
    {
        public int Id { get; set; }

        public string Name { get; set; } = string.Empty;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public ICollection<BudgetItem> BudgetItems { get; set; } = new List<BudgetItem>();
    }
}
