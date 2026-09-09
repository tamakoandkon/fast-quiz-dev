namespace FastQuiz.Quiz.UseCase.Queries;

public record GetAllQuestionSnapshotQuery(string? Cursor = null, int Count = 30);
