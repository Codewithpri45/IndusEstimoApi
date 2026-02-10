using Microsoft.Extensions.Caching.Memory;
using IndasEstimo.Application.Exceptions;
using IndasEstimo.Domain.Enums;
using IndasEstimo.Infrastructure.Database.Services;
using IndasEstimo.Infrastructure.MultiTenancy;
using IndasEstimo.Shared.Constants;
using IndasEstimo.Shared.Extensions;

namespace IndasEstimo.Api.Middleware;

public class TenantMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<TenantMiddleware> _logger;

    public TenantMiddleware(RequestDelegate next, ILogger<TenantMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(
        HttpContext context,
        IMasterDbService masterDbService,
        IMemoryCache cache)
    {
        // Skip tenant resolution for auth endpoints and health checks
        var path = context.Request.Path.Value?.ToLower() ?? "";
        if (path.StartsWith("/api/auth") || path.StartsWith("/health") || path.StartsWith("/swagger"))
        {
            await _next(context);
            return;
        }

        // Only process authenticated requests
        if (context.User?.Identity?.IsAuthenticated != true)
        {
            await _next(context);
            return;
        }

        try
        {
            // Extract tenant ID from JWT claims
            var tenantId = context.User.GetTenantId();
            var tenantCode = context.User.GetTenantCode();

            if (!tenantId.HasValue || string.IsNullOrEmpty(tenantCode))
            {
                throw new UnauthorizedTenantAccessException("Tenant information missing from token");
            }

            // Try cache first (5-minute TTL)
            var cacheKey = $"tenant_conn_{tenantId.Value}";
            if (!cache.TryGetValue(cacheKey, out string? connectionString))
            {
                // Cache miss: Query Master DB
                connectionString = await masterDbService.GetTenantConnectionStringAsync(tenantId.Value);

                if (string.IsNullOrEmpty(connectionString))
                {
                    throw new TenantNotFoundException($"Connection string not found for tenant {tenantId.Value}");
                }

                // Validate tenant status
                var tenantStatus = await masterDbService.GetTenantStatusAsync(tenantId.Value);
                if (tenantStatus != TenantStatus.Active)
                {
                    throw new TenantSuspendedException($"Tenant {tenantCode} is {tenantStatus}");
                }

                // Cache for 5 minutes
                var cacheOptions = new MemoryCacheEntryOptions()
                    .SetAbsoluteExpiration(TimeSpan.FromMinutes(5));
                cache.Set(cacheKey, connectionString, cacheOptions);

                _logger.LogInformation("Cached connection string for tenant {TenantId}", tenantId.Value);
            }

            // Set tenant context in HttpContext.Items for downstream services
            var tenantInfo = new TenantInfo
            {
                TenantId = tenantId.Value,
                TenantCode = tenantCode,
                ConnectionString = connectionString
            };

            context.Items[HttpContextKeys.TenantInfo] = tenantInfo;

            _logger.LogDebug("Tenant resolved: {TenantId} - {TenantCode}", tenantId.Value, tenantCode);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Tenant resolution failed");
            throw; // Will be caught by ExceptionMiddleware
        }

        await _next(context);
    }
}
