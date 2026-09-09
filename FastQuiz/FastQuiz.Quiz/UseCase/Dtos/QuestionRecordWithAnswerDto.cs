using FastQuiz.Question.Domain.Entities;

namespace FastQuiz.Quiz.UseCase.Dtos;

public record QuestionRecordWithAnswerDto(
    string Id,
    string QuizAttemptId,
    string QuestionSnapshotId,
    string MarkUpContent,
    QuestionType Type,
    int Level,
    string GroupId,
    string? ParentId,
    IReadOnlyList<QuestionAnswerSnapshotDto> Answers,
    IReadOnlyList<AnswerRecordDto> AnswerRecords,
    bool IsCorrect
);
