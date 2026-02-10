using IndasEstimo.Application.Common;
using IndasEstimo.Application.DTOs.Auth;

namespace IndasEstimo.Application.Interfaces.Services;

public interface IAuthService
{
    Task<Result<LoginResponse>> AuthenticateAsync(string tenantCode, string username, string password);
    Task<Result<RefreshTokenResponse>> RefreshTokenAsync(string refreshToken);
    Task<Result> RevokeTokenAsync(string refreshToken);
    Task<Result<UserInfoDto>> GetUserInfoAsync(int userId);
}
