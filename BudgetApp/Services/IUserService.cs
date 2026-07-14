using System.Collections.Generic;
using System.Threading.Tasks;
using BudgetApp.Models;

namespace BudgetApp.Services
{
    public interface IUserService
    {
        Task<List<User>> GetAllUsersAsync();

        Task<User?> FindByUsernameAsync(string username);

        Task<User> CreateUserAsync(string username);
    }
}
