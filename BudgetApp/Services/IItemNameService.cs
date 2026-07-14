using System.Collections.Generic;
using System.Threading.Tasks;
using BudgetApp.Models;

namespace BudgetApp.Services
{
    public interface IItemNameService
    {
        Task<List<ItemName>> SearchAsync(string query);

        Task<ItemName> GetOrCreateAsync(string name);
    }
}
