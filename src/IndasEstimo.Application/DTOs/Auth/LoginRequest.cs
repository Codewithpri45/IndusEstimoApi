namespace IndasEstimo.Application.DTOs.Auth;

public record LoginRequest(
    string TenantCode,
    string Username,
    string Password);
