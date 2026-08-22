using System.Collections.Generic;
using System.Threading.Tasks;
using BudgetApp.ViewModels;

namespace BudgetApp.Services
{
    public interface IRecurringItemService
    {
        Task<List<RecurringItemViewModel>> GetRecurringItemsAsync(int userId);

        Task<RecurringItemViewModel> SaveRecurringItemAsync(RecurringItemViewModel model, int userId);
    }
}