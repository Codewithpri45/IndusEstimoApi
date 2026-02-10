namespace IndasEstimo.Domain.Entities.Master;

public class Tenant
{
    public int TenantId { get; set; }
    public string TenantCode { get; set; } = string.Empty;
    public string TenantName { get; set; } = string.Empty;
    public string? CompanyCode { get; set; }
    public int Status { get; set; } // 1=Active, 2=Suspended, 3=Expired
    public DateTime CreatedAt { get; set; }

    // Navigation properties
    public virtual TenantConnectionString? ConnectionString { get; set; }
    public virtual ICollection<TenantLicense> Licenses { get; set; } = new List<TenantLicense>();
    public virtual ICollection<RefreshToken> RefreshTokens { get; set; } = new List<RefreshToken>();
}
