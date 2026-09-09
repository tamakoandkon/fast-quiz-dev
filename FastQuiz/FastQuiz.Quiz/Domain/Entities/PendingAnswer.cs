namespace FastQuiz.Quiz.Domain.Entities;

public class PendingAnswer
{
    public string UserId { get; set; }
    public string TenantId { get; set; }
    public string QuestionRecordId { get; set; } // Key
    public string QuizAttemptId { get; set; }
    public string Value { get; set; } // 可能被覆盖
    public DateTimeOffset AddedTime { get; set; }
}
