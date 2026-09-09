using FastQuiz.Question.Domain.Entities;

namespace FastQuiz.Quiz.UseCase.Dtos;

public record QuestionRecordWithoutAnswerDto(
    string Id,
    string QuizAttemptId,
    string QuestionSnapshotId,
    string MarkUpContent,
    QuestionType Type,
    int Level,
    string GroupId,
    string? ParentId,
    IReadOnlyList<QuestionAnswerRecordWithoutAnswerDto> Answers,
    IReadOnlyList<AnswerRecordDto> AnswerRecords
);
