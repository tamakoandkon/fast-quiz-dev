using FastQuiz.API.Extensions;
using FastQuiz.Auth.Infrastructure.Initializer;
using FastQuiz.Auth.Infrastructure.Services;
using FastQuiz.Auth.Interface.Extensions;

namespace FastQuiz.API.Setup;

public static class AuthSetupExtensions
{
    public static IServiceCollection AddAuth(
        this IServiceCollection services,
        IConfiguration configuration
    )
    {
        var authDbInfo = configuration.GetAuthDatabaseConnection();

        var jwtSetting =
            configuration.GetSection("Jwt").Get<JwtTokenSettings>()
            ?? throw new InvalidOperationException("Jwt配置不可为空");

        services.AddAuthServices(
            db =>
            {
                db.ConnectionString = authDbInfo.ConnectionString;
                db.Type = authDbInfo.Type;
            },
            jwt =>
            {
                jwt.Issuer = jwtSetting.Issuer;
                jwt.Audience = jwtSetting.Audience;
                jwt.Key = jwtSetting.Key;
            }
        );

        services.Configure<BootstrapAdminSettings>(configuration.GetSection("BootstrapAdmin"));
        if (configuration.GetValue<bool>("BootstrapAdmin:Enabled"))
            services.AddHostedService<AuthDbInitializer>();

        return services;
    }
}
