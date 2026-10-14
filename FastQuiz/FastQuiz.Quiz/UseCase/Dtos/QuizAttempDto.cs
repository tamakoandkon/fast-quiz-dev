using FastQuiz.Quiz.Domain.Entities;

namespace FastQuiz.Quiz.UseCase.Dtos;

public record QuizAttempDto(
    string Id,
    string AssignId,
    string QuizTemplateId,
    string QuizTemplateNema,
    string QuizSampleId,
    DateTimeOffset? StartTime,
    DateTimeOffset? SubmitTime,
    int Score,
    QuizAttemptStatus Status,
    string? AssignUsername = null
);
