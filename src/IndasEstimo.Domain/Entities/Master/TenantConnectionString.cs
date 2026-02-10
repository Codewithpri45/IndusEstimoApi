namespace IndasEstimo.Domain.Entities.Master;

public class TenantConnectionString
{
    public int Id { get; set; }
    public int TenantId { get; set; }
    public string ServerName { get; set; } = string.Empty;
    public string DatabaseName { get; set; } = string.Empty;
    public string DbUserName { get; set; } = string.Empty;
    public string DbPasswordEncrypted { get; set; } = string.Empty; // AES-256 encrypted
    public string EncryptionIV { get; set; } = string.Empty; // Initialization Vector for AES
    public DateTime CreatedAt { get; set; }

    // Navigation property
    public virtual Tenant Tenant { get; set; } = null!;
}
