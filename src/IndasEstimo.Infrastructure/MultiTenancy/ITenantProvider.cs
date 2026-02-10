namespace IndasEstimo.Infrastructure.MultiTenancy;

public interface ITenantProvider
{
    TenantInfo GetCurrentTenant();
    TenantInfo? GetCurrentTenantOrDefault();
}
