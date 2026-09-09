namespace FastQuiz.Auth.UseCase.Queries;

public record GetAllRoleQuery(string? Cursor = null, int Count = 30, bool IsPrevious = false);
