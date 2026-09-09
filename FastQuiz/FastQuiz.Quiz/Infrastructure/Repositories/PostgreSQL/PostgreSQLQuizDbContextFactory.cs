using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Options;

namespace FastQuiz.Quiz.Infrastructure.Repositories.PostgreSQL;

public class PostgreSQLQuizDbContextFactory : IDesignTimeDbContextFactory<PostgreSQLQuizDbContext>
{
    public PostgreSQLQuizDbContext CreateDbContext(string[] args)
    {
        const string designTimeConnectionString =
            "Host=127.0.0.1;Database=testdb;Username=postgres;Password=postgres";

        var optionsBuilder = new DbContextOptionsBuilder<PostgreSQLQuizDbContext>();
        optionsBuilder.UseNpgsql(designTimeConnectionString);

        return new PostgreSQLQuizDbContext(
            optionsBuilder.Options,
            Options.Create(
                new QuizDbContextSettings() { ConnectionString = designTimeConnectionString }
            )
        );
    }
}
