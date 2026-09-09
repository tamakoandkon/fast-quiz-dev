namespace FastQuiz.Quiz.UseCase.Queries;

public record GetAllQuizAttemptQuery(
    string? Cursor = null,
    int Count = 30,
    bool IsPrevious = false
);
