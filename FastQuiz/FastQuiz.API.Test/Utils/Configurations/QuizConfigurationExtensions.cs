using FastQuiz.Quiz.Infrastructure.Repositories;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;

namespace FastQuiz.API.Test.Utils.Configurations;

internal static class QuizConfigurationExtensions
{
    internal static IWebHostBuilder ConfigureQuiz(this IWebHostBuilder builder)
    {
        string baseDirectory = AppContext.BaseDirectory;
        string dbPath = Path.Combine(baseDirectory, "data", "quiz.db");

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
                            new QuizDbContextSettings
                            {
                                ConnectionString = $"Data Source={dbPath};Cache=Shared",
                            }
                        )
                    )
                );
            }
        );

        return builder;
    }
}
