using FastQuiz.Question.Domain.Entities;

namespace FastQuiz.Quiz.UseCase.Dtos;

public record QuestionSnapshotDto(
    string Id,
    string QuizTemplateId,
    string QuestionId,
    string MarkUpContent,
    QuestionType Type,
    int Level,
    string GroupId,
    string? ParentId,
    IReadOnlyList<QuestionAnswerSnapshotDto> Answers
);
