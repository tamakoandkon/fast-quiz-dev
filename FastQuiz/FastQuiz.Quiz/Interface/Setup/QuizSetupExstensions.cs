using FastQuiz.Quiz.Domain.Repositories;
using FastQuiz.Quiz.Domain.Services;
using FastQuiz.Quiz.Infrastructure.Repositories;
using FastQuiz.Quiz.Infrastructure.Repositories.MariaDb;
using FastQuiz.Quiz.Infrastructure.Repositories.PostgreSQL;
using FastQuiz.Quiz.Infrastructure.Repositories.Sqlite;
using FastQuiz.Quiz.UseCase.Extensions;
using FastQuiz.Quiz.UseCase.Services;
using FastQuiz.Quiz.UseCase.Validators;
using FluentValidation;
using Hangfire;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace FastQuiz.Quiz.Interface.Setup;

public static class QuizSetupExstensions
{
    public static IServiceCollection AddQuizService(
        this IServiceCollection services,
        Action<QuizDbContextSettings> dbSettingAction
    )
    {
        services.AddValidatorsFromAssemblyContaining<CreateQuizTemplateCommandValidator>();

        services.Configure(dbSettingAction);

        var settings = new QuizDbContextSettings();
        dbSettingAction(settings);

        switch (settings.Type)
        {
            case "sqlite":
                {
                    services.AddDbContext<SqliteQuizDbContext>(
                        (s, options) =>
                        {
                            var dbSettings = s.GetRequiredService<IOptions<QuizDbContextSettings>>().Value;

                            if (string.IsNullOrEmpty(dbSettings.ConnectionString))
                            {
                                throw new InvalidOperationException(
                                    "SQLite connection string is not configured."
                                );
                            }

                            options.UseSqlite(
                                dbSettings.ConnectionString,
                                x =>
                                {
                                    x.CommandTimeout(30);
                                }
                            );
                        }
                    );
                }
                break;
            case "mariadb":
                {
                    services.AddDbContext<MariaDbQuizDbContext>(
                        (s, options) =>
                        {
                            var dbSettings = s.GetRequiredService<IOptions<QuizDbContextSettings>>().Value;

                            if (string.IsNullOrEmpty(dbSettings.ConnectionString))
                            {
                                throw new InvalidOperationException(
                                    "MariaDb connection string is not configured."
                                );
                            }

                            options.UseMySql(
                                dbSettings.ConnectionString,
                                ServerVersion.AutoDetect(dbSettings.ConnectionString)
                            );
                        }
                    );
                }
                break;
            case "postgresql":
                {
                    services.AddDbContext<PostgreSQLQuizDbContext>(
                        (s, options) =>
                        {
                            var dbSettings = s.GetRequiredService<IOptions<QuizDbContextSettings>>().Value;

                            if (string.IsNullOrEmpty(dbSettings.ConnectionString))
                            {
                                throw new InvalidOperationException(
                                    "PostgreSQL connection string is not configured."
                                );
                            }

                            options.UseNpgsql(dbSettings.ConnectionString);
                        }
                    );
                }
                break;
            default:
                throw new InvalidOperationException(
                    $"不支持的数据库类型：{settings.Type},请在sqlite,mariadb,postgresql中选一个"
                );
        }

        services
            .AddScoped<IQuizTemplateRepository, SqliteQuizTemplateRepository>()
            .AddScoped<IQuestionSelectionRuleRepository, SqliteQuestionSelectionRuleRepository>()
            .AddScoped<IQuizTemplateService, QuizTemplateService>()
            .AddScoped<IQuizSnapshotRepository, SqliteQuizSnapshotRepository>()
            .AddScoped<IQuestionSnapshotRepository, SqliteQuestionSnapshotRepository>()
            .AddScoped<
                IQuizSnapshotQuestionSnapshotMapRepository,
                SqliteQuizSnapshotQuestionSnapshotMapRepository
            >()
            .AddScoped<IQuizSnapshotService, QuizSnapshotService>()
            .AddScoped<IQuizAttemptRepository, SqliteQuizAttemptRepository>()
            .AddScoped<IQuestionRecordRepository, SqliteQuestionRecordRepository>()
            .AddScoped<IQuizAtteptService, QuizAttemptService>()
            .AddScoped<IQuestionGradingDecider, QuestionGradingDecider>()
            .AddScoped<IQuizReviewService, QuizReviewService>();

        services.AddSingleton<IAnswerBufferService, AnswerBufferService>();
        services.AddHostedService<AnswerBufferBackgroundService>();

        // Add Hangfire services.
        services.AddHangfire(configuration =>
            configuration
                .SetDataCompatibilityLevel(CompatibilityLevel.Version_180)
                .UseSimpleAssemblyNameTypeSerializer()
                .UseRecommendedSerializerSettings()
                .UseInMemoryStorage()
        );

        // Add the processing server as IHostedService
        services.AddHangfireServer();

        services.AddQuizCache();

        return services;
    }

    /// <summary>
    /// 生产环境专用：自动应用数据库迁移
    /// </summary>
    public static void ApplyQuizMigrations(this IApplicationBuilder app)
    {
        using var scope = app.ApplicationServices.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<SqliteQuizDbContext>();

        if (db.Database.GetPendingMigrations().Any())
        {
            db.Database.Migrate();
        }

        app.UseHangfireDashboard();
        var backgroundJobs = scope.ServiceProvider.GetRequiredService<IBackgroundJobClient>();
        backgroundJobs.Enqueue(() => Console.WriteLine("Hello world from Hangfire!"));
    }

    public static void MapQuizEndPoints(this IEndpointRouteBuilder app)
    {
        app.MapQuizTemplateApi();
        app.MapQuizSnapshotApi();
        app.MapQuizAttemptApi();
    }
}
