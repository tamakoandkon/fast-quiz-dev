namespace FastQuiz.Quiz.UseCase.Queries;

public record GetAllQuizTemplateQuery(
    string? Cursor = null,
    int Count = 30,
    bool IsPrevious = false
);
