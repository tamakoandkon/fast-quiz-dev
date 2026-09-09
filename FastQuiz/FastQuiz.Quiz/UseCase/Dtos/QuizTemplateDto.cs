namespace FastQuiz.Quiz.UseCase.Dtos;

public record QuizTemplateDto(
    string Id,
    string Name,
    string Description,
    string Remark,
    int AttemptLimit,
    bool IsUnlimitedTime,
    DateTimeOffset BeginTime,
    DateTimeOffset EndTime,
    int DurationMinutes,
    bool IsPublished,
    string OwnerId,
    DateTimeOffset CreatedAt,
    DateTimeOffset UpdatedAt,
    string? UserGroupId
);
