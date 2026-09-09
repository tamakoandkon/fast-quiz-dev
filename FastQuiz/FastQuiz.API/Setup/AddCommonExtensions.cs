using FastQuiz.Common.Setup;

namespace FastQuiz.API.Setup;

public static class AddCommonExtensions
{
    public static IServiceCollection AddCommon(this IServiceCollection services)
    {
        return services.AddCommonUtils();
    }
}
