using EasyCaching.Core;
using FastQuiz.Auth.UseCase.Services;
using Microsoft.Extensions.DependencyInjection;

namespace FastQuiz.Quiz.UseCase.Extensions;

internal static class QuizCacheExtentions
{
    private const string CacheName = "Quiz:QuestionSnapshotList:Memory";
    private const string QuizCacheName = "Quiz:tmp:Memory";

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

        services.AddEasyCaching(options =>
        {
            options.UseInMemory(
                config =>
                {
                    config.DBConfig.SizeLimit = 100000;
                },
                QuizCacheName
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

    internal static IEasyCachingProvider GetQuizTmpCacheProvider(
        this IEasyCachingProviderFactory factory
    )
    {
        return factory.GetCachingProvider(QuizCacheName);
    }
}
