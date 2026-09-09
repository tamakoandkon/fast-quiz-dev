using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Options;

namespace FastQuiz.Auth.Infrastructure.Repositories.Sqlite;

public class SqliteAuthDbContextFactory : IDesignTimeDbContextFactory<SqliteAuthDbContext>
{
    public SqliteAuthDbContext CreateDbContext(string[] args)
    {
        const string designTimeConnectionString = "Data Source=designTime_migration.db";

        var optionsBuilder = new DbContextOptionsBuilder<SqliteAuthDbContext>();
        optionsBuilder.UseSqlite(designTimeConnectionString);

        return new SqliteAuthDbContext(
            optionsBuilder.Options,
            Options.Create(
                new AuthDbContextSettings() { ConnectionString = designTimeConnectionString }
            )
        );
    }
}
