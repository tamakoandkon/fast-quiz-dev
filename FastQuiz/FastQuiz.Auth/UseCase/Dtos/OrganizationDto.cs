namespace FastQuiz.Auth.UseCase.Dtos;

public record OrganizationDto(
    string Id,
    string Name,
    string TenantId,
    string? ParentId,
    DateTimeOffset CreatedAt,
    DateTimeOffset UpdatedAt
);
