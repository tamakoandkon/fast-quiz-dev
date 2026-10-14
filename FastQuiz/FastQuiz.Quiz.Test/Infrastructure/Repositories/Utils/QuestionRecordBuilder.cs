using FastQuiz.Quiz.Domain.Entities;

namespace FastQuiz.Quiz.Test.Infrastructure.Repositories.Utils;

/// <summary>
/// QuestionRecord 的构建器，支持链式调用和批量生成
/// </summary>
public class QuestionRecordBuilder
{
    // 1. 私有字段区域
    private string _id;
    private string _markUpContent;
    private bool? _isCorrect;
    private string _quizAttemptId;
    private string _questionSnapshotId;
    private int _sortOrder;

    // 标记字段是否被显式设置
    private bool _idSet;
    private bool _markUpContentSet;
    private bool _isCorrectSet;
    private bool _quizAttemptIdSet;
    private bool _questionSnapshotIdSet;
    private bool _sortOrderSet;

    // 2. 私有构造函数
    private QuestionRecordBuilder()
    {
        Reset();
    }

    // 3. 静态工厂方法
    /// <summary>
    /// 创建一个新的 Builder 实例
    /// </summary>
    public static QuestionRecordBuilder New() => new();

    // 4. 属性设置方法（WithXxx）

    /// <summary>设置 Id</summary>
    public QuestionRecordBuilder WithId(string id)
    {
        _id = id;
        _idSet = true;
        return this;
    }

    /// <summary>设置 MarkUpContent</summary>
    public QuestionRecordBuilder WithMarkUpContent(string markUpContent)
    {
        _markUpContent = markUpContent;
        _markUpContentSet = true;
        return this;
    }

    /// <summary>设置 IsCorrect</summary>
    public QuestionRecordBuilder WithIsCorrect(bool isCorrect)
    {
        _isCorrect = isCorrect;
        _isCorrectSet = true;
        return this;
    }

    /// <summary>设置 QuestionSessionId</summary>
    public QuestionRecordBuilder WithQuizAttemptId(string quizAttemptId)
    {
        _quizAttemptId = quizAttemptId;
        _quizAttemptIdSet = true;
        return this;
    }

    public QuestionRecordBuilder WithQuestionSnapshotId(string questionSnapshotId)
    {
        _questionSnapshotId = questionSnapshotId;
        _questionSnapshotIdSet = true;
        return this;
    }

    /// <summary>设置 SortOrder</summary>
    public QuestionRecordBuilder WithSortOrder(int sortOrder)
    {
        _sortOrder = sortOrder;
        _sortOrderSet = true;
        return this;
    }

    // 6. 辅助方法（Clone, Reset）

    /// <summary>克隆当前 Builder 状态</summary>
    public QuestionRecordBuilder Clone()
    {
        var clone = new QuestionRecordBuilder();
        clone._id = _id;
        clone._markUpContent = _markUpContent;
        clone._isCorrect = _isCorrect;
        clone._quizAttemptId = _quizAttemptId;
        clone._questionSnapshotId = _questionSnapshotId;
        clone._sortOrder = _sortOrder;
        clone._idSet = _idSet;
        clone._markUpContentSet = _markUpContentSet;
        clone._isCorrectSet = _isCorrectSet;
        clone._quizAttemptIdSet = _quizAttemptIdSet;
        clone._questionSnapshotIdSet = _questionSnapshotIdSet;
        clone._sortOrderSet = _sortOrderSet;
        return clone;
    }

    /// <summary>重置所有字段到默认值</summary>
    public QuestionRecordBuilder Reset()
    {
        _id = null!;
        _markUpContent = null!;
        _isCorrect = null;
        _quizAttemptId = null!;
        _questionSnapshotId = null!;
        _sortOrder = 0;

        _idSet = false;
        _markUpContentSet = false;
        _isCorrectSet = false;
        _quizAttemptIdSet = false;
        _questionSnapshotIdSet = false;
        _sortOrderSet = false;

        return this;
    }

    // 7. 构建方法

    /// <summary>构建单个实体实例</summary>
    public QuestionRecord Build()
    {
        // 自动生成未设置的必填字段
        if (!_idSet)
        {
            _id = Guid.NewGuid().ToString();
        }

        if (!_markUpContentSet)
        {
            _markUpContent = GenerateRandomMarkUpContent();
        }

        if (!_isCorrectSet)
        {
            _isCorrect = null;
        }

        if (!_quizAttemptIdSet)
        {
            _quizAttemptId = Guid.NewGuid().ToString();
        }

        if (!_questionSnapshotIdSet)
        {
            _questionSnapshotId = Guid.NewGuid().ToString();
        }

        if (!_sortOrderSet)
        {
            _sortOrder = 0;
        }

        var record = new QuestionRecord
        {
            Id = _id,
            MarkUpContent = _markUpContent,
            IsCorrect = _isCorrect,
            QuestionSnapshotId = _questionSnapshotId,
            QuizAttemptId = _quizAttemptId,
            SortOrder = _sortOrder,
        };

        return record;
    }

    /// <summary>批量构建指定数量的实体</summary>
    public List<QuestionRecord> BuildMany(int count)
    {
        return BuildMany(count, null);
    }

    /// <summary>批量构建指定数量的实体，支持个性化配置</summary>
    /// <param name="count">生成数量</param>
    /// <param name="configure">个性化配置委托，参数为 builder 和当前索引</param>
    public List<QuestionRecord> BuildMany(int count, Action<QuestionRecordBuilder, int>? configure)
    {
        if (count <= 0)
            throw new ArgumentException("Count must be positive", nameof(count));

        var results = new List<QuestionRecord>(count);

        for (int i = 0; i < count; i++)
        {
            // 克隆当前状态或重置到初始状态
            var builder = Clone();
            builder.Reset();

            // 执行个性化配置
            configure?.Invoke(builder, i);

            // 构建实体
            results.Add(builder.Build());
        }

        return results;
    }

    // 私有辅助方法
    private string GenerateRandomMarkUpContent()
    {
        var random = new Random();
        var contents = new[]
        {
            "<p>Sample question content</p>",
            "<div>What is the answer to this question?</div>",
            "<p>请选择正确的选项</p>",
            "<span>测试问题内容</span>",
        };
        return contents[random.Next(contents.Length)];
    }
}
