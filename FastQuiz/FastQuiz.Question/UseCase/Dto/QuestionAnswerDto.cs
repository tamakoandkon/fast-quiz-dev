using FastQuiz.Question.Domain.Entities;

namespace FastQuiz.Question.UseCase.Dto;

public record QuestionAnswerDto(
    string Id,
    string QuestionId,
    int SortOrder,
    string MarkupContent,
    bool IsCorrect,
    MatchStrategy MatchStrategy
);
