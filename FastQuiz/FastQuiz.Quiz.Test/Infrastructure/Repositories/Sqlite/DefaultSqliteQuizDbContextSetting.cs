using FastQuiz.Quiz.Infrastructure.Repositories;

namespace FastQuiz.Quiz.Test.Infrastructure.Repositories.Sqlite;

internal static class DefaultSqliteQuizDbContextSetting
{
    public static QuizDbContextSettings GetDefault()
    {
        return new QuizDbContextSettings { ConnectionString = "Data Source=:memory:" };
    }
}
