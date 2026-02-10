using Microsoft.AspNetCore.Http;
using IndasEstimo.Application.Exceptions;
using IndasEstimo.Shared.Constants;

namespace IndasEstimo.Infrastructure.MultiTenancy;

public class TenantProvider : ITenantProvider
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public TenantProvider(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public TenantInfo GetCurrentTenant()
    {
        var httpContext = _httpContextAccessor.HttpContext;
        if (httpContext == null)
        {
            throw new InvalidOperationException("HttpContext is not available");
        }

        if (!httpContext.Items.TryGetValue(HttpContextKeys.TenantInfo, out var tenantInfoObj))
        {
            throw new TenantNotFoundException("Tenant information not found in request context");
        }

        return (TenantInfo)tenantInfoObj!;
    }

    public TenantInfo? GetCurrentTenantOrDefault()
    {
        var httpContext = _httpContextAccessor.HttpContext;
        if (httpContext == null)
        {
            return null;
        }

        if (!httpContext.Items.TryGetValue(HttpContextKeys.TenantInfo, out var tenantInfoObj))
        {
            return null;
        }

        return tenantInfoObj as TenantInfo;
    }
}
