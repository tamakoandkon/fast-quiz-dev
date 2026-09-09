namespace FastQuiz.Auth.UseCase.Queries;

public record GetUsersByOrganizationIdQuery(string? Cursor, int Count, bool IsPrevious);
