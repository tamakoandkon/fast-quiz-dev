using FastQuiz.Question.Infrastructure.Repositories;
using FastQuiz.Question.Interface.Setup;

namespace FastQuiz.API.Setup;

public static class QuestionSetupExtensions
{
    public static IServiceCollection AddQuestion(
        this IServiceCollection services,
        IConfiguration configuration
    )
    {
        var dbSetting = configuration.GetSection("Question").Get<QuestionDbContextSettings>();
        services.AddQuestionService(db =>
        {
            db.ConnectionString = dbSetting.ConnectionString;
            db.Type = dbSetting.Type;
            Console.WriteLine(
                "Auth db connection is {0}, Question table name is {1}, Question group table name is {2}, Question answer table name is {3}",
                dbSetting.ConnectionString,
                dbSetting.QuestionTableName,
                dbSetting.QuestionGroupTableName,
                dbSetting.QuestionAnswerTableName
            );
        });

        return services;
    }
}
