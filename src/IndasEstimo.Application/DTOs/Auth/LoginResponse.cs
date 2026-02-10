namespace IndasEstimo.Application.DTOs.Auth;

public record LoginResponse(
    string AccessToken,
    string RefreshToken,
    DateTime ExpiresAt,
    UserInfoDto User);
