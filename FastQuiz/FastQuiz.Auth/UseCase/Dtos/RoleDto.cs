namespace FastQuiz.Auth.UseCase.Dtos;

public record RoleDto(
    string Id,
    string Name,
    string Description,
    string TenantId,
    bool IsSystem,
    bool IsEnable,
    DateTimeOffset CreatedAt,
    DateTimeOffset UpdatedAt,
    List<string> Permissions
);
