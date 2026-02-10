namespace IndasEstimo.Application.DTOs.Auth;

public record MasterLoginResponse(
    int TenantId,
    string TenantCode,
    string TenantName,
    string? CompanyCode);
