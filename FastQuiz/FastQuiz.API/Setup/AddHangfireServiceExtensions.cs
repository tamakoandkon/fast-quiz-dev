using Hangfire;
using Hangfire.MySql;
using Hangfire.PostgreSql;
using Hangfire.Storage.SQLite;

namespace FastQuiz.API.Setup;

public static class AddHangfireServiceExtensions
{
    public static IServiceCollection AddHangfireServcie(
        this IServiceCollection services,
        IConfiguration configuration,
        IHostEnvironment environment
    )
    {
        var provider = configuration["Hangfire:Type"]?.ToLowerInvariant() ?? "sqlite";
        var conn = configuration["Hangfire:ConnectionString"];
        if (provider == "memory" && !environment.IsDevelopment())
            throw new InvalidOperationException("In-memory Hangfire storage is only allowed in Development.");
        if (provider is "mariadb" or "postgresql" && string.IsNullOrWhiteSpace(conn))
            throw new InvalidOperationException("Hangfire:ConnectionString is required for the selected database.");

        if (provider == "sqlite" && conn?.Contains('=') == true)
            throw new InvalidOperationException("SQLite Hangfire storage requires a file path, not a connection string.");

        services.AddHangfire(config =>
        {
            config
                .SetDataCompatibilityLevel(CompatibilityLevel.Version_180)
                .UseSimpleAssemblyNameTypeSerializer()
                .UseRecommendedSerializerSettings();

            switch (provider)
            {
                case "memory":
                    config.UseInMemoryStorage();
                    break;
                case "sqlite":
                    // This provider expects a filename, not an ADO.NET connection string.
                    var sqlitePath = Path.GetFullPath(conn ?? "data/hangfire.db");
                    Directory.CreateDirectory(Path.GetDirectoryName(sqlitePath)!);
                    config.UseSQLiteStorage(sqlitePath);
                    break;
                case "mariadb":
                    config.UseStorage(new MySqlStorage(conn, new MySqlStorageOptions()));
                    break;
                case "postgresql":
                    config.UsePostgreSqlStorage(opts => opts.UseNpgsqlConnection(conn));
                    break;

                default:
                    throw new InvalidOperationException($"不支持的 Hangfire 存储类型：{provider}");
            }
        });

        services.AddHangfireServer();
        return services;
    }
}
