using IndasEstimo.Domain.Entities.Tenant;

namespace IndasEstimo.Infrastructure.Database.Services;

public interface ITenantDbService
{
    Task<User?> GetUserByUsernameAsync(string username);
    Task<User?> GetUserByIdAsync(int userId);
    Task<bool> ValidateUserCredentialsAsync(string username, string password);
    Task<List<string>> GetUserRolesAsync(int userId);
    Task UpdateLastLoginAsync(int userId);
}
