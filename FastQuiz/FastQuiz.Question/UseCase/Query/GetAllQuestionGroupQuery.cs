namespace FastQuiz.Question.UseCase.Query;

public record GetAllQuestionGroupQuery(
    string? Cursor = null,
    int Count = 30,
    bool IsPrevious = false,
    string? NamePrefix = null
);
