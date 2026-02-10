using IndasEstimo.Application.Common;
using IndasEstimo.Application.DTOs.Auth;

namespace IndasEstimo.Application.Interfaces.Services;

public interface IMasterAuthService
{
    Task<Result<MasterLoginResponse>> GetTenantInfoAsync(string tenantCode);
    Task<Result<List<MasterLoginResponse>>> GetAllActiveTenantsAsync();
}
