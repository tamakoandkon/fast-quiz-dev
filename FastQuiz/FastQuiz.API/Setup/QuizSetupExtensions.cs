using FastQuiz.Quiz.Infrastructure.Repositories;
using FastQuiz.Quiz.Interface.Setup;

namespace FastQuiz.API.Setup;

public static class QuizSetupExtensions
{
    public static IServiceCollection AddQuiz(
        this IServiceCollection services,
        IConfiguration configuration
    )
    {
        var dbSetting = configuration.GetSection("Quiz").Get<QuizDbContextSettings>();
        services.AddQuizService(db =>
        {
            db.ConnectionString = dbSetting.ConnectionString;
            Console.WriteLine("Auth db connection is {0}", dbSetting.ConnectionString);
        });
        return services;
    }
}
