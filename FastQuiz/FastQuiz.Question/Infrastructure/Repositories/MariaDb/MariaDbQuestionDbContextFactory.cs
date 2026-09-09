using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Options;

namespace FastQuiz.Question.Infrastructure.Repositories.MariaDb;

public class MariaDbQuestionDbContextFactory : IDesignTimeDbContextFactory<MariaDbQuestionDbContext>
{
    public MariaDbQuestionDbContext CreateDbContext(string[] args)
    {
        const string designTimeConnectionString =
            "Server=127.0.0.1;Database=testdb;User=root;Password=root;";

        var optionsBuilder = new DbContextOptionsBuilder<MariaDbQuestionDbContext>();
        optionsBuilder.UseMySql(
            designTimeConnectionString,
            ServerVersion.AutoDetect(designTimeConnectionString)
        );

        return new MariaDbQuestionDbContext(
            optionsBuilder.Options,
            Options.Create(
                new QuestionDbContextSettings() { ConnectionString = designTimeConnectionString }
            )
        );
    }
}
