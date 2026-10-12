using FastQuiz.API.Extensions;
using FastQuiz.Quiz.Interface.Setup;

namespace FastQuiz.API.Setup;

public static class QuizSetupExtensions
{
    public static IServiceCollection AddQuiz(
        this IServiceCollection services,
        IConfiguration configuration
    )
    {
        var quizDbInfo = configuration.GetQuizDatabaseConnection();
        services.AddQuizService(db =>
        {
            db.ConnectionString = quizDbInfo.ConnectionString;
            db.Type = quizDbInfo.Type;
        });
        return services;
    }
}
