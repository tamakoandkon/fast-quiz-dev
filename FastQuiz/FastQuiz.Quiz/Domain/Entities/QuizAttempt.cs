namespace FastQuiz.Quiz.Domain.Entities;

public class QuizAttempt
{
    public string Id { get; set; } = null!;
    public string? AssignId { get; set; }
    public string QuizTemplateId { get; set; } = null!;
    public string QuizSnapshotId { get; set; } = null!;
    public DateTimeOffset? StartTime { get; set; }
    public DateTimeOffset? SubmitTime { get; set; }
    public int Score { get; set; }
    public QuizAttemptStatus Status { get; set; }
    public List<QuestionRecord> QuestionRecords { get; set; } = [];
}
