using FastQuiz.Auth.Interface.Extensions;
using FastQuiz.Question.Interface.Setup;
using FastQuiz.Quiz.Interface.Setup;

namespace FastQuiz.API.Setup;

public static class ApplyMigrationsExtensions
{
    /// <summary>
    /// 根据配置应用所有迁移。如果配置中设置了 "Migrations:RunMigrationsOnly" 为 true，则仅运行迁移并返回 true。否则，如果 "Migrations:RunMigrationsOnStartup" 为 true，则在启动时应用所有迁移。
    /// </summary>
    /// <param name="app"></param>
    /// <param name="configuration"></param>
    /// <returns></returns>
    public static bool ApplyAllMigrations(
        this IApplicationBuilder app,
        IConfiguration configuration
    )
    {
        var runMigrationsOnly = configuration.GetValue<bool>("Migrations:RunMigrationsOnly");

        if (runMigrationsOnly)
        {
            app.ApplyAllMigrations();
            return true;
        }

        var runMigrationsOnStartup = configuration.GetValue(
            "Migrations:RunMigrationsOnStartup",
            true
        );
        if (runMigrationsOnStartup)
        {
            app.ApplyAllMigrations();
        }
        return false;
    }

    public static void ApplyAllMigrations(this IApplicationBuilder app)
    {
        app.ApplyAuthMigrations();
        app.ApplyQuestionMigrations();
        app.ApplyQuizMigrations();
    }
}
