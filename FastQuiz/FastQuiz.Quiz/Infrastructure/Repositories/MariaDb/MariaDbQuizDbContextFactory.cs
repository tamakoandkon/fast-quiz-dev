using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Options;

namespace FastQuiz.Quiz.Infrastructure.Repositories.MariaDb;

public class MariaDbQuizDbContextFactory : IDesignTimeDbContextFactory<MariaDbQuizDbContext>
{
    public MariaDbQuizDbContext CreateDbContext(string[] args)
    {
        const string designTimeConnectionString =
            "Server=127.0.0.1;Database=testdb;User=root;Password=root;";

        var optionsBuilder = new DbContextOptionsBuilder<MariaDbQuizDbContext>();
        optionsBuilder.UseMySql(
            designTimeConnectionString,
            ServerVersion.AutoDetect(designTimeConnectionString)
        );

        return new MariaDbQuizDbContext(
            optionsBuilder.Options,
            Options.Create(
                new QuizDbContextSettings() { ConnectionString = designTimeConnectionString }
            )
        );
    }
}
