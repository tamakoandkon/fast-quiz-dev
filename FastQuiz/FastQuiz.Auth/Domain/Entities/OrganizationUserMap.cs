namespace FastQuiz.Auth.Domain.Entities;

public class OrganizationUserMap
{
    private OrganizationUserMap() { }

    public OrganizationUserMap(string organizationId, string userId)
    {
        OrganizationId = organizationId;
        UserId = userId;
    }

    public string OrganizationId { get; set; } = null!;
    public string UserId { get; set; } = null!;
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
}
