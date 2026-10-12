using FastQuiz.Common.Models;

namespace FastQuiz.Quiz.Infrastructure.Repositories;

public class QuizDbContextSettings
{
    public string ConnectionString { get; set; } = null!;
    public DatabaseType Type { get; set; } = DatabaseType.Sqlite;
    public string QuizTemplateTableName { get; set; } = "QuizTemplates";
    public string QuestionSelectionRuleTableName { get; set; } = "QuestionSelectionRules";
    public string QuestionSnapshotTableName { get; set; } = "QuestionSnapshots";
    public string QuestionAnswerSnapshotTableName { get; set; } = "QuestionAnswerSnapshots";
    public string QuizSnapshotTableName { get; set; } = "QuizSnapshots";
    public string QuizSnapshotQuestionSnapshotMapTableName { get; set; } =
        "QuizSnapshotQuestionSnapshotMaps";
    public string QuizAttemptTableName { get; set; } = "QuizAttempts";
    public string QuestionRecordTableName { get; set; } = "QuestionRecords";
    public string AnswerRecordTableName { get; set; } = "AnswerRecords";
}
