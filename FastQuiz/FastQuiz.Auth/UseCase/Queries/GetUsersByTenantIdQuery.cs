namespace FastQuiz.Auth.UseCase.Queries;

public record GetUsersByTenantIdQuery(string? Cursor, int Count, bool IsPrevious);
