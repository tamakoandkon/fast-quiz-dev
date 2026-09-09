namespace FastQuiz.Auth.Domain.Entities;

public class Tenant
{
    public const string DefaultTenantId = "system_tenant";
    public const string DefaultTenantName = "system tenant";

    private Tenant() { }

    public Tenant(string id, string name, string description)
    {
        Id = id;
        Name = name;
        Description = description;
    }

    public string Id { get; set; } = null!;
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
    public DateTimeOffset UpdatedAt { get; set; } = DateTimeOffset.UtcNow;
}
