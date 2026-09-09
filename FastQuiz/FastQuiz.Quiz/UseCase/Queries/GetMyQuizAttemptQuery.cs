namespace FastQuiz.Quiz.UseCase.Queries;

public record GetMyQuizAttemptQuery(string? Cursor = null, int Count = 30);
