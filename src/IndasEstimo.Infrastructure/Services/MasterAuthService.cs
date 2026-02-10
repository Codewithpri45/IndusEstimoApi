using IndasEstimo.Application.Common;
using IndasEstimo.Application.DTOs.Auth;
using IndasEstimo.Application.Interfaces.Services;
using IndasEstimo.Domain.Enums;
using IndasEstimo.Infrastructure.Database.Services;

namespace IndasEstimo.Infrastructure.Services;

public class MasterAuthService : IMasterAuthService
{
    private readonly IMasterDbService _masterDbService;

    public MasterAuthService(IMasterDbService masterDbService)
    {
        _masterDbService = masterDbService;
    }

    public async Task<Result<MasterLoginResponse>> GetTenantInfoAsync(string tenantCode)
    {
        var tenant = await _masterDbService.GetTenantByCodeAsync(tenantCode);

        if (tenant == null)
        {
            return Result<MasterLoginResponse>.Failure("Tenant not found");
        }

        if (tenant.Status != (int)TenantStatus.Active)
        {
            return Result<MasterLoginResponse>.Failure($"Tenant is {(TenantStatus)tenant.Status}");
        }

        var response = new MasterLoginResponse(
            tenant.TenantId,
            tenant.TenantCode,
            tenant.TenantName,
            tenant.CompanyCode);

        return Result<MasterLoginResponse>.Success(response);
    }

    public async Task<Result<List<MasterLoginResponse>>> GetAllActiveTenantsAsync()
    {
        var tenants = await _masterDbService.GetAllActiveTenantsAsync();

        var response = tenants.Select(t => new MasterLoginResponse(
            t.TenantId,
            t.TenantCode,
            t.TenantName,
            t.CompanyCode)).ToList();

        return Result<List<MasterLoginResponse>>.Success(response);
    }
}
