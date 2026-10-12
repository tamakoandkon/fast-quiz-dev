using Hangfire;
using Hangfire.MySql;
using Hangfire.PostgreSql;
using Hangfire.Storage.SQLite;

namespace FastQuiz.API.Setup;

public static class AddHangfireServiceExtensions
{
    public static IServiceCollection AddHangfireServcie(
        this IServiceCollection services,
        IConfiguration configuration
    )
    {
        services.AddHangfire(config =>
        {
            config
                .SetDataCompatibilityLevel(CompatibilityLevel.Version_180)
                .UseSimpleAssemblyNameTypeSerializer()
                .UseRecommendedSerializerSettings();

            var provider = configuration["Hangfire:Type"]?.ToLowerInvariant();
            var conn = configuration.GetSection("Hangfire:ConnectionString").Get<string>();

            switch (provider)
            {
                case "memory":
                    config.UseInMemoryStorage();
                    break;
                case "sqlite":
                    var sqlitePath = conn ?? "hangfire.db";
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
