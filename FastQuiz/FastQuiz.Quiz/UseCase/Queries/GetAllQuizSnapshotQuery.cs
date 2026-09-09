namespace FastQuiz.Quiz.UseCase.Queries;

public record GetAllQuizSnapshotQuery(string? Cursor = null, int Count = 10);
