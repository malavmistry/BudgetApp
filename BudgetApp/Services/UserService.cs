using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using BudgetApp.Data;
using BudgetApp.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace BudgetApp.Services
{
    public class UserService : IUserService
    {
        private readonly AppDbContext _dbContext;
        private readonly ILogger<UserService> _logger;

        public UserService(AppDbContext dbContext, ILogger<UserService> logger)
        {
            _dbContext = dbContext;
            _logger = logger;
        }

        public async Task<List<User>> GetAllUsersAsync()
        {
            return await _dbContext.Users
                .OrderBy(u => u.Username)
                .ToListAsync();
        }

        public async Task<User?> FindByUsernameAsync(string username)
        {
            return await _dbContext.Users
                .FirstOrDefaultAsync(u => u.Username == username);
        }

        public async Task<User> CreateUserAsync(string username)
        {
            var user = new User
            {
                Username = username,
                CreatedAt = DateTime.UtcNow
            };

            _dbContext.Users.Add(user);
            await _dbContext.SaveChangesAsync();

            _logger.LogInformation("Created user {Username}", username);
            return user;
        }
    }
}
