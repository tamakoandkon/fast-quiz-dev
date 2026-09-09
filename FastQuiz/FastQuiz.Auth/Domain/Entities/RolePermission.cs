namespace FastQuiz.Auth.Domain.Entities;

public class RolePermission
{
    public RolePermission(string permission, string roleId, DateTimeOffset createdAt)
    {
        Permission = permission;
        RoleId = roleId;
        CreatedAt = createdAt;
    }

    private RolePermission() { }

    public string Permission { get; set; } = null!;
    public string RoleId { get; set; } = null!;
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
}
