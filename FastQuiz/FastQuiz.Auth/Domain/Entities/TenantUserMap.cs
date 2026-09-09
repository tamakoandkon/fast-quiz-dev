namespace FastQuiz.Auth.Domain.Entities;

public class TenantUserMap
{
    private TenantUserMap() { }

    public TenantUserMap(string tenantId, string userId)
    {
        TenantId = tenantId;
        UserId = userId;
    }

    public string TenantId { get; set; } = null!;
    public string UserId { get; set; } = null!;
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
}
