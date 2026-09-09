namespace FastQuiz.Auth.UseCase.Queries;

public record GetAllTenantQuery(string? Cursor = null, int Count = 30, bool IsPrevious = false);
