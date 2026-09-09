namespace FastQuiz.Auth.Domain.Entities;

public class UserRole
{
    private UserRole() { }

    public UserRole(string userId, string roleId, string tenantId)
    {
        UserId = userId;
        RoleId = roleId;
        TenantId = tenantId;
    }

    public string UserId { get; set; } = null!;
    public string RoleId { get; set; } = null!;
    public string TenantId { get; set; } = null!;
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
    public DateTimeOffset UpdatedAt { get; set; } = DateTimeOffset.UtcNow;
}
