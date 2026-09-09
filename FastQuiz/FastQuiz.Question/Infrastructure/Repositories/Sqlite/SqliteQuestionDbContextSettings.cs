namespace FastQuiz.Question.Infrastructure.Repositories.Sqlite;

public class SqliteQuestionDbContextSettings
{
    public string ConnectionString { get; set; } = null!;
    public string QuestionTableName { get; set; } = "Questions";
    public string QuestionGroupTableName { get; set; } = "QuestionGroups";
    public string QuestionAnswerTableName { get; set; } = "QuestionAnswers";
}
