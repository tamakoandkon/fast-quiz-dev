using System.Text;
using FastQuiz.Auth.Domain.Repositories;
using System.Security.Claims;
using FastQuiz.Auth.Domain.Services;
using FastQuiz.Auth.Infrastructure.Repositories;
using FastQuiz.Auth.Infrastructure.Repositories.MariaDb;
using FastQuiz.Auth.Infrastructure.Repositories.PostgreSql;
using FastQuiz.Auth.Infrastructure.Repositories.Sqlite;
using FastQuiz.Auth.Infrastructure.Services;
using FastQuiz.Auth.UseCase.Extensions;
using FastQuiz.Auth.UseCase.Services;
using FastQuiz.Auth.UseCase.Validators;
using FastQuiz.Common.Models;
using FluentValidation;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace FastQuiz.Auth.Interface.Extensions;

public static class AuthExtensions
{
    public static IServiceCollection AddAuthServices(
        this IServiceCollection services,
        Action<AuthDbContextSettings> dbConfig,
        Action<JwtTokenSettings> jwtConfig
    )
    {
        services.AddValidatorsFromAssemblyContaining<UserLoginCommandValidator>();

        services.Configure(dbConfig).Configure(jwtConfig);

        var settings = new AuthDbContextSettings();

        dbConfig(settings);

        switch (settings.Type)
        {
            case DatabaseType.Sqlite:
                {
                    services.AddSqliteRepo();
                }
                break;
            case DatabaseType.MariaDb:
                {
                    services.AddMariaDbRepo();
                }
                break;
            case DatabaseType.PostgreSql:
                {
                    services.AddPostgreSqlRepo();
                }
                break;
            default:
                throw new InvalidOperationException(
                    $"不支持的数据库类型：{settings.Type},请在sqlite,mariadb,postgresql中选一个"
                );
        }

        services.AddScoped<IPermissionService, PermissionService>();

        services
            .AddScoped<IPasswordHasher, BCryptPasswordHasher>()
            .AddScoped<ITokenGenerator, TokenGenerator>()
            .AddScoped<IUserApplicationService, UserApplicationService>();

        services
            .AddScoped<ITenantService, TenantService>()
            .AddScoped<IOrganizationService, OrganizationService>()
            .AddScoped<IRoleService, RoleService>();

        services.AddPermissionCache();

        // 2. 配置 JWT 认证
        var jwtSettings = new JwtTokenSettings();
        jwtConfig.Invoke(jwtSettings);
        jwtSettings.Validate();

        services
            .AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(options =>
            {
                options.Events = new JwtBearerEvents
                {
                    OnTokenValidated = async context =>
                    {
                        var userId = context.Principal?.FindFirstValue(ClaimTypes.NameIdentifier);
                        var users = context.HttpContext.RequestServices.GetRequiredService<IUserRepository>();
                        var user = userId is null ? null : await users.FindByIdAsync(userId);
                        var stamp = context.Principal?.FindFirstValue(CredentialStamp.ClaimType);
                        if (user is null || !CredentialStamp.Matches(stamp, user.PasswordHash, jwtSettings.Key))
                            context.Fail("账号凭据已变更，请重新登录");
                    },
                };
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = jwtSettings.Issuer,
                    ValidAudience = jwtSettings.Audience,
                    IssuerSigningKey = new SymmetricSecurityKey(
                        Encoding.UTF8.GetBytes(jwtSettings.Key)
                    ),
                };
            });

        // 3. 启用授权
        services.AddAuthorization();

        return services;
    }

    /// <summary>
    /// 生产环境专用：自动应用数据库迁移
    /// </summary>
    public static void ApplyAuthMigrations(this IApplicationBuilder app)
    {
        using var scope = app.ApplicationServices.CreateScope();
        var sp = scope.ServiceProvider;

        var settings = sp.GetRequiredService<IOptions<AuthDbContextSettings>>().Value;

        DbContext db = settings.Type switch
        {
            DatabaseType.Sqlite => sp.GetRequiredService<SqliteAuthDbContext>(),
            DatabaseType.MariaDb => sp.GetRequiredService<MariaDbAuthDbContext>(),
            DatabaseType.PostgreSql => sp.GetRequiredService<PostgreSqlAuthDbContext>(),
            _ => throw new InvalidOperationException(
                $"不支持的数据库类型：{settings.Type},请在sqlite,mariadb,postgresql中选一个"
            ),
        };

        if (db.Database.GetPendingMigrations().Any())
        {
            db.Database.Migrate();
        }
    }

    public static void MapAuthEndPoints(this IEndpointRouteBuilder app)
    {
        app.MapTenantEndPoints();
        app.MapOrganizationEndPoints();
        app.MapRoleEndPoints();
        app.MapAuthEndpoints();
    }

    private static IServiceCollection AddMariaDbRepo(this IServiceCollection services)
    {
        services.AddDbContext<MariaDbAuthDbContext>(
            (s, options) =>
            {
                var dbSettings = s.GetRequiredService<IOptions<AuthDbContextSettings>>().Value;
                if (string.IsNullOrEmpty(dbSettings.ConnectionString))
                {
                    throw new InvalidOperationException(
                        "MariaDB connection string is not configured."
                    );
                }
                options.UseMySql(
                    dbSettings.ConnectionString,
                    ServerVersion.AutoDetect(dbSettings.ConnectionString)
                );
            }
        );
        return services
            .AddScoped<ITenantRepository, MariaDbTenantRepository>()
            .AddScoped<IOrganizationRepository, MariaDbOrganizationRepository>()
            .AddScoped<IRoleRepository, MariaDbRoleRepository>()
            .AddScoped<IUserRepository, MariaDbUserRepository>();
    }

    private static IServiceCollection AddPostgreSqlRepo(this IServiceCollection services)
    {
        services.AddDbContext<PostgreSqlAuthDbContext>(
            (s, options) =>
            {
                var dbSettings = s.GetRequiredService<IOptions<AuthDbContextSettings>>().Value;
                if (string.IsNullOrEmpty(dbSettings.ConnectionString))
                {
                    throw new InvalidOperationException(
                        "PostgreSQL connection string is not configured."
                    );
                }
                options.UseNpgsql(dbSettings.ConnectionString);
            }
        );
        return services
            .AddScoped<ITenantRepository, PostgreSqlTenantRepository>()
            .AddScoped<IOrganizationRepository, PostgreSqlOrganizationRepository>()
            .AddScoped<IRoleRepository, PostgreSqlRoleRepository>()
            .AddScoped<IUserRepository, PostgreSqlUserRepository>();
    }

    private static IServiceCollection AddSqliteRepo(this IServiceCollection services)
    {
        services.AddDbContext<SqliteAuthDbContext>(
            (s, options) =>
            {
                var dbSettings = s.GetRequiredService<IOptions<AuthDbContextSettings>>().Value;
                if (string.IsNullOrEmpty(dbSettings.ConnectionString))
                {
                    throw new InvalidOperationException(
                        "SQLite connection string is not configured."
                    );
                }
                options.UseSqlite(dbSettings.ConnectionString, x => x.CommandTimeout(30));
            }
        );
        return services
            .AddScoped<ITenantRepository, SqliteTenantRepository>()
            .AddScoped<IOrganizationRepository, SqliteOrganizationRepository>()
            .AddScoped<IRoleRepository, SqliteRoleRepository>()
            .AddScoped<IUserRepository, SqliteUserRepository>();
    }
}
