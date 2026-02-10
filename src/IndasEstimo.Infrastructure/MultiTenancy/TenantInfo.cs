namespace IndasEstimo.Infrastructure.MultiTenancy;

public class TenantInfo
{
    public int TenantId { get; set; }
    public string TenantCode { get; set; } = string.Empty;
    public string ConnectionString { get; set; } = string.Empty;
}
