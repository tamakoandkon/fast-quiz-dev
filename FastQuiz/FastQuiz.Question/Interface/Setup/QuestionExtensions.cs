using System.Reflection;
using FastQuiz.Common.Models;
using FastQuiz.Question.Domain.Repositories;
using FastQuiz.Question.Domain.Servcies;
using FastQuiz.Question.Infrastructure.Repositories;
using FastQuiz.Question.Infrastructure.Repositories.MariaDb;
using FastQuiz.Question.Infrastructure.Repositories.PostgreSQL;
using FastQuiz.Question.Infrastructure.Repositories.Sqlite;
using FastQuiz.Question.Infrastructure.Services;
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
            case DatabaseType.Sqlite:
                {
                    services.AddSqliteRepo();
                }
                break;
            case DatabaseType.MariaDb:
                {
                    services.AddMariaDbRepo();
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
            .AddScoped<IQuestionGroupService, QuestionGroupService>()
            .AddScoped<IQuestionService, QuestionService>()
            .AddScoped<IQuestionRandomPicker, QuestionRandomPicker>();

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
        var sp = scope.ServiceProvider;

        var settings = sp.GetRequiredService<IOptions<QuestionDbContextSettings>>().Value;

        DbContext db = settings.Type switch
        {
            DatabaseType.Sqlite => sp.GetRequiredService<SqliteQuestionDbContext>(),
            DatabaseType.MariaDb => sp.GetRequiredService<MariaDbQuestionDbContext>(),
            DatabaseType.PostgreSql => sp.GetRequiredService<PostgreSqlQuestionDbContext>(),
            _ => throw new InvalidOperationException(
                $"不支持的数据库类型：{settings.Type},请在sqlite,mariadb,postgresql中选一个"
            ),
        };

        if (db.Database.GetPendingMigrations().Any())
        {
            db.Database.Migrate();
        }
    }

    private static IServiceCollection AddSqliteRepo(this IServiceCollection services)
    {
        services.AddDbContext<SqliteQuestionDbContext>(
            (s, options) =>
            {
                var dbSettings = s.GetRequiredService<IOptions<QuestionDbContextSettings>>().Value;

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
            .AddScoped<IQuestionGroupRepository, SqliteQuestionGroupRepository>()
            .AddScoped<IQuestionRepository, SqliteQuestionRepository>();
    }

    private static IServiceCollection AddMariaDbRepo(this IServiceCollection services)
    {
        services.AddDbContext<MariaDbQuestionDbContext>(
            (s, options) =>
            {
                var dbSettings = s.GetRequiredService<IOptions<QuestionDbContextSettings>>().Value;
                if (string.IsNullOrEmpty(dbSettings.ConnectionString))
                {
                    throw new InvalidOperationException(
                        "MariaDB connection string is not configured."
                    );
                }
                options.UseMySql(
                    dbSettings.ConnectionString,
                    ServerVersion.AutoDetect(dbSettings.ConnectionString),
                    x =>
                    {
                        x.CommandTimeout(30);
                    }
                );
            }
        );
        return services
            .AddScoped<IQuestionGroupRepository, MariaDbQuestionGroupRepository>()
            .AddScoped<IQuestionRepository, MariaDbQuestionRepository>();
    }

    private static IServiceCollection AddPostgreSqlRepo(this IServiceCollection services)
    {
        services.AddDbContext<PostgreSqlQuestionDbContext>(
            (s, options) =>
            {
                var dbSettings = s.GetRequiredService<IOptions<QuestionDbContextSettings>>().Value;
                if (string.IsNullOrEmpty(dbSettings.ConnectionString))
                {
                    throw new InvalidOperationException(
                        "PostgreSQL connection string is not configured."
                    );
                }
                options.UseNpgsql(
                    dbSettings.ConnectionString,
                    x =>
                    {
                        x.CommandTimeout(30);
                    }
                );
            }
        );
        return services
            .AddScoped<IQuestionGroupRepository, PostgreSqlQuestionGroupRepository>()
            .AddScoped<IQuestionRepository, PostgreSqlQuestionRepository>();
    }
}
