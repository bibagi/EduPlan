using Schedule.Core.Models;

namespace Schedule.Core.Services;

public interface IAuthService
{
    Task<User?> AuthenticateAsync(string login, string password);
    Task<bool> CreateUserAsync(string login, string password, string role);
    Task<bool> UpdateUserAsync(int userId, string password, string role);
    Task<bool> DeleteUserAsync(int userId);
    Task<List<User>> GetAllUsersAsync();
}
