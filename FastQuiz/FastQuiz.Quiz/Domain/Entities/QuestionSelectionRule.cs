using FastQuiz.Question.Domain.Entities;

namespace FastQuiz.Quiz.Domain.Entities;

public class QuestionSelectionRule
{
    public string Id { get; set; } = null!;
    public string QuizeTemplateId { get; set; } = null!;
    public string GroupId { get; set; } = null!;
    public QuestionType Type { get; set; }
    public int Level { get; set; }
    public int Count { get; set; }
    public int Score { get; set; }
}
