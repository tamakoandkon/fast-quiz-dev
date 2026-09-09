namespace FastQuiz.Quiz.Domain.Entities;

public class QuizSnapshot
{
    public string Id { get; set; } = null!;
    public string QuizTemplateId { get; set; } = null!;
    public int TotalScore { get; set; }
    public List<QuestionSnapshot> Questions { get; set; } = [];
}
