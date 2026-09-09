namespace FastQuiz.Auth.UseCase.Dtos;

public record TenantDto(
    string Id,
    string Name,
    string Description,
    DateTimeOffset CreatedAt,
    DateTimeOffset UpdatedAt
);
