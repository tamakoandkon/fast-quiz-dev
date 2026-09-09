using FastQuiz.Quiz.Domain.Entities;

namespace FastQuiz.Quiz.Test.Infrastructure.Repositories.Utils;

/// <summary>
/// QuizAttempt 的构建器，支持链式调用和批量生成
/// </summary>
public class QuizAttemptBuilder
{
    // 1. 私有字段区域
    private string _id;
    private string? _assignId;
    private string _quizTemplateId;
    private string _quizSnapshotId;
    private DateTimeOffset _startTime;
    private DateTimeOffset _submitTime;
    private int _score;
    private QuizAttemptStatus _status;
    private List<QuestionRecord> _questionRecords;

    // 标记字段是否被显式设置
    private bool _idSet;
    private bool _assignIdSet;
    private bool _quizTemplateIdSet;
    private bool _quizSnapshotIdSet;
    private bool _startTimeSet;
    private bool _submitTimeSet;
    private bool _scoreSet;
    private bool _statusSet;
    private bool _questionRecordsSet;

    // 2. 私有构造函数
    private QuizAttemptBuilder()
    {
        Reset();
    }

    // 3. 静态工厂方法
    /// <summary>
    /// 创建一个新的 Builder 实例
    /// </summary>
    public static QuizAttemptBuilder New() => new();

    // 4. 属性设置方法（WithXxx）

    /// <summary>
    /// 设置 Id
    /// </summary>
    public QuizAttemptBuilder WithId(string id)
    {
        _id = id;
        _idSet = true;
        return this;
    }

    /// <summary>
    /// 设置 AssignId
    /// </summary>
    public QuizAttemptBuilder WithAssignId(string? assignId)
    {
        _assignId = assignId;
        _assignIdSet = true;
        return this;
    }

    /// <summary>
    /// 设置 QuizTemplateId
    /// </summary>
    public QuizAttemptBuilder WithQuizTemplateId(string quizTemplateId)
    {
        _quizTemplateId = quizTemplateId;
        _quizTemplateIdSet = true;
        return this;
    }

    /// <summary>
    /// 设置 QuizSampleId
    /// </summary>
    public QuizAttemptBuilder WithQuizSnapshotId(string quizSampleId)
    {
        _quizSnapshotId = quizSampleId;
        _quizSnapshotIdSet = true;
        return this;
    }

    /// <summary>
    /// 设置开始时间
    /// </summary>
    public QuizAttemptBuilder WithStartTime(DateTimeOffset startTime)
    {
        _startTime = startTime;
        _startTimeSet = true;
        return this;
    }

    /// <summary>
    /// 设置提交时间
    /// </summary>
    public QuizAttemptBuilder WithSubmitTime(DateTimeOffset submitTime)
    {
        _submitTime = submitTime;
        _submitTimeSet = true;
        return this;
    }

    /// <summary>
    /// 设置分数
    /// </summary>
    public QuizAttemptBuilder WithScore(int score)
    {
        _score = score;
        _scoreSet = true;
        return this;
    }

    /// <summary>
    /// 设置状态
    /// </summary>
    public QuizAttemptBuilder WithStatus(QuizAttemptStatus status)
    {
        _status = status;
        _statusSet = true;
        return this;
    }

    /// <summary>
    /// 替换整个问题记录集合
    /// </summary>
    public QuizAttemptBuilder WithQuestionRecords(List<QuestionRecord> questionRecords)
    {
        _questionRecords = questionRecords;
        _questionRecordsSet = true;
        return this;
    }

    /// <summary>
    /// 添加单个问题记录（追加模式）
    /// </summary>
    public QuizAttemptBuilder AddQuestionRecord(QuestionRecord questionRecord)
    {
        if (!_questionRecordsSet)
        {
            _questionRecords = new List<QuestionRecord>();
            _questionRecordsSet = true;
        }
        _questionRecords.Add(questionRecord);
        return this;
    }

    /// <summary>
    /// 添加多个问题记录（追加模式）
    /// </summary>
    public QuizAttemptBuilder AddQuestionRecords(IEnumerable<QuestionRecord> questionRecords)
    {
        if (!_questionRecordsSet)
        {
            _questionRecords = new List<QuestionRecord>();
            _questionRecordsSet = true;
        }
        _questionRecords.AddRange(questionRecords);
        return this;
    }

