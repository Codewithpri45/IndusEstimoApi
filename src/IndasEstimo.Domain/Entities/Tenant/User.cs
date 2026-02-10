namespace IndasEstimo.Domain.Entities.Tenant;

public class User
{
    public int UserId { get; set; }
    public int CompanyId { get; set; }
    public int ProductionUnitId { get; set; }
    public string FYear { get; set; } = string.Empty;
    public string Username { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty; // PBKDF2 hash
    public string? Email { get; set; }
    public string? FullName { get; set; }
    public int Status { get; set; } // 1=Active, 2=Inactive, 3=Locked
    public DateTime CreatedAt { get; set; }
    public DateTime? LastLoginAt { get; set; }

    // Navigation properties
    public virtual ICollection<UserRole> UserRoles { get; set; } = new List<UserRole>();
}
