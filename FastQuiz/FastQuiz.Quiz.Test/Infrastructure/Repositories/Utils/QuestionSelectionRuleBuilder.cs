using FastQuiz.Question.Domain.Entities;
using FastQuiz.Quiz.Domain.Entities;

namespace FastQuiz.Quiz.Test.Infrastructure.Repositories.Utils;

/// <summary>
/// QuestionSelectionRule 的构建器，支持链式调用和批量生成
/// </summary>
public class QuestionSelectionRuleBuilder
{
    // 1. 私有字段区域
    private string _id;
    private string _quizeTemplateId;
    private string _groupId;
    private QuestionType _type;
    private int _level;
    private int _count;
    private int _score;

    // 标记字段是否已显式设置
    private bool _idSet;
    private bool _quizeTemplateIdSet;
    private bool _groupIdSet;
    private bool _typeSet;
    private bool _levelSet;
    private bool _countSet;
    private bool _scoreSet;

    // 2. 私有构造函数
    private QuestionSelectionRuleBuilder()
    {
        Reset();
    }

    // 3. 静态工厂方法
    /// <summary>
    /// 创建一个新的 Builder 实例
    /// </summary>
    public static QuestionSelectionRuleBuilder New() => new();

    // 4. 属性设置方法（WithXxx）

    /// <summary>
    /// 设置 Id
    /// </summary>
    public QuestionSelectionRuleBuilder WithId(string id)
    {
        _id = id;
        _idSet = true;
        return this;
    }

    /// <summary>
    /// 设置 QuizeTemplateId
    /// </summary>
    public QuestionSelectionRuleBuilder WithQuizTemplateId(string quizeTemplateId)
    {
        _quizeTemplateId = quizeTemplateId;
        _quizeTemplateIdSet = true;
        return this;
    }

    /// <summary>
    /// 设置 GroupId
    /// </summary>
    public QuestionSelectionRuleBuilder WithGroupId(string groupId)
    {
        _groupId = groupId;
        _groupIdSet = true;
        return this;
    }

    /// <summary>
    /// 设置 Type
    /// </summary>
    public QuestionSelectionRuleBuilder WithType(QuestionType type)
    {
        _type = type;
        _typeSet = true;
        return this;
    }

    /// <summary>
    /// 设置 Level
    /// </summary>
    public QuestionSelectionRuleBuilder WithLevel(int level)
    {
        _level = level;
        _levelSet = true;
        return this;
    }

    /// <summary>
    /// 设置 Count
    /// </summary>
    public QuestionSelectionRuleBuilder WithCount(int count)
    {
        _count = count;
        _countSet = true;
        return this;
    }

    /// <summary>
    /// 设置 Score
    /// </summary>
    public QuestionSelectionRuleBuilder WithScore(int score)
    {
        _score = score;
        _scoreSet = true;
        return this;
    }

    // 6. 辅助方法（Clone, Reset）

    /// <summary>
    /// 克隆当前 Builder 状态
    /// </summary>
    public QuestionSelectionRuleBuilder Clone()
    {
        var clone = new QuestionSelectionRuleBuilder();
        clone._id = _id;
        clone._quizeTemplateId = _quizeTemplateId;
        clone._groupId = _groupId;
        clone._type = _type;
        clone._level = _level;
        clone._count = _count;
        clone._score = _score;
        clone._idSet = _idSet;
        clone._quizeTemplateIdSet = _quizeTemplateIdSet;
        clone._groupIdSet = _groupIdSet;
        clone._typeSet = _typeSet;
        clone._levelSet = _levelSet;
        clone._countSet = _countSet;
        clone._scoreSet = _scoreSet;
        return clone;
    }

    /// <summary>
    /// 重置所有字段到默认值
    /// </summary>
    public QuestionSelectionRuleBuilder Reset()
    {
        _id = null!;
        _quizeTemplateId = null!;
        _groupId = null!;
        _type = QuestionType.SingleChoice; // 根据实际默认枚举值调整
        _level = 1;
        _count = 1;
        _score = 1;

        _idSet = false;
        _quizeTemplateIdSet = false;
        _groupIdSet = false;
        _typeSet = false;
        _levelSet = false;
        _countSet = false;
        _scoreSet = false;

        return this;
    }

    // 7. 构建方法

    /// <summary>
    /// 构建单个实体实例，自动生成未设置的必填字段
    /// </summary>
    public QuestionSelectionRule Build()
    {
        // 自动生成未设置的必填字段
        if (!_idSet)
        {
            _id = Guid.NewGuid().ToString();
        }

        if (!_quizeTemplateIdSet)
        {
            _quizeTemplateId = Guid.NewGuid().ToString();
        }

        if (!_groupIdSet)
        {
            _groupId = Guid.NewGuid().ToString();
        }

        // 值类型使用默认值（已在构造函数中初始化）

        return new QuestionSelectionRule
        {
            Id = _id,
            QuizeTemplateId = _quizeTemplateId,
            GroupId = _groupId,
            Type = _type,
            Level = _level,
            Count = _count,
            Score = _score,
        };
    }

    /// <summary>
    /// 批量构建实体
    /// </summary>
    public List<QuestionSelectionRule> BuildMany(int count)
    {
        return BuildMany(count, null);
    }

    /// <summary>
    /// 批量构建实体，支持个性化配置
    /// </summary>
    /// <param name="count">生成数量</param>
    /// <param name="configure">配置委托，参数为 builder 实例和当前索引</param>
    public List<QuestionSelectionRule> BuildMany(
        int count,
        Action<QuestionSelectionRuleBuilder, int>? configure
    )
    {
        if (count <= 0)
            throw new ArgumentException("Count must be greater than 0", nameof(count));

        var results = new List<QuestionSelectionRule>(count);

        for (int i = 0; i < count; i++)
        {
            var builder = this.Clone();
            configure?.Invoke(builder, i);
            results.Add(builder.Build());
        }

        return results;
    }
}
