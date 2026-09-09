namespace FastQuiz.Quiz.Domain.Entities;

public class AnswerRecord
{
    public string Id { get; set; } = null!;
    public string QuizAttemptId { get; set; } = null!;
    public string QuestionRecordId { get; set; } = null!;
    public string Value { get; set; } = null!;
}
