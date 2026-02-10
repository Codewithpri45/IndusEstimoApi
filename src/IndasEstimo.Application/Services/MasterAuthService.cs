using IndasEstimo.Application.Common;
using IndasEstimo.Application.DTOs.Auth;
using IndasEstimo.Application.Interfaces.Services;
using IndasEstimo.Domain.Enums;

namespace IndasEstimo.Application.Services;

// Note: This service will be completed in Infrastructure layer with database dependencies
// This is a placeholder to satisfy the Application layer interface
public class MasterAuthServicePlaceholder : IMasterAuthService
{
    public Task<Result<MasterLoginResponse>> GetTenantInfoAsync(string tenantCode)
    {
        throw new NotImplementedException("This will be implemented in Infrastructure layer");
    }

    public Task<Result<List<MasterLoginResponse>>> GetAllActiveTenantsAsync()
    {
        throw new NotImplementedException("This will be implemented in Infrastructure layer");
    }
}
