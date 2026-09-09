using FastQuiz.Auth.Infrastructure.Initializer;
using FastQuiz.Auth.Infrastructure.Repositories;
using FastQuiz.Auth.Infrastructure.Services;
using FastQuiz.Auth.Interface.Extensions;

namespace FastQuiz.API.Setup;

public static class AuthSetupExtensions
{
    public static IServiceCollection AddAuth(
        this IServiceCollection services,
        IConfiguration configuration,
        IWebHostEnvironment env
    )
    {
        var dbSetting =
            configuration.GetSection("Auth").Get<AuthDbContextSettings>()
            ?? throw new InvalidOperationException("Auth配置不可为空");
        var jwtSetting =
            configuration.GetSection("Jwt").Get<JwtTokenSettings>()
            ?? throw new InvalidOperationException("Jwt配置不可为空");

        services.AddAuthServices(
            db =>
            {
                db.ConnectionString = dbSetting.ConnectionString;
                db.UserTableName = dbSetting.UserTableName;
                db.Type = dbSetting.Type;
            },
            jwt =>
            {
                jwt.Issuer = jwtSetting.Issuer;
                jwt.Audience = jwtSetting.Audience;
                jwt.Key = jwtSetting.Key;
            }
        );

        services.AddHostedService<AuthDbInitializer>();

        return services;
    }
}
