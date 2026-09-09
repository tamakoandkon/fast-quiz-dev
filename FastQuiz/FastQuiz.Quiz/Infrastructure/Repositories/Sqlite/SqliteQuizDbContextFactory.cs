using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Options;

namespace FastQuiz.Quiz.Infrastructure.Repositories.Sqlite;

public class SqliteQuizDbContextFactory : IDesignTimeDbContextFactory<SqliteQuizDbContext>
{
    public SqliteQuizDbContext CreateDbContext(string[] args)
    {
        const string designTimeConnectionString = "Data Source=designTime_migration.db";

        var optionsBuilder = new DbContextOptionsBuilder<SqliteQuizDbContext>();
        optionsBuilder.UseSqlite(designTimeConnectionString);

        return new SqliteQuizDbContext(
            optionsBuilder.Options,
            Options.Create(
                new QuizDbContextSettings() { ConnectionString = designTimeConnectionString }
            )
        );
    }
}
