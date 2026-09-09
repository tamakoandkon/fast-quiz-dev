using System.Text;
using FastQuiz.Auth.Domain.Repositories;
using FastQuiz.Auth.Domain.Services;
using FastQuiz.Auth.Infrastructure.Repositories;
using FastQuiz.Auth.Infrastructure.Repositories.MariaDb;
using FastQuiz.Auth.Infrastructure.Repositories.PostgreSql;
using FastQuiz.Auth.Infrastructure.Repositories.Sqlite;
using FastQuiz.Auth.Infrastructure.Services;
using FastQuiz.Auth.UseCase.Extensions;
using FastQuiz.Auth.UseCase.Services;
using FastQuiz.Auth.UseCase.Validators;
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
            case "sqlite":
                {
                    services.AddDbContext<SqliteAuthDbContext>(
                        (s, options) =>
                        {
                            var dbSettings = s.GetRequiredService<
                                IOptions<AuthDbContextSettings>
                            >().Value;

                            if (string.IsNullOrEmpty(dbSettings.ConnectionString))
                            {
                                throw new InvalidOperationException(
                                    "SQLite connection string is not configured."
                                );
                            }

                            options.UseSqlite(
                                dbSettings.ConnectionString,
                                x =>
                                {
                                    x.CommandTimeout(30);
                                }
                            );
                        }
                    );
                }
                break;
            case "mariadb":
                {
                    services.AddDbContext<MariaDbAuthDbContext>(
                        (s, options) =>
                        {
                            var dbSettings = s.GetRequiredService<
                                IOptions<AuthDbContextSettings>
                            >().Value;

                            if (string.IsNullOrEmpty(dbSettings.ConnectionString))
                            {
                                throw new InvalidOperationException(
                                    "SQLite connection string is not configured."
                                );
                            }

                            options.UseMySql(
                                dbSettings.ConnectionString,
                                ServerVersion.AutoDetect(dbSettings.ConnectionString)
                            );
                        }
                    );
                }
                break;
            case "postgresql":
                {
                    services.AddDbContext<PostgreSqlAuthDbContext>(
                        (s, options) =>
                        {
                            var dbSettings = s.GetRequiredService<
                                IOptions<AuthDbContextSettings>
                            >().Value;

                            if (string.IsNullOrEmpty(dbSettings.ConnectionString))
                            {
                                throw new InvalidOperationException(
                                    "SQLite connection string is not configured."
                                );
                            }

                            options.UseNpgsql(dbSettings.ConnectionString);
                        }
                    );
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
            .AddScoped<IUserRepository, SqliteUserRepository>()
            .AddScoped<IUserApplicationService, UserApplicationService>();

        services
            .AddScoped<ITenantRepository, SqliteTenantRepository>()
            .AddScoped<IOrganizationRepository, SqliteOrganizationRepository>()
            .AddScoped<IRoleRepository, SqliteRoleRepository>()
            .AddScoped<ITenantService, TenantService>()
            .AddScoped<IOrganizationService, OrganizationService>()
            .AddScoped<IRoleService, RoleService>();

        services.AddPermissionCache();

        // 2. 配置 JWT 认证
        var jwtSettings = new JwtTokenSettings();
        jwtConfig.Invoke(jwtSettings);

        services
            .AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(options =>
            {
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
        var db = scope.ServiceProvider.GetRequiredService<SqliteAuthDbContext>();

        if (db.Database.GetPendingMigrations().Any())
        {
            db.Database.Migrate();
        }
    }

    public static void MapAuthApi(this IEndpointRouteBuilder app)
    {
        app.MapTenantApi();
        app.MapOrganizationApi();
        app.MapRoleApi();
        app.MapAuthEndpoints();
    }
}
