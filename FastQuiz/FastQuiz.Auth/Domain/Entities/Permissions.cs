namespace FastQuiz.Auth.Domain.Entities;

/// <summary>
/// 管理员特权枚举集合
/// </summary>
public static class Permissions
{
    public const string System = "system";
    public const string Tenant = "tenant";
    public const string Organization = "organization";
    public const string Role = "role";
    public const string User = "user";
    public const string Question = "question";
    public const string QuizTemplate = "quiz-template";
    public const string QuizSnapshot = "quiz-snapshot";
    public const string QuizAttempt = "quiz-attempt";
    public static string SystemAdmin => System;

    public static IReadOnlyList<string> AllPermissions() =>
        [
            SystemAdmin,
            Tenant,
            Organization,
            Role,
            User,
            Question,
            QuizTemplate,
            QuizSnapshot,
            QuizAttempt,
        ];
}
