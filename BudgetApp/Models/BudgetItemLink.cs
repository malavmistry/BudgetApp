namespace BudgetApp.Models
{
    /// <summary>
    /// Links a budget item to additional budgets beyond its primary one.
    /// Items linked here appear in the target budget and affect its totals.
    /// </summary>
    public class BudgetItemLink
    {
        public int Id { get; set; }

        public int BudgetItemId { get; set; }

        /// <summary>The additional budget this item is linked to.</summary>
        public int LinkedBudgetId { get; set; }

        public BudgetItem BudgetItem { get; set; } = null!;

        public Budget LinkedBudget { get; set; } = null!;
    }
}
