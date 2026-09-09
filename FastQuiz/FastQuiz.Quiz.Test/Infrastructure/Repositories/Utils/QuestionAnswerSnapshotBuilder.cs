using FastQuiz.Question.Domain.Entities;
using FastQuiz.Quiz.Domain.Entities;

namespace FastQuiz.Quiz.Test.Infrastructure.Repositories.Utils;

/// <summary>
/// QuestionAnswerSnapshot 的构建器，支持链式调用和批量生成
/// </summary>
public class QuestionAnswerSnapshotBuilder
{
    // 1. 私有字段区域
    private string _id;
    private string _markUpContent;
    private bool _isCorrect;
    private string _questionSnapshotId;
    private int _sortOrder;
    private MatchStrategy _strategy;
    private bool _isIdSet;
    private bool _isMarkUpContentSet;
    private bool _isIsCorrectSet;
    private bool _isQuestionSnapshotIdSet;

    // 2. 私有构造函数
    private QuestionAnswerSnapshotBuilder()
    {
        Reset();
    }

    // 3. 静态工厂方法
    /// <summary>
    /// 创建一个新的 Builder 实例
    /// </summary>
    public static QuestionAnswerSnapshotBuilder New() => new();

    // 4. 属性设置方法（WithXxx）
    /// <summary>设置 Id</summary>
    public QuestionAnswerSnapshotBuilder WithId(string id)
    {
        _id = id;
        _isIdSet = true;
        return this;
    }

    /// <summary>设置 MarkUpContent</summary>
    public QuestionAnswerSnapshotBuilder WithMarkUpContent(string markUpContent)
    {
        _markUpContent = markUpContent;
        _isMarkUpContentSet = true;
        return this;
    }

    /// <summary>设置 IsCorrect</summary>
    /// <param name="isCorrect">应传入 "True" 或 "False" 字符串</param>
    public QuestionAnswerSnapshotBuilder WithIsCorrect(bool isCorrect)
    {
        _isCorrect = isCorrect;
        _isIsCorrectSet = true;
        return this;
    }

    /// <summary>设置 QuestionSnapshotId</summary>
    public QuestionAnswerSnapshotBuilder WithQuestionSnapshotId(string questionSnapshotId)
    {
        _questionSnapshotId = questionSnapshotId;
        _isQuestionSnapshotIdSet = true;
        return this;
    }

    /// <summary>设置 SortOrder</summary>
    public QuestionAnswerSnapshotBuilder WithSortOrder(int sortOrder)
    {
        _sortOrder = sortOrder;
        return this;
    }

    /// <summary>设置 Strategy</summary>
    public QuestionAnswerSnapshotBuilder WithStrategy(MatchStrategy strategy)
    {
        _strategy = strategy;
        return this;
    }

    // 6. 辅助方法（Clone, Reset）
    /// <summary>克隆当前 Builder 的状态</summary>
    public QuestionAnswerSnapshotBuilder Clone()
    {
        var clone = new QuestionAnswerSnapshotBuilder();
        clone._id = _id;
        clone._markUpContent = _markUpContent;
        clone._isCorrect = _isCorrect;
        clone._questionSnapshotId = _questionSnapshotId;
        clone._sortOrder = _sortOrder;
        clone._strategy = _strategy;
        clone._isIdSet = _isIdSet;
        clone._isMarkUpContentSet = _isMarkUpContentSet;
        clone._isIsCorrectSet = _isIsCorrectSet;
        clone._isQuestionSnapshotIdSet = _isQuestionSnapshotIdSet;
        return clone;
    }

    /// <summary>重置所有字段到默认值</summary>
    public QuestionAnswerSnapshotBuilder Reset()
    {
        _id = null!;
        _markUpContent = null!;
        _isCorrect = false;
        _questionSnapshotId = null!;
        _sortOrder = 0;
        _strategy = MatchStrategy.Exact;
        _isIdSet = false;
        _isMarkUpContentSet = false;
        _isIsCorrectSet = false;
        _isQuestionSnapshotIdSet = false;
        return this;
    }

    // 7. 构建方法
    /// <summary>构建单个实体实例，自动生成未设置的必填字段</summary>
    public QuestionAnswerSnapshot Build()
    {
        // 自动生成未设置的必填字段
        if (!_isIdSet)
            _id = Guid.NewGuid().ToString();

        if (!_isMarkUpContentSet)
            _markUpContent = GenerateMarkUpContent();

        if (!_isIsCorrectSet)
            _isCorrect = false;

        if (!_isQuestionSnapshotIdSet)
            _questionSnapshotId = Guid.NewGuid().ToString();

        // SortOrder 和 Strategy 有合理默认值，无需生成

        return new QuestionAnswerSnapshot
        {
            Id = _id,
            MarkUpContent = _markUpContent,
            IsCorrect = _isCorrect,
            QuestionSnapshotId = _questionSnapshotId,
            SortOrder = _sortOrder,
            Strategy = _strategy,
        };
    }

    /// <summary>批量构建指定数量的实体</summary>
    public List<QuestionAnswerSnapshot> BuildMany(int count)
    {
        return BuildMany(count, null);
    }

    /// <summary>批量构建指定数量的实体，支持每个实例的个性化配置</summary>
    public List<QuestionAnswerSnapshot> BuildMany(
        int count,
        Action<QuestionAnswerSnapshotBuilder, int>? configure
    )
    {
        if (count <= 0)
            throw new ArgumentException("Count must be greater than 0", nameof(count));

        var results = new List<QuestionAnswerSnapshot>(count);
        for (int i = 0; i < count; i++)
        {
            var builder = Clone();
            configure?.Invoke(builder, i);
            results.Add(builder.Build());
        }
        return results;
    }

    // 辅助方法：生成默认的 MarkUpContent
    private string GenerateMarkUpContent()
    {
        return $"<p>Default answer content for {_id}</p>";
    }
}
