using FastQuiz.Question.Domain.Entities;

namespace FastQuiz.Quiz.UseCase.Dtos;

public record QuestionAnswerSnapshotDto(
    string Id,
    string MarkUpContent,
    bool IsCorrect,
    string QuestionSnapshotId,
    int SortOrder,
    MatchStrategy MatchStrategy
);
