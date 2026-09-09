using FastQuiz.Auth.Infrastructure.Repositories;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;

namespace FastQuiz.API.Test.Utils.Configurations;

internal static class AuthConfigurationExtensions
{
    internal static IWebHostBuilder ConfigureAuth(this IWebHostBuilder builder)
    {
        string baseDirectory = AppContext.BaseDirectory;
        string dbPath = Path.Combine(baseDirectory, "data", "auth.db");

        string? directory = Path.GetDirectoryName(dbPath);
        if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
        {
            Directory.CreateDirectory(directory);
        }

        builder.ConfigureServices(
            (context, services) =>
            {
                services.Replace(
                    ServiceDescriptor.Singleton(
                        Options.Create(
                            new AuthDbContextSettings
                            {
                                ConnectionString = $"Data Source={dbPath};Cache=Shared",
                            }
                        )
                    )
                );
            }
        );

        builder.ConfigureServices(services =>
        {
            services
                .AddAuthentication("TestScheme")
                .AddScheme<AuthenticationSchemeOptions, TestAuthHandler>("TestScheme", _ => { });

            services.PostConfigure<AuthenticationOptions>(options =>
            {
                options.DefaultAuthenticateScheme = "TestScheme";
                options.DefaultChallengeScheme = "TestScheme";
                options.DefaultScheme = "TestScheme";
            });
        });

        return builder;
    }
}
