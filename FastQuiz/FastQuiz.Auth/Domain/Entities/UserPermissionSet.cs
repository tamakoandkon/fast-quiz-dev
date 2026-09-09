namespace FastQuiz.Auth.Domain.Entities;

public class UserPermissionSet
{
    public string UserId { get; set; } = null!;
    public string TenantId { get; set; } = null!;
    public List<string> Permissions { get; set; } = [];
}
