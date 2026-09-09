using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Options;

namespace FastQuiz.Question.Infrastructure.Repositories.Sqlite;

public class SqliteQuestionDbContextFactory : IDesignTimeDbContextFactory<SqliteQuestionDbContext>
{
    public SqliteQuestionDbContext CreateDbContext(string[] args)
    {
        const string designTimeConnectionString = "Data Source=designTime_migration.db";

        var optionsBuilder = new DbContextOptionsBuilder<SqliteQuestionDbContext>();
        optionsBuilder.UseSqlite(designTimeConnectionString);

        return new SqliteQuestionDbContext(
            optionsBuilder.Options,
            Options.Create(
                new QuestionDbContextSettings() { ConnectionString = designTimeConnectionString }
            )
        );
    }
}
