using FastQuiz.Question.Domain.Entities;
using FastQuiz.Quiz.Domain.Entities;

namespace FastQuiz.Quiz.Test.Infrastructure.Repositories.Utils;

/// <summary>
/// QuestionSnapshot 的构建器，支持链式调用和批量生成
/// </summary>
public class QuestionSnapshotBuilder
{
    // 1. 私有字段区域
    private string _id;
    private string _quizTemplateId;
    private string _questionId;
    private string _markUpContent;
    private QuestionType _type;
    private int _level;
    private string _groupId;
    private string? _parentId;
    private List<QuestionAnswerSnapshot> _answerSnapshots;
    private bool _idSet;
    private bool _quizTemplateIdSet;
    private bool _questionIdSet;
    private bool _markUpContentSet;
    private bool _typeSet;
    private bool _levelSet;
    private bool _groupIdSet;
    private bool _parentIdSet;
    private bool _answerSnapshotsSet;

    // 2. 私有构造函数
    private QuestionSnapshotBuilder()
    {
        Reset();
    }

    // 3. 静态工厂方法
    /// <summary>
    /// 创建新的 Builder 实例
    /// </summary>
    public static QuestionSnapshotBuilder New() => new();

    // 4. 属性设置方法（WithXxx）
    /// <summary>
    /// 设置 Id
    /// </summary>
    public QuestionSnapshotBuilder WithId(string id)
    {
        _id = id;
        _idSet = true;
        return this;
    }

    public QuestionSnapshotBuilder WithQuizTemplateId(string quizTemplateId)
    {
        _quizTemplateId = quizTemplateId;
        _quizTemplateIdSet = true;
        return this;
    }

    /// <summary>
    /// 设置 QuestionId
    /// </summary>
    public QuestionSnapshotBuilder WithQuestionId(string questionId)
    {
        _questionId = questionId;
        _questionIdSet = true;
        return this;
    }

    /// <summary>
    /// 设置 MarkUpContent
    /// </summary>
    public QuestionSnapshotBuilder WithMarkUpContent(string markUpContent)
    {
        _markUpContent = markUpContent;
        _markUpContentSet = true;
        return this;
    }

    /// <summary>
    /// 设置 Type
    /// </summary>
    public QuestionSnapshotBuilder WithType(QuestionType type)
    {
        _type = type;
        _typeSet = true;
        return this;
    }

    /// <summary>
    /// 设置 Level
    /// </summary>
    public QuestionSnapshotBuilder WithLevel(int level)
    {
        _level = level;
        _levelSet = true;
        return this;
    }

    /// <summary>
    /// 设置 GroupId
    /// </summary>
    public QuestionSnapshotBuilder WithGroupId(string groupId)
    {
        _groupId = groupId;
        _groupIdSet = true;
        return this;
    }

    /// <summary>
    /// 设置 ParentId
    /// </summary>
    public QuestionSnapshotBuilder WithParentId(string? parentId)
    {
        _parentId = parentId;
        _parentIdSet = true;
        return this;
    }

    /// <summary>
    /// 替换整个 AnswerSnapshots 集合
    /// </summary>
    public QuestionSnapshotBuilder WithAnswerSnapshots(List<QuestionAnswerSnapshot> answerSnapshots)
    {
        _answerSnapshots = answerSnapshots ?? new List<QuestionAnswerSnapshot>();
        _answerSnapshotsSet = true;
        return this;
    }

    /// <summary>
    /// 添加单个 QuestionAnswerSnapshot
    /// </summary>
    public QuestionSnapshotBuilder AddAnswerSnapshot(QuestionAnswerSnapshot answerSnapshot)
    {
        if (!_answerSnapshotsSet)
        {
            _answerSnapshots = new List<QuestionAnswerSnapshot>();
            _answerSnapshotsSet = true;
        }
        _answerSnapshots.Add(answerSnapshot);
        return this;
    }

    /// <summary>
    /// 添加多个 QuestionAnswerSnapshot
    /// </summary>
    public QuestionSnapshotBuilder AddAnswerSnapshots(
        IEnumerable<QuestionAnswerSnapshot> answerSnapshots
    )
    {
        if (!_answerSnapshotsSet)
        {
            _answerSnapshots = new List<QuestionAnswerSnapshot>();
            _answerSnapshotsSet = true;
        }
        _answerSnapshots.AddRange(answerSnapshots);
        return this;
    }

