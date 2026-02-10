using IndasEstimo.Domain.Entities.Master;
using IndasEstimo.Domain.Enums;

namespace IndasEstimo.Infrastructure.Database.Services;

public interface IMasterDbService
{
    Task<Tenant?> GetTenantByCodeAsync(string tenantCode);
    Task<Tenant?> GetTenantByIdAsync(int tenantId);
    Task<string?> GetTenantConnectionStringAsync(int tenantId);
    Task<TenantStatus> GetTenantStatusAsync(int tenantId);
    Task<List<Tenant>> GetAllActiveTenantsAsync();
    Task<bool> ValidateTenantLicenseAsync(int tenantId);
}
