using FastQuiz.API.Extensions;
using FastQuiz.API.Models;
using FastQuiz.Common.Models;

namespace FastQuiz.API.Setup;

public static class AddHealthCheckService
{
    public static IServiceCollection AddHealthCheckServices(
        this IServiceCollection services,
        IConfiguration configuration
    )
    {
        var builder = services
            .AddHealthChecks()
            .AddAuthDatabaseHealthCheck(configuration.GetAuthDatabaseConnection())
            .AddQuestionDatabaseHealthCheck(configuration.GetQuestionDatabaseConnection())
            .AddQuizDatabaseHealthCheck(configuration.GetQuizDatabaseConnection())
            .AddHangfireHealthCheck(5);

        return services;
    }

    private static IHealthChecksBuilder AddAuthDatabaseHealthCheck(
        this IHealthChecksBuilder builder,
        DatabaseConnectionInfo authDbInfo
    )
    {
        switch (authDbInfo.Type)
        {
            case DatabaseType.PostgreSql:
                builder.AddNpgSql(authDbInfo.ConnectionString, name: "AuthDB");
                break;
            case DatabaseType.MariaDb:
                builder.AddMySql(authDbInfo.ConnectionString, name: "AuthDB");
                break;
            case DatabaseType.Sqlite:
                builder.AddSqlite(authDbInfo.ConnectionString, name: "AuthDB");
                break;
            default:
                throw new NotSupportedException(
                    $"Database type {authDbInfo.Type} is not supported for health checks."
                );
        }

        return builder;
    }

    private static IHealthChecksBuilder AddQuestionDatabaseHealthCheck(
        this IHealthChecksBuilder builder,
        DatabaseConnectionInfo authDbInfo
    )
    {
        switch (authDbInfo.Type)
        {
            case DatabaseType.PostgreSql:
                builder.AddNpgSql(authDbInfo.ConnectionString, name: "QuestionDB");
                break;
            case DatabaseType.MariaDb:
                builder.AddMySql(authDbInfo.ConnectionString, name: "QuestionDB");
                break;
            case DatabaseType.Sqlite:
                builder.AddSqlite(authDbInfo.ConnectionString, name: "QuestionDB");
                break;
            default:
                throw new NotSupportedException(
                    $"Database type {authDbInfo.Type} is not supported for health checks."
                );
        }

        return builder;
    }

    private static IHealthChecksBuilder AddQuizDatabaseHealthCheck(
        this IHealthChecksBuilder builder,
        DatabaseConnectionInfo quizDbInfo
    )
    {
        switch (quizDbInfo.Type)
        {
            case DatabaseType.PostgreSql:
                builder.AddNpgSql(quizDbInfo.ConnectionString, name: "QuizDB");
                break;
            case DatabaseType.MariaDb:
                builder.AddMySql(quizDbInfo.ConnectionString, name: "QuizDB");
                break;
            case DatabaseType.Sqlite:
                builder.AddSqlite(quizDbInfo.ConnectionString, name: "QuizDB");
                break;
            default:
                throw new NotSupportedException(
                    $"Database type {quizDbInfo.Type} is not supported for health checks."
                );
        }
        return builder;
    }

    private static IHealthChecksBuilder AddHangfireHealthCheck(
        this IHealthChecksBuilder builder,
        int maxFailureCount
    )
    {
        builder.AddHangfire(options =>
        {
            options.MaximumJobsFailed = maxFailureCount;
        });
        return builder;
    }
}
