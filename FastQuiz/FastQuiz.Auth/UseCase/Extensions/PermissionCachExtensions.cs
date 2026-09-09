using EasyCaching.Core;
using FastQuiz.Auth.UseCase.Services;
using Microsoft.Extensions.DependencyInjection;

namespace FastQuiz.Auth.UseCase.Extensions;

public static class PermissionCacheExtensions
{
    private const string CacheName = "Auth:Permission:Memory";

    public static IServiceCollection AddPermissionCache(this IServiceCollection services)
    {
        services.AddEasyCaching(options =>
        {
            options.UseInMemory(
                config =>
                {
                    config.DBConfig.SizeLimit = 50000;
                },
                CacheName
            );
        });

        services.AddScoped<IPermissionService, PermissionService>();
        return services;
    }

    internal static IEasyCachingProvider GetPermissionCacheProvider(
        this IEasyCachingProviderFactory factory
    )
    {
        return factory.GetCachingProvider(CacheName);
    }
}
