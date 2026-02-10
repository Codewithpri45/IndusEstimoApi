using IndasEstimo.Domain.Entities.Master;

namespace IndasEstimo.Infrastructure.Security;

public interface IRefreshTokenService
{
    Task<string> GenerateRefreshTokenAsync(int tenantId, int userId);
    Task<bool> ValidateRefreshTokenAsync(string token);
    Task RevokeRefreshTokenAsync(string token, string? replacedByToken = null);
    Task<RefreshToken?> GetRefreshTokenAsync(string token);
}
