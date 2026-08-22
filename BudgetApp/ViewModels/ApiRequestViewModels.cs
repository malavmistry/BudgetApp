namespace BudgetApp.ViewModels
{
    public class DeleteRequest
    {
        public int Id { get; set; }
    }

    public class CreateBudgetRequest
    {
        public string Name { get; set; } = string.Empty;
        public bool IsTimeBound { get; set; }
        public int? Month { get; set; }
        public int? Year { get; set; }
    }

    public class RenameBudgetRequest
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
    }

    public class DeleteRecurringRequest
    {
        public int Id { get; set; }
    }

    public class SetActiveRequest
    {
        public int Id { get; set; }
        public bool IsActive { get; set; }
    }
}