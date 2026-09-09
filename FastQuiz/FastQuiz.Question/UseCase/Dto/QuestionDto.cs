using FastQuiz.Question.Domain.Entities;

namespace FastQuiz.Question.UseCase.Dto;

public record QuestionDto(
    string Id,
    QuestionType Type,
    int Level,
    string? Remark,
    string GroupId,
    string MarkupContent,
    IReadOnlyList<QuestionAnswerDto> Answers
);
