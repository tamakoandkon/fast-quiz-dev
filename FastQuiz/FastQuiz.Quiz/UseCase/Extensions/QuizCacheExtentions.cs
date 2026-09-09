using EasyCaching.Core;
using FastQuiz.Auth.UseCase.Services;
using Microsoft.Extensions.DependencyInjection;

namespace FastQuiz.Quiz.UseCase.Extensions;

internal static class QuizCacheExtentions
{
    private const string CacheName = "Quiz:QuestionSnapshotList:Memory";

    public static IServiceCollection AddQuizCache(this IServiceCollection services)
    {
        services.AddEasyCaching(options =>
        {
            options.UseInMemory(
                config =>
                {
                    config.DBConfig.SizeLimit = 1000;
                },
                CacheName
            );
        });

        services.AddScoped<IPermissionService, PermissionService>();
        return services;
    }

    internal static IEasyCachingProvider GetQuestionSnapshotListCacheProvider(
        this IEasyCachingProviderFactory factory
    )
    {
        return factory.GetCachingProvider(CacheName);
    }
}
