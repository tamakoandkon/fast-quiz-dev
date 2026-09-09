using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace FastQuiz.Auth.Infrastructure.Initializer;

public class AuthDbInitializer(IServiceProvider serviceProvider, ILogger<AuthDbInitializer> logger)
    : IHostedService
{
    public async Task StartAsync(CancellationToken cancellationToken)
    {
        logger.LogInformation("Starting auth database initialization...");

        try
        {
            await AuthDbSeedData.InitializeAsync(serviceProvider, cancellationToken);

            logger.LogInformation("Auth database initialization completed successfully.");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "An error occurred while initializing the auth database.");
            throw;
        }
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        logger.LogInformation("Auth database initializer is stopping...");
        return Task.CompletedTask;
    }
}
