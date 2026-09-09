using FastQuiz.Question.Domain.Entities;

namespace FastQuiz.Quiz.UseCase.Dtos;

public record QuestionAnswerRecordWithoutAnswerDto(
    string Id,
    string MarkUpContent,
    string QuestionSnapshotId,
    int SortOrder,
    MatchStrategy MatchStrategy
);
