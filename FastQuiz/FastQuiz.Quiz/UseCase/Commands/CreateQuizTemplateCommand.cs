namespace FastQuiz.Quiz.UseCase.Commands;

public record CreateQuizTemplateCommand(
    string Name,
    string Description,
    string Remark,
    int AttemptLimit,
    bool IsUnlimitedTime,
    DateTimeOffset BeginTime,
    DateTimeOffset EndTime,
    int DurationMinutes,
    string? UserGroupId
);
