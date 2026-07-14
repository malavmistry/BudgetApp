using System.ComponentModel.DataAnnotations;

namespace BudgetApp.ViewModels
{
    public class CategoryViewModel
    {
        public int Id { get; set; }

        [Required]
        [MaxLength(25)]
        public string Name { get; set; } = string.Empty;

        [MaxLength(200)]
        public string? Description { get; set; }

        public bool IsActive { get; set; } = true;
    }
}
