namespace IndasEstimo.Domain.Entities.Master;

public class RefreshToken
{
    public int Id { get; set; }
    public int UserId { get; set; } // Reference to user in tenant DB
    public int TenantId { get; set; }
    public string Token { get; set; } = string.Empty;
    public DateTime ExpiresAt { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? RevokedAt { get; set; }
    public string? ReplacedByToken { get; set; } // For token rotation
    public bool IsRevoked { get; set; }

    // Navigation property
    public virtual Tenant Tenant { get; set; } = null!;

    // Computed properties
    public bool IsExpired => DateTime.UtcNow >= ExpiresAt;
    public bool IsActive => !IsRevoked && !IsExpired;
}
