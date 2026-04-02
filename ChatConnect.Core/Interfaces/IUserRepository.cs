namespace ChatConnect.Core.Interfaces;
using ChatConnect.Core.Entities;

public interface IUserRepository
{
    Task<User?> GetByIdAsync(int id);
    Task<User?> GetByEmailAsync(string email);
    Task<User> AddAsync(User user);
    Task UpdateAsync(User user);
    Task<IEnumerable<User>> SearchUsersAsync(string query, int excludeUserId);
    Task<IEnumerable<User>> GetOnlineUsersAsync();
}
