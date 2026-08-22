using System.Collections.Generic;
using System.Threading.Tasks;
using BudgetApp.ViewModels;

namespace BudgetApp.Services
{
    public interface IRecurringItemService
    {
        Task<List<RecurringItemViewModel>> GetUserRecurringItemsAsync(int userId);

        Task<RecurringItemViewModel> SaveRecurringItemAsync(RecurringItemViewModel model, int userId);

        Task DeleteRecurringItemAsync(int id, int userId);

        Task SetActiveAsync(int id, bool isActive, int userId);
    }
}
