namespace FastQuiz.Auth.UseCase.Queries;

public record GetAllMyTenantQuery(string? Cursor, int Count, bool IsPrevious);
