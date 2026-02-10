namespace IndasEstimo.Domain.Entities.Master;

public class TenantLicense
{
    public int Id { get; set; }
    public int TenantId { get; set; }
    public int LicenseType { get; set; } // 1=Standard, 2=Premium, 3=Enterprise
    public int? MaxUsers { get; set; }
    public DateTime? ExpiryDate { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }

    // Navigation property
    public virtual Tenant Tenant { get; set; } = null!;
}
