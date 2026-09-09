using FastQuiz.Question.Infrastructure.Repositories.Sqlite;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;

namespace FastQuiz.API.Test.Utils.Configurations;

internal static class QuestionConfigurationExtensinos
{
    internal static IWebHostBuilder ConfigureQuestion(this IWebHostBuilder builder)
    {
        string baseDirectory = AppContext.BaseDirectory;
        string dbPath = Path.Combine(baseDirectory, "data", "question.db");
        Console.WriteLine("DBPATH" + dbPath);

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
                            new SqliteQuestionDbContextSettings
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