    /// <summary>
    /// 设置为根问题（无父级）
    /// </summary>
    public QuestionSnapshotBuilder AsRootQuestion()
    {
        _parentId = null;
        _parentIdSet = true;
        return this;
    }

    /// <summary>
    /// 设置难度级别（1-5）
    /// </summary>
    public QuestionSnapshotBuilder WithDifficultyLevel(int level)
    {
        if (level < 1)
            level = 1;
        if (level > 5)
            level = 5;
        _level = level;
        _levelSet = true;
        return this;
    }

    // 6. 辅助方法（Clone, Reset）
    /// <summary>
    /// 克隆当前 Builder 状态
    /// </summary>
    public QuestionSnapshotBuilder Clone()
    {
        var clone = new QuestionSnapshotBuilder();
        clone._id = _id;
        clone._idSet = _idSet;
        clone._questionId = _questionId;
        clone._questionIdSet = _questionIdSet;
        clone._markUpContent = _markUpContent;
        clone._markUpContentSet = _markUpContentSet;
        clone._type = _type;
        clone._typeSet = _typeSet;
        clone._level = _level;
        clone._levelSet = _levelSet;
        clone._groupId = _groupId;
        clone._groupIdSet = _groupIdSet;
        clone._parentId = _parentId;
        clone._parentIdSet = _parentIdSet;
        clone._answerSnapshots =
            _answerSnapshots != null ? new List<QuestionAnswerSnapshot>(_answerSnapshots) : null;
        clone._answerSnapshotsSet = _answerSnapshotsSet;
        return clone;
    }

    /// <summary>
    /// 重置所有字段到默认值
    /// </summary>
    public QuestionSnapshotBuilder Reset()
    {
        _id = null!;
        _questionId = null!;
        _markUpContent = null!;
        _type = QuestionType.SingleChoice;
        _level = 1;
        _groupId = null!;
        _parentId = null;
        _answerSnapshots = new List<QuestionAnswerSnapshot>();

        _idSet = false;
        _questionIdSet = false;
        _markUpContentSet = false;
        _typeSet = false;
        _levelSet = false;
        _groupIdSet = false;
        _parentIdSet = false;
        _answerSnapshotsSet = true; // 默认初始化为空集合

        return this;
    }

    // 7. 构建方法
    /// <summary>
    /// 构建单个 QuestionSnapshot 实例
    /// </summary>
    public QuestionSnapshot Build()
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

        if (!_questionIdSet)
        {
            _questionId = Guid.NewGuid().ToString();
        }

        if (!_markUpContentSet)
        {
            _markUpContent =
                $"<p>Auto-generated question content at {DateTime.Now:yyyy-MM-dd HH:mm:ss}</p>";
        }

        if (!_typeSet)
        {
            _type = QuestionType.SingleChoice;
        }

        if (!_levelSet)
        {
            _level = 1;
        }

        if (!_groupIdSet)
        {
            _groupId = Guid.NewGuid().ToString();
        }

        if (!_parentIdSet)
        {
            _parentId = null;
        }

        if (!_answerSnapshotsSet)
        {
            _answerSnapshots = new List<QuestionAnswerSnapshot>();
        }

        return new QuestionSnapshot
        {
            Id = _id,
            QuizTemplateId = _quizTemplateId,
            QuestionId = _questionId,
            MarkUpContent = _markUpContent,
            Type = _type,
            Level = _level,
            GroupId = _groupId,
            ParentId = _parentId,
            AnswerSnapshots = _answerSnapshots ?? new List<QuestionAnswerSnapshot>(),
        };
    }

    /// <summary>
    /// 批量构建 QuestionSnapshot 实例
    /// </summary>
    public List<QuestionSnapshot> BuildMany(int count)
    {
        return BuildMany(count, null);
    }

    /// <summary>
    /// 批量构建 QuestionSnapshot 实例，支持个性化配置
    /// </summary>
    public List<QuestionSnapshot> BuildMany(
        int count,
        Action<QuestionSnapshotBuilder, int>? configure
    )
    {
        if (count <= 0)
            throw new ArgumentException("Count must be greater than 0", nameof(count));

        var results = new List<QuestionSnapshot>(count);

        for (int i = 0; i < count; i++)
        {
            var builder = Clone();
            configure?.Invoke(builder, i);
            results.Add(builder.Build());
        }

        return results;
    }
}
