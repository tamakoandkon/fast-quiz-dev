namespace FastQuiz.Question.Infrastructure.Repositories;

public class QuestionDbContextSettings
{
    public string ConnectionString { get; set; } = null!;
    public string Type { get; set; } = "sqlite";
    public string QuestionTableName { get; set; } = "Questions";
    public string QuestionGroupTableName { get; set; } = "QuestionGroups";
    public string QuestionAnswerTableName { get; set; } = "QuestionAnswers";
}
