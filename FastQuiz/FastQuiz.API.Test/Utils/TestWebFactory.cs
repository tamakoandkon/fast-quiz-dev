using FastQuiz.API.Test.Utils.Configurations;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using System.Security.Cryptography;
using Microsoft.AspNetCore.Mvc.Testing;

namespace FastQuiz.API.Test.Utils;

internal class TestWebAppFactory : WebApplicationFactory<Program>
{
    protected override IHost CreateHost(IHostBuilder builder)
    {
        // Minimal hosting reads registration-time settings before ConfigureWebHost callbacks.
        builder.ConfigureHostConfiguration(config =>
            config.AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Jwt:Key"] = Convert.ToBase64String(RandomNumberGenerator.GetBytes(32)),
                ["BootstrapAdmin:Enabled"] = "true",
                ["BootstrapAdmin:Password"] = Convert.ToHexString(RandomNumberGenerator.GetBytes(8)),
                ["Migrations:RunMigrationsOnStartup"] = "true",
                ["Hangfire:Type"] = "memory",
            }));
        return base.CreateHost(builder);
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Development").ConfigureAuth().ConfigureQuestion().ConfigureQuiz();
    }
}
