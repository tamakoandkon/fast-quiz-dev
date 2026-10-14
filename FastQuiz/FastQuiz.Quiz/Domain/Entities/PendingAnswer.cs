namespace FastQuiz.Quiz.Domain.Entities;

public class PendingAnswer
{
    public string UserId { get; set; } = null!;
    public string TenantId { get; set; } = null!;
    public string QuestionRecordId { get; set; } = null!; // Key
    public string QuizAttemptId { get; set; } = null!;
    public IReadOnlyList<string> Values { get; set; } = [];
    public DateTimeOffset AddedTime { get; set; }
}
