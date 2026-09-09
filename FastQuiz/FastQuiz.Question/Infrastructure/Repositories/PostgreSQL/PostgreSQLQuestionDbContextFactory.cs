using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Options;

namespace FastQuiz.Question.Infrastructure.Repositories.PostgreSQL;

public class PostgreSqlQuestionDbContextFactory
    : IDesignTimeDbContextFactory<PostgreSqlQuestionDbContext>
{
    public PostgreSqlQuestionDbContext CreateDbContext(string[] args)
    {
        const string designTimeConnectionString =
            "Host=127.0.0.1;Database=testdb;Username=postgres;Password=postgres";

        var optionsBuilder = new DbContextOptionsBuilder<PostgreSqlQuestionDbContext>();
        optionsBuilder.UseNpgsql(designTimeConnectionString);

        return new PostgreSqlQuestionDbContext(
            optionsBuilder.Options,
            Options.Create(
                new QuestionDbContextSettings() { ConnectionString = designTimeConnectionString }
            )
        );
    }
}
