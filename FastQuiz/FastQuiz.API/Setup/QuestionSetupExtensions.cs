using FastQuiz.API.Extensions;
using FastQuiz.Question.Interface.Setup;

namespace FastQuiz.API.Setup;

public static class QuestionSetupExtensions
{
    public static IServiceCollection AddQuestion(
        this IServiceCollection services,
        IConfiguration configuration
    )
    {
        var questionDbInfo = configuration.GetQuestionDatabaseConnection();

        services.AddQuestionService(db =>
        {
            db.ConnectionString = questionDbInfo.ConnectionString;
            db.Type = questionDbInfo.Type;
        });

        return services;
    }
}
