namespace FastQuiz.Quiz.UseCase.Queries;

/// <summary>
/// 查询某个考生的考试记录。本人或有 quiz 模块权限(QuizAttempt)的人可用。
/// </summary>
/// <param name="TemplateId">考试模板 Id，为空时不限定考试</param>
/// <param name="OnlyBest">是否只取每场考试最好的一次成绩</param>
public record GetUserRecordsQuery(
    string? TemplateId = null,
    bool OnlyBest = false,
    string? Cursor = null,
    int Count = 30,
    bool IsPrevious = false
);
