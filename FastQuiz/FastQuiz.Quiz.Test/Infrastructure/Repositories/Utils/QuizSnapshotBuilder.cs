using FastQuiz.Quiz.Domain.Entities;

namespace FastQuiz.Quiz.Test.Infrastructure.Repositories.Utils;

/// <summary>
/// QuizSnapshot 的构建器，支持链式调用和批量生成
/// </summary>
internal class QuizSnapshotBuilder
{
    // 1. 私有字段区域
    private string _id;
    private string _quizTemplateId;
    private int _totalScore;
    private List<QuestionSnapshot> _questions;
    private bool _idSet;
    private bool _quizTemplateIdSet;

    // 2. 私有构造函数
    private QuizSnapshotBuilder()
    {
        Reset();
    }

    // 3. 静态工厂方法
    /// <summary>
    /// 创建新的 Builder 实例
    /// </summary>
    public static QuizSnapshotBuilder New() => new();

    // 4. 属性设置方法（WithXxx）
    /// <summary>
    /// 设置 Id
    /// </summary>
    public QuizSnapshotBuilder WithId(string id)
    {
        _id = id ?? throw new ArgumentNullException(nameof(id));
        _idSet = true;
        return this;
    }

    /// <summary>
    /// 设置 QuizTemplateId
    /// </summary>
    public QuizSnapshotBuilder WithQuizTemplateId(string quizTemplateId)
    {
        _quizTemplateId = quizTemplateId ?? throw new ArgumentNullException(nameof(quizTemplateId));
        _quizTemplateIdSet = true;
        return this;
    }

    /// <summary>
    /// 设置总分
    /// </summary>
    public QuizSnapshotBuilder WithTotalScore(int totalScore)
    {
        _totalScore = totalScore;
        return this;
    }

    /// <summary>
    /// 替换整个问题列表
    /// </summary>
    public QuizSnapshotBuilder WithQuestions(List<QuestionSnapshot> questions)
    {
        _questions = questions ?? new List<QuestionSnapshot>();
        return this;
    }

    /// <summary>
    /// 添加单个问题（追加）
    /// </summary>
    public QuizSnapshotBuilder AddQuestion(QuestionSnapshot question)
    {
        if (question == null)
            throw new ArgumentNullException(nameof(question));
        _questions.Add(question);
        return this;
    }

    /// <summary>
    /// 添加多个问题（追加）
    /// </summary>
    public QuizSnapshotBuilder AddQuestions(IEnumerable<QuestionSnapshot> questions)
    {
        if (questions == null)
            throw new ArgumentNullException(nameof(questions));
        _questions.AddRange(questions);
        return this;
    }

    /// <summary>
    /// 创建一个空的快照（没有任何问题）
    /// </summary>
    public QuizSnapshotBuilder EmptySnapshot()
    {
        _questions.Clear();
        _totalScore = 0;
        return this;
    }

    // 6. 辅助方法（Clone, Reset）

    /// <summary>
    /// 克隆当前 Builder 的状态
    /// </summary>
    public QuizSnapshotBuilder Clone()
    {
        return new QuizSnapshotBuilder
        {
            _id = _id,
            _quizTemplateId = _quizTemplateId,
            _totalScore = _totalScore,
            _questions = new List<QuestionSnapshot>(_questions),
            _idSet = _idSet,
            _quizTemplateIdSet = _quizTemplateIdSet,
        };
    }

    /// <summary>
    /// 重置所有字段到默认值
    /// </summary>
    public QuizSnapshotBuilder Reset()
    {
        _id = null!;
        _quizTemplateId = null!;
        _totalScore = 0;
        _questions = new List<QuestionSnapshot>();
        _idSet = false;
        _quizTemplateIdSet = false;
        return this;
    }

    // 7. 构建方法

    /// <summary>
    /// 构建单个 QuizSnapshot 实例
    /// </summary>
    public QuizSnapshot Build()
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

        return new QuizSnapshot
        {
            Id = _id,
            QuizTemplateId = _quizTemplateId,
            TotalScore = _totalScore,
            Questions = new List<QuestionSnapshot>(_questions), // 创建副本防止外部修改
        };
    }

    /// <summary>
    /// 批量构建指定数量的实例
    /// </summary>
    public List<QuizSnapshot> BuildMany(int count)
    {
        return BuildMany(count, null);
    }

    /// <summary>
    /// 批量构建指定数量的实例，支持对每个实例进行个性化配置
    /// </summary>
    /// <param name="count">生成数量</param>
    /// <param name="configure">配置回调，参数为 builder 和当前索引</param>
    public List<QuizSnapshot> BuildMany(int count, Action<QuizSnapshotBuilder, int>? configure)
    {
        if (count <= 0)
            throw new ArgumentException("Count must be positive", nameof(count));

        var results = new List<QuizSnapshot>(count);
        for (int i = 0; i < count; i++)
        {
            var builder = this.Clone();
            configure?.Invoke(builder, i);
            results.Add(builder.Build());
        }
        return results;
    }
}
