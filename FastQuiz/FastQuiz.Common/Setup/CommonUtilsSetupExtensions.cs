using FastQuiz.Common.Exceptions.Handler;
using FastQuiz.Common.Utils;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;

namespace FastQuiz.Common.Setup;

public static class CommonUtilsSetupExtensions
{
    public static IServiceCollection AddCommonUtils(this IServiceCollection services)
    {
        services
            .AddScoped<IClaimsManager, ClaimsManager>()
            .AddSingleton<IIdGenerator, IdGenerator>();
        return services;
    }

    public static IServiceCollection AddGlobalExceptionHandler(this IServiceCollection services)
    {
        return services.AddExceptionHandler<GlobalExceptionHandler>().AddProblemDetails();
    }

    public static void UseGlobalExceptionHandler(this IApplicationBuilder app)
    {
        app.UseExceptionHandler();
    }
}
