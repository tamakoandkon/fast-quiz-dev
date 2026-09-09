using FastQuiz.Quiz.Infrastructure.Repositories;
using FastQuiz.Quiz.Infrastructure.Repositories.MariaDb;
using FastQuiz.Quiz.Infrastructure.Repositories.PostgreSQL;
using FastQuiz.Quiz.Infrastructure.Repositories.Sqlite;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace FastQuiz.Quiz.Test.Infrastructure.Repositories.Utils;

internal static class InitialTestDbHelper
{
    private const string PostgreSqlConnectionString =
        "Host=127.0.0.1;Database=testdb;Username=postgres;Password=postgres";

    private const string MariaDbConnectionString =
        "Server=127.0.0.1;Database=testdb;User=root;Password=root;";

    public static (SqliteConnection, SqliteQuizDbContext) GetTestSqliteQuizDbContext(
        string connectionStr = "Data Source=:memory:"
    )
    {
        var connection = new SqliteConnection(connectionStr);
        connection.Open();

        var options = new DbContextOptionsBuilder<SqliteQuizDbContext>()
            .UseSqlite(connection)
            .Options;

        return (
            connection,
            new SqliteQuizDbContext(
                options,
                Options.Create(new QuizDbContextSettings() { ConnectionString = connectionStr })
            )
        );
    }

    public static PostgreSQLQuizDbContext GetTestPostgreSqlQuizDbContext()
    {
        var options = new DbContextOptionsBuilder<PostgreSQLQuizDbContext>()
            .UseNpgsql(PostgreSqlConnectionString)
            .Options;

        return new PostgreSQLQuizDbContext(
            options,
            Options.Create(
                new QuizDbContextSettings() { ConnectionString = PostgreSqlConnectionString }
            )
        );
    }

    public static MariaDbQuizDbContext GetTestMariaDbQuizDbContext()
    {
        var options = new DbContextOptionsBuilder<MariaDbQuizDbContext>()
            .UseMySql(MariaDbConnectionString, ServerVersion.AutoDetect(MariaDbConnectionString))
            .Options;

        return new MariaDbQuizDbContext(
            options,
            Options.Create(
                new QuizDbContextSettings() { ConnectionString = MariaDbConnectionString }
            )
        );
    }
}
