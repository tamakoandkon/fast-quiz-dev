namespace FastQuiz.Auth.Domain.Entities;

public class Organization
{
    public const string DefaultOrganizationId = "default_organization_id";
    public const string DefaultOrganizationName = "default_organization_name";

    private Organization() { }

    public Organization(string id, string tenantId, string? parentId, string name)
    {
        Id = id;
        TenantId = tenantId;
        ParentId = parentId;
        Name = name;
    }

    public string Id { get; set; } = null!;
    public string TenantId { get; set; } = null!;
    public string? ParentId { get; set; }
    public string Name { get; set; } = string.Empty;
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
    public DateTimeOffset UpdatedAt { get; set; } = DateTimeOffset.UtcNow;
}
