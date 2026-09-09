namespace FastQuiz.Auth.UseCase.Queries;

public record GetAllOrganizationQuery(string? Cursor, int Count, bool IsPrevious);
