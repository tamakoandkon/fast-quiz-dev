using FastQuiz.Question.Domain.Entities;

namespace FastQuiz.Quiz.Domain.Entities;

public class QuestionSnapshot
{
    public string Id { get; set; } = null!;
    public string QuizTemplateId { get; set; } = null!;
    public string QuestionId { get; set; } = null!;
    public string MarkUpContent { get; set; } = null!;
    public QuestionType Type { get; set; }
    public int Level { get; set; }
    public string GroupId { get; set; } = null!;
    public string? ParentId { get; set; } = null;
    public List<QuestionAnswerSnapshot> AnswerSnapshots { get; set; } = [];
}
