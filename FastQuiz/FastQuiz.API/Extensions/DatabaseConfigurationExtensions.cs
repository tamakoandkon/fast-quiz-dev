using FastQuiz.API.Models;
using FastQuiz.Common.Models;

namespace FastQuiz.API.Extensions;

public static class DatabaseConfigurationExtensions
{
    /// <summary>
    /// 获取认证模块数据库连接信息。
    /// </summary>
    /// <param name="configuration"></param>
    /// <returns></returns>
    public static DatabaseConnectionInfo GetAuthDatabaseConnection(
        this IConfiguration configuration
    ) => configuration.GetDatabaseConnection("Auth");

    /// <summary>
    /// 获取题库模块数据库连接信息。
    /// </summary>
    /// <param name="configuration"></param>
    /// <returns></returns>
    public static DatabaseConnectionInfo GetQuestionDatabaseConnection(
        this IConfiguration configuration
    ) => configuration.GetDatabaseConnection("Question");

    /// <summary>
    /// 获取测验模块数据库连接信息。
    /// </summary>
    /// <param name="configuration"></param>
    /// <returns></returns>
    public static DatabaseConnectionInfo GetQuizDatabaseConnection(
        this IConfiguration configuration
    ) => configuration.GetDatabaseConnection("Quiz");

    /// <summary>
    /// 获取指定模块数据库连接信息。
    /// 模块节点不存在时回退默认；节点存在但配置非法时抛异常。
    /// </summary>
    public static DatabaseConnectionInfo GetDatabaseConnection(
        this IConfiguration configuration,
        string moduleName
    )
    {
        var section = configuration.GetSection(moduleName);

        if (!section.Exists())
        {
            return configuration.GetDefaultDatabaseConnection();
        }

        return Parse(section, moduleName);
    }

    /// <summary>
    /// 获取默认数据库连接信息。
    /// </summary>
    /// <param name="configuration"></param>
    /// <returns></returns>
    public static DatabaseConnectionInfo GetDefaultDatabaseConnection(
        this IConfiguration configuration
    )
    {
        var section = configuration.GetSection("Database:Default");

        return Parse(section, "Database:Default");
    }

    /// <summary>
    /// 解析一个配置节点，非法直接抛异常
    /// </summary>
    private static DatabaseConnectionInfo Parse(IConfigurationSection section, string path)
    {
        if (!section.Exists())
        {
            throw new InvalidOperationException($"{path} 配置无效：配置节点不存在。");
        }

        var typeStr = section["Type"];
        if (string.IsNullOrWhiteSpace(typeStr))
        {
            throw new InvalidOperationException($"{path} 配置无效：缺少 Type，或 Type 为空。");
        }

        var connStr = section["ConnectionString"];
        if (string.IsNullOrWhiteSpace(connStr))
        {
            throw new InvalidOperationException(
                $"{path} 配置无效：缺少 ConnectionString，或 ConnectionString 为空。"
            );
        }

        var type = ParseType(typeStr);

        return new DatabaseConnectionInfo(type, connStr);
    }

    private static DatabaseType ParseType(string type) =>
        type.ToLowerInvariant() switch
        {
            "sqlite" => DatabaseType.Sqlite,
            "postgresql" => DatabaseType.PostgreSql,
            "mariadb" => DatabaseType.MariaDb,
            _ => throw new InvalidOperationException($"不支持的数据库类型: {type}"),
        };
}
