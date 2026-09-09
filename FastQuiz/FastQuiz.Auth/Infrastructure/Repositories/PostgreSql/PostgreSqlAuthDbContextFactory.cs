using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Options;

namespace FastQuiz.Auth.Infrastructure.Repositories.PostgreSql;

public class PostgreSqlAuthDbContextFactory : IDesignTimeDbContextFactory<PostgreSqlAuthDbContext>
{
    public PostgreSqlAuthDbContext CreateDbContext(string[] args)
    {
        const string designTimeConnectionString =
            "Host=127.0.0.1;Database=testdb;Username=postgres;Password=postgres";

        var optionsBuilder = new DbContextOptionsBuilder<PostgreSqlAuthDbContext>();
        optionsBuilder.UseNpgsql(designTimeConnectionString);

        return new PostgreSqlAuthDbContext(
            optionsBuilder.Options,
            Options.Create(
                new AuthDbContextSettings() { ConnectionString = designTimeConnectionString }
            )
        );
    }
}
