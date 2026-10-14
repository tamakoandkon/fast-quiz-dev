namespace FastQuiz.Quiz.Domain.Entities;

public class QuestionRecord
{
    public string Id { get; set; } = null!;
    public string MarkUpContent { get; set; } = null!;
    /// <summary>
    /// null 表示尚未批阅，false 表示已批阅且判错，true 表示已批阅且判对。
    /// </summary>
    public bool? IsCorrect { get; set; }
    public string QuizAttemptId { get; set; } = null!;
    public int SortOrder { get; set; } = 0;
    public string QuestionSnapshotId { get; set; } = null!;
    public List<AnswerRecord> AnswerRecords { get; set; } = [];

    public QuestionRecord() { }

    public QuestionRecord(
        string id,
        string markUpContent,
        bool? isCorrect,
        string quizAttemptId,
        int sortOrder,
        string questionSnapshotId,
        List<AnswerRecord> answerRecords
    )
    {
        Id = id;
        MarkUpContent = markUpContent;
        IsCorrect = isCorrect;
        QuizAttemptId = quizAttemptId;
        SortOrder = sortOrder;
        QuestionSnapshotId = questionSnapshotId;
        AnswerRecords = answerRecords;
    }
}
