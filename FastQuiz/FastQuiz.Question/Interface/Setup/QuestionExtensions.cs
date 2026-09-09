using System.Reflection;
using FastQuiz.Question.Domain.Repositories;
using FastQuiz.Question.Infrastructure.Repositories;
using FastQuiz.Question.Infrastructure.Repositories.MariaDb;
using FastQuiz.Question.Infrastructure.Repositories.PostgreSQL;
using FastQuiz.Question.Infrastructure.Repositories.Sqlite;
using FastQuiz.Question.UseCase.Extensions;
using FastQuiz.Question.UseCase.Services;
using FastQuiz.Question.UseCase.Validators;
using FluentValidation;
using Mapster;
using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace FastQuiz.Question.Interface.Setup;

public static class QuestionExtensions
{
    public static IServiceCollection AddQuestionService(
        this IServiceCollection services,
        Action<QuestionDbContextSettings> dbSettingAction
    )
    {
        services.AddValidatorsFromAssemblyContaining<CreateQuestionGroupCommandValidator>();

        services.Configure(dbSettingAction);

        var settings = new QuestionDbContextSettings();

        dbSettingAction(settings);

        switch (settings.Type)
        {
            case "sqlite":
                {
                    services.AddDbContext<SqliteQuestionDbContext>(
                        (s, options) =>
                        {
                            var dbSettings = s.GetRequiredService<
                                IOptions<QuestionDbContextSettings>
                            >().Value;

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
                    services.AddDbContext<MariaDbQuestionDbContext>(
                        (s, options) =>
                        {
                            var dbSettings = s.GetRequiredService<
                                IOptions<QuestionDbContextSettings>
                            >().Value;

                            if (string.IsNullOrEmpty(dbSettings.ConnectionString))
                            {
                                throw new InvalidOperationException(
                                    "SQLite connection string is not configured."
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
                    services.AddDbContext<PostgreSqlQuestionDbContext>(
                        (s, options) =>
                        {
                            var dbSettings = s.GetRequiredService<
                                IOptions<QuestionDbContextSettings>
                            >().Value;

                            if (string.IsNullOrEmpty(dbSettings.ConnectionString))
                            {
                                throw new InvalidOperationException(
                                    "SQLite connection string is not configured."
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
            .AddScoped<IQuestionGroupRepository, SqliteQuestionGroupRepository>()
            .AddScoped<IQuestionRepository, SqliteQuestionRepository>()
            .AddScoped<IQuestionGroupService, QuestionGroupService>()
            .AddScoped<IQuestionService, QuestionService>();

        services.AddQuestionCache();

        services.AddMapster();

        TypeAdapterConfig.GlobalSettings.Scan(Assembly.GetExecutingAssembly());

        return services;
    }

    /// <summary>
    /// 生产环境专用：自动应用数据库迁移
    /// </summary>
    public static void ApplyQuestionMigrations(this IApplicationBuilder app)
    {
        using var scope = app.ApplicationServices.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<SqliteQuestionDbContext>();

        if (db.Database.GetPendingMigrations().Any())
        {
            db.Database.Migrate();
        }
    }
}
