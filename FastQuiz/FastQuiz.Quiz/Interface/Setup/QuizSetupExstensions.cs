using FastQuiz.Common.Models;
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
            case DatabaseType.Sqlite:
                {
                    services.AddSqliteRepo();
                }
                break;
            case DatabaseType.MariaDb:
                {
                    services.AddMariaRepo();
                }
                break;
            case DatabaseType.PostgreSql:
                {
                    services.AddPostgreSqlRepo();
                }
                break;
            default:
                throw new InvalidOperationException(
                    $"不支持的数据库类型：{settings.Type},请在sqlite,mariadb,postgresql中选一个"
                );
        }

        services
            .AddScoped<IQuizTemplateService, QuizTemplateService>()
            .AddScoped<IQuizSnapshotService, QuizSnapshotService>()
            .AddScoped<IQuizAtteptService, QuizAttemptService>()
            .AddScoped<IQuestionGradingDecider, QuestionGradingDecider>()
            .AddScoped<IQuizReviewService, QuizReviewService>();

        services.AddScoped<IAnswerBufferService, AnswerBufferService>();

        services.AddQuizCache();

        return services;
    }

    /// <summary>
    /// 生产环境专用：自动应用数据库迁移
    /// </summary>
    public static void ApplyQuizMigrations(this IApplicationBuilder app)
    {
        using var scope = app.ApplicationServices.CreateScope();
        var sp = scope.ServiceProvider;

        var settings = sp.GetRequiredService<IOptions<QuizDbContextSettings>>().Value;

        DbContext db = settings.Type switch
        {
            DatabaseType.Sqlite => sp.GetRequiredService<SqliteQuizDbContext>(),
            DatabaseType.MariaDb => sp.GetRequiredService<MariaDbQuizDbContext>(),
            DatabaseType.PostgreSql => sp.GetRequiredService<PostgreSQLQuizDbContext>(),
            _ => throw new InvalidOperationException(
                $"不支持的数据库类型：{settings.Type},请在sqlite,mariadb,postgresql中选一个"
            ),
        };

        if (db.Database.GetPendingMigrations().Any())
        {
            db.Database.Migrate();
        }
    }

    public static void MapQuizEndPoints(this IEndpointRouteBuilder app)
    {
        app.MapQuizTemplateEndPoints();
        app.MapQuizSnapshotEndPoints();
        app.MapQuizAttemptEndPoints();
    }

    private static IServiceCollection AddPostgreSqlRepo(this IServiceCollection services)
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

                options.UseNpgsql(dbSettings.ConnectionString, x => x.CommandTimeout(30));
            }
        );

        return services
            .AddScoped<IQuizUnitOfWork>(sp =>
                new EfQuizUnitOfWork(sp.GetRequiredService<PostgreSQLQuizDbContext>())
            )
            .AddScoped<IQuizTemplateRepository, PostgreSQLQuizTemplateRepository>()
            .AddScoped<
                IQuestionSelectionRuleRepository,
                PostgreSQLQuestionSelectionRuleRepository
            >()
            .AddScoped<IQuizSnapshotRepository, PostgreSQLQuizSnapshotRepository>()
            .AddScoped<IQuestionSnapshotRepository, PostgreSQLQuestionSnapshotRepository>()
            .AddScoped<
                IQuizSnapshotQuestionSnapshotMapRepository,
                PostgreSQLQuizSnapshotQuestionSnapshotMapRepository
            >()
            .AddScoped<IQuizAttemptRepository, PostgreSQLQuizAttemptRepository>()
            .AddScoped<IQuestionRecordRepository, PostgreSQLQuestionRecordRepository>();
    }

    private static IServiceCollection AddMariaRepo(this IServiceCollection services)
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
                    ServerVersion.AutoDetect(dbSettings.ConnectionString),
                    x => x.CommandTimeout(30)
                );
            }
        );

        return services
            .AddScoped<IQuizUnitOfWork>(sp =>
                new EfQuizUnitOfWork(sp.GetRequiredService<MariaDbQuizDbContext>())
            )
            .AddScoped<IQuizTemplateRepository, MariaDbQuizTemplateRepository>()
            .AddScoped<IQuestionSelectionRuleRepository, MariaDbQuestionSelectionRuleRepository>()
            .AddScoped<IQuizSnapshotRepository, MariaDbQuizSnapshotRepository>()
            .AddScoped<IQuestionSnapshotRepository, MariaDbQuestionSnapshotRepository>()
            .AddScoped<
                IQuizSnapshotQuestionSnapshotMapRepository,
                MariaDbQuizSnapshotQuestionSnapshotMapRepository
            >()
            .AddScoped<IQuizAttemptRepository, MariaDbQuizAttemptRepository>()
            .AddScoped<IQuestionRecordRepository, MariaDbQuestionRecordRepository>();
    }

    private static IServiceCollection AddSqliteRepo(this IServiceCollection services)
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

        return services
            .AddScoped<IQuizUnitOfWork>(sp =>
                new EfQuizUnitOfWork(sp.GetRequiredService<SqliteQuizDbContext>())
            )
            .AddScoped<IQuizTemplateRepository, SqliteQuizTemplateRepository>()
            .AddScoped<IQuestionSelectionRuleRepository, SqliteQuestionSelectionRuleRepository>()
            .AddScoped<IQuizSnapshotRepository, SqliteQuizSnapshotRepository>()
            .AddScoped<IQuestionSnapshotRepository, SqliteQuestionSnapshotRepository>()
            .AddScoped<
                IQuizSnapshotQuestionSnapshotMapRepository,
                SqliteQuizSnapshotQuestionSnapshotMapRepository
            >()
            .AddScoped<IQuizAttemptRepository, SqliteQuizAttemptRepository>()
            .AddScoped<IQuestionRecordRepository, SqliteQuestionRecordRepository>();
    }
}
