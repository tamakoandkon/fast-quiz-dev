using FastQuiz.Auth.Interface.Extensions;
using FastQuiz.Question.Interface.Setup;
using FastQuiz.Quiz.Interface.Setup;

namespace FastQuiz.API.Setup;

public static class ApplyMigrationsExtensions
{
    public static void ApplyAllMigrations(this IApplicationBuilder app)
    {
        app.ApplyAuthMigrations();
        app.ApplyQuestionMigrations();
        app.ApplyQuizMigrations();
    }
}
