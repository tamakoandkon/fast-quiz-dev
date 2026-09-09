using FastQuiz.Question.Domain.Entities;

namespace FastQuiz.Question.UseCase.Query;

public record GetAllQuestionQuery(
    string? Cursor = null,
    QuestionType? Type = null,
    int Level = -1,
    int Count = 30,
    bool IsPrevious = false
);
