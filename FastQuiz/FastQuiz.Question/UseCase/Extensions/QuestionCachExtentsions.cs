using EasyCaching.Core;
using FastQuiz.Auth.UseCase.Services;
using Microsoft.Extensions.DependencyInjection;

namespace FastQuiz.Question.UseCase.Extensions;

internal static class QuestionCachExtentsions
{
    private const string CacheName = "Question:IdList:Memory";

    public static IServiceCollection AddQuestionCache(this IServiceCollection services)
    {
        services.AddEasyCaching(options =>
        {
            options.UseInMemory(
                config =>
                {
                    config.DBConfig.SizeLimit = 300;
                },
                CacheName
            );
        });

        services.AddScoped<IPermissionService, PermissionService>();
        return services;
    }

    internal static IEasyCachingProvider GetQuestionCacheProvider(
        this IEasyCachingProviderFactory factory
    )
    {
        return factory.GetCachingProvider(CacheName);
    }
}
