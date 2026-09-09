using FastQuiz.Question.Domain.Entities;

namespace FastQuiz.Quiz.Domain.Entities;

public class QuestionAnswerSnapshot
{
    public string Id { get; set; } = null!;
    public string MarkUpContent { get; set; } = null!;
    public bool IsCorrect { get; set; }
    public string QuestionSnapshotId { get; set; } = null!;
    public int SortOrder { get; set; } = 0;
    public MatchStrategy Strategy { get; set; } = MatchStrategy.Exact;
}
