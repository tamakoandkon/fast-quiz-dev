using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Options;

namespace FastQuiz.Auth.Infrastructure.Repositories.MariaDb;

public class MariaDbAuthDbContextFactory : IDesignTimeDbContextFactory<MariaDbAuthDbContext>
{
    public MariaDbAuthDbContext CreateDbContext(string[] args)
    {
        const string designTimeConnectionString =
            "Server=127.0.0.1;Database=testdb;User=root;Password=root;";

        var optionsBuilder = new DbContextOptionsBuilder<MariaDbAuthDbContext>();
        optionsBuilder.UseMySql(
            designTimeConnectionString,
            ServerVersion.AutoDetect(designTimeConnectionString)
        );

        return new MariaDbAuthDbContext(
            optionsBuilder.Options,
            Options.Create(
                new AuthDbContextSettings() { ConnectionString = designTimeConnectionString }
            )
        );
    }
}
