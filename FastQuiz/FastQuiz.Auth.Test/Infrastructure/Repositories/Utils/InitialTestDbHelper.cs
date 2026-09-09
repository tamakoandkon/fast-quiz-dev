using FastQuiz.Auth.Infrastructure.Repositories;
using FastQuiz.Auth.Infrastructure.Repositories.MariaDb;
using FastQuiz.Auth.Infrastructure.Repositories.PostgreSql;
using FastQuiz.Auth.Infrastructure.Repositories.Sqlite;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace FastQuiz.Auth.Test.Infrastructure.Repositories.Utils;

internal static class InitialTestDbHelper
{
    public static PostgreSqlAuthDbContext GetTestPostgreSqlAuthDbContext(
        string connectionStr =
            "Host=127.0.0.1;Database=testuserdb;Username=postgres;Password=postgres"
    )
    {
        var options = new DbContextOptionsBuilder<PostgreSqlAuthDbContext>()
            .UseNpgsql(connectionStr)
            .Options;

        var settings = Options.Create(
            new AuthDbContextSettings() { ConnectionString = connectionStr }
        );
        return new PostgreSqlAuthDbContext(options, settings);
    }

    public static MariaDbAuthDbContext GetTestMariaDbAuthDbContext(
        string connectionStr = "Server=127.0.0.1;Database=testauthdb;User=root;Password=root;"
    )
    {
        var options = new DbContextOptionsBuilder<MariaDbAuthDbContext>()
            .UseMySql(connectionStr, ServerVersion.AutoDetect(connectionStr))
            .Options;

        var settings = Options.Create(
            new AuthDbContextSettings() { ConnectionString = connectionStr }
        );

        return new MariaDbAuthDbContext(options, settings);
    }

    public static (SqliteConnection, SqliteAuthDbContext) GetTestSqliteAuthDbContext(
        string connectionStr = "Data Source=:memory:"
    )
    {
        var connection = new SqliteConnection(connectionStr);
        connection.Open();

        var options = new DbContextOptionsBuilder<SqliteAuthDbContext>()
            .UseSqlite(connection)
            .Options;

        return (
            connection,
            new SqliteAuthDbContext(
                options,
                Options.Create(new AuthDbContextSettings() { ConnectionString = connectionStr })
            )
        );
    }
}
