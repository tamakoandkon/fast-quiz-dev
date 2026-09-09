namespace FastQuiz.Auth.Domain.Entities;

public class Role
{
    public const string DefaultAdminRoleId = "default_admin";
    public const string DefaultAdminRoleName = "system admin";

    private Role() { }

    public Role(
        string id,
        string tenantId,
        string name,
        string description,
        bool isSystem,
        bool isEnabled
    )
    {
        Id = id;
        TenantId = tenantId;
        Name = name;
        Description = description;
        IsSystem = isSystem;
        IsEnabled = isEnabled;
    }

    public string Id { get; set; } = null!;
    public string TenantId { get; set; } = null!;
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public bool IsSystem { get; set; } = false;
    public bool IsEnabled { get; set; } = true;
    public List<string> Permissions { get; set; } = [];
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
    public DateTimeOffset UpdatedAt { get; set; } = DateTimeOffset.UtcNow;
}
