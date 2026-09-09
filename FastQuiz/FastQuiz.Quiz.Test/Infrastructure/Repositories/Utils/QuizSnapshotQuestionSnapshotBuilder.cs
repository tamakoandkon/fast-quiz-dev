using FastQuiz.Quiz.Domain.Entities;

namespace FastQuiz.Quiz.Test.Infrastructure.Repositories.Utils;

/// <summary>
/// QuizSnapshotQuestionSnapshotMap 的构建器，支持链式调用和批量生成
/// </summary>
internal class QuizSnapshotQuestionSnapshotMapBuilder
{
    // 1. 私有字段区域
    private string _quizSnapshotId;
    private string _questionSnapshotId;
    private bool _quizSnapshotIdSet;
    private bool _questionSnapshotIdSet;

    // 2. 私有构造函数
    private QuizSnapshotQuestionSnapshotMapBuilder()
    {
        Reset();
    }

    // 3. 静态工厂方法
    /// <summary>
    /// 创建新的 Builder 实例
    /// </summary>
    public static QuizSnapshotQuestionSnapshotMapBuilder New() => new();

    // 4. 属性设置方法（WithXxx）
    /// <summary>
    /// 设置 QuizSnapshotId
    /// </summary>
    public QuizSnapshotQuestionSnapshotMapBuilder WithQuizSnapshotId(string value)
    {
        _quizSnapshotId = value;
        _quizSnapshotIdSet = true;
        return this;
    }

    /// <summary>
    /// 设置 QuestionSnapshotId
    /// </summary>
    public QuizSnapshotQuestionSnapshotMapBuilder WithQuestionSnapshotId(string value)
    {
        _questionSnapshotId = value;
        _questionSnapshotIdSet = true;
        return this;
    }

    // 6. 辅助方法（Clone, Reset）
    /// <summary>
    /// 克隆当前 Builder 状态
    /// </summary>
    public QuizSnapshotQuestionSnapshotMapBuilder Clone()
    {
        var cloned = new QuizSnapshotQuestionSnapshotMapBuilder();
        cloned._quizSnapshotId = _quizSnapshotId;
        cloned._questionSnapshotId = _questionSnapshotId;
        cloned._quizSnapshotIdSet = _quizSnapshotIdSet;
        cloned._questionSnapshotIdSet = _questionSnapshotIdSet;
        return cloned;
    }

    /// <summary>
    /// 重置所有字段到默认值
    /// </summary>
    public QuizSnapshotQuestionSnapshotMapBuilder Reset()
    {
        _quizSnapshotId = null!;
        _questionSnapshotId = null!;
        _quizSnapshotIdSet = false;
        _questionSnapshotIdSet = false;
        return this;
    }

    // 7. 构建方法
    /// <summary>
    /// 构建单个实体实例，未设置的必填字段自动生成
    /// </summary>
    public QuizSnapshotQuestionSnapshotMap Build()
    {
        return new QuizSnapshotQuestionSnapshotMap
        {
            QuizSnapshotId = _quizSnapshotIdSet ? _quizSnapshotId : Guid.NewGuid().ToString(),
            QuestionSnapshotId = _questionSnapshotIdSet
                ? _questionSnapshotId
                : Guid.NewGuid().ToString(),
        };
    }

    /// <summary>
    /// 批量构建指定数量的实体
    /// </summary>
    public List<QuizSnapshotQuestionSnapshotMap> BuildMany(int count)
    {
        return BuildMany(count, null);
    }

    /// <summary>
    /// 批量构建指定数量的实体，支持个性化配置
    /// </summary>
    /// <param name="count">生成数量</param>
    /// <param name="configure">配置回调，参数为 builder 副本和当前索引</param>
    public List<QuizSnapshotQuestionSnapshotMap> BuildMany(
        int count,
        Action<QuizSnapshotQuestionSnapshotMapBuilder, int>? configure
    )
    {
        if (count <= 0)
            throw new ArgumentException("Count must be greater than 0", nameof(count));

        var results = new List<QuizSnapshotQuestionSnapshotMap>(count);

        for (int i = 0; i < count; i++)
        {
            var builder = New();

            configure?.Invoke(builder, i);

            results.Add(builder.Build());
        }

        return results;
    }
}