    // 6. 辅助方法

    /// <summary>
    /// 克隆当前 Builder 状态
    /// </summary>
    public QuizAttemptBuilder Clone()
    {
        var clone = new QuizAttemptBuilder();
        clone._id = _id;
        clone._idSet = _idSet;
        clone._assignId = _assignId;
        clone._assignIdSet = _assignIdSet;
        clone._quizTemplateId = _quizTemplateId;
        clone._quizTemplateIdSet = _quizTemplateIdSet;
        clone._quizSnapshotId = _quizSnapshotId;
        clone._quizSnapshotIdSet = _quizSnapshotIdSet;
        clone._startTime = _startTime;
        clone._startTimeSet = _startTimeSet;
        clone._submitTime = _submitTime;
        clone._submitTimeSet = _submitTimeSet;
        clone._score = _score;
        clone._scoreSet = _scoreSet;
        clone._status = _status;
        clone._statusSet = _statusSet;
        clone._questionRecords =
            _questionRecords != null ? new List<QuestionRecord>(_questionRecords) : null;
        clone._questionRecordsSet = _questionRecordsSet;
        return clone;
    }

    /// <summary>
    /// 重置所有字段到默认值
    /// </summary>
    public QuizAttemptBuilder Reset()
    {
        _id = null!;
        _idSet = false;
        _assignId = null;
        _assignIdSet = false;
        _quizTemplateId = null!;
        _quizTemplateIdSet = false;
        _quizSnapshotId = null!;
        _quizSnapshotIdSet = false;
        _startTime = DateTimeOffset.UtcNow;
        _startTimeSet = false;
        _submitTime = DateTimeOffset.UtcNow;
        _submitTimeSet = false;
        _score = 0;
        _scoreSet = false;
        _status = QuizAttemptStatus.InProgress;
        _statusSet = false;
        _questionRecords = [];
        _questionRecordsSet = false;
        return this;
    }

    // 7. 构建方法

    /// <summary>
    /// 构建单个 QuizAttempt 实例，自动生成未设置的必填字段
    /// </summary>
    public QuizAttempt Build()
    {
        // 自动生成未设置的必填字段
        if (!_idSet)
        {
            _id = Guid.NewGuid().ToString();
        }

        if (!_quizTemplateIdSet)
        {
            _quizTemplateId = Guid.NewGuid().ToString();
        }

        if (!_quizSnapshotIdSet)
        {
            _quizSnapshotId = Guid.NewGuid().ToString();
        }

        if (!_startTimeSet)
        {
            _startTime = DateTimeOffset.UtcNow;
        }

        if (!_submitTimeSet)
        {
            _submitTime = DateTimeOffset.UtcNow;
        }

        if (!_scoreSet)
        {
            _score = 0;
        }

        if (!_statusSet)
        {
            _status = QuizAttemptStatus.InProgress;
        }

        if (!_questionRecordsSet)
        {
            _questionRecords = [];
        }

        return new QuizAttempt
        {
            Id = _id,
            AssignId = _assignId,
            QuizTemplateId = _quizTemplateId,
            QuizSnapshotId = _quizSnapshotId,
            StartTime = _startTime,
            SubmitTime = _submitTime,
            Score = _score,
            Status = _status,
            QuestionRecords = _questionRecords,
        };
    }

    /// <summary>
    /// 批量构建多个 QuizAttempt 实例
    /// </summary>
    public List<QuizAttempt> BuildMany(int count)
    {
        return BuildMany(count, null);
    }

    /// <summary>
    /// 批量构建多个 QuizAttempt 实例，支持为每个实例单独配置
    /// </summary>
    /// <param name="count">生成数量</param>
    /// <param name="configure">配置委托，参数为 builder 和当前索引</param>
    public List<QuizAttempt> BuildMany(int count, Action<QuizAttemptBuilder, int>? configure)
    {
        var results = new List<QuizAttempt>(count);
        for (int i = 0; i < count; i++)
        {
            var builder = New();
            builder.Reset();
            configure?.Invoke(builder, i);
            results.Add(builder.Build());
        }
        return results;
    }
}
