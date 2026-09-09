using FastQuiz.Quiz.Domain.Entities;

namespace FastQuiz.Quiz.Test.Infrastructure.Repositories.Utils;

/// <summary>
/// QuizTemplate 的构建器，支持链式调用和批量生成
/// </summary>
public class QuizTemplateBuilder
{
    // 1. 私有字段区域
    private string _id;
    private string _name;
    private string _description;
    private string _remark;
    private string _ownerId;
    private int _attemptLimit;
    private bool _isUnlimitedTime;
    private DateTimeOffset _beginTime;
    private DateTimeOffset _endTime;
    private int _durationMinutes;
    private bool _isPublished;
    private DateTimeOffset _createdAt;
    private DateTimeOffset _updatedAt;
    private string? _userGroup;

    // 标记字段是否已被显式设置
    private bool _isIdSet;
    private bool _isNameSet;
    private bool _isOwnerIdSet;

    // 2. 私有构造函数
    private QuizTemplateBuilder()
    {
        Reset();
    }

    // 3. 静态工厂方法
    /// <summary>
    /// 创建一个新的 QuizTemplateBuilder 实例
    /// </summary>
    public static QuizTemplateBuilder New() => new();

    // 4. 属性设置方法（WithXxx）
    /// <summary>
    /// 设置 Id
    /// </summary>
    public QuizTemplateBuilder WithId(string value)
    {
        _id = value;
        _isIdSet = true;
        return this;
    }

    /// <summary>
    /// 设置 Name
    /// </summary>
    public QuizTemplateBuilder WithName(string value)
    {
        _name = value;
        _isNameSet = true;
        return this;
    }

    /// <summary>
    /// 设置 Description
    /// </summary>
    public QuizTemplateBuilder WithDescription(string value)
    {
        _description = value;
        return this;
    }

    /// <summary>
    /// 设置 Remark
    /// </summary>
    public QuizTemplateBuilder WithRemark(string value)
    {
        _remark = value;
        return this;
    }

    /// <summary>
    /// 设置 OwnerId
    /// </summary>
    public QuizTemplateBuilder WithOwnerId(string value)
    {
        _ownerId = value;
        _isOwnerIdSet = true;
        return this;
    }

    /// <summary>
    /// 设置 AttemptLimit（小于0表示无限次数）
    /// </summary>
    public QuizTemplateBuilder WithAttemptLimit(int value)
    {
        _attemptLimit = value;
        return this;
    }

    /// <summary>
    /// 设置 IsUnlimitedTime（true=无限时间，false=有限时间）
    /// </summary>
    public QuizTemplateBuilder WithIsUnlimitedTime(bool value)
    {
        _isUnlimitedTime = value;
        return this;
    }

    /// <summary>
    /// 设置为无限时间模式（便捷方法）
    /// </summary>
    public QuizTemplateBuilder UnlimitedTime()
    {
        _isUnlimitedTime = true;
        return this;
    }

    /// <summary>
    /// 设置为有限时间模式（便捷方法）
    /// </summary>
    public QuizTemplateBuilder LimitedTime()
    {
        _isUnlimitedTime = false;
        return this;
    }

    /// <summary>
    /// 设置 BeginTime
    /// </summary>
    public QuizTemplateBuilder WithBeginTime(DateTimeOffset value)
    {
        _beginTime = value;
        return this;
    }

    /// <summary>
    /// 设置 EndTime
    /// </summary>
    public QuizTemplateBuilder WithEndTime(DateTimeOffset value)
    {
        _endTime = value;
        return this;
    }

    /// <summary>
    /// 设置 DurationMinutes（限时分钟数）
    /// </summary>
    public QuizTemplateBuilder WithDurationMinutes(int value)
    {
        _durationMinutes = value;
        return this;
    }

    /// <summary>
    /// 设置 IsPublished
    /// </summary>
    public QuizTemplateBuilder WithIsPublished(bool value)
    {
        _isPublished = value;
        return this;
    }

    public QuizTemplateBuilder WithUserGroupId(string? userGroupId)
    {
        _userGroup = userGroupId;
        return this;
    }

    /// <summary>
    /// 设置 CreatedAt
    /// </summary>
    public QuizTemplateBuilder WithCreatedAt(DateTimeOffset value)
    {
        _createdAt = value;
        return this;
    }

    /// <summary>
    /// 设置 UpdatedAt
    /// </summary>
    public QuizTemplateBuilder WithUpdatedAt(DateTimeOffset value)
    {
        _updatedAt = value;
        return this;
    }

    // 6. 辅助方法（Clone, Reset）

    /// <summary>
    /// 克隆当前 Builder 状态
    /// </summary>
    public QuizTemplateBuilder Clone()
    {
        var clone = new QuizTemplateBuilder
        {
            _id = _id,
            _name = _name,
            _description = _description,
            _remark = _remark,
            _ownerId = _ownerId,
            _attemptLimit = _attemptLimit,
            _isUnlimitedTime = _isUnlimitedTime,
            _beginTime = _beginTime,
            _endTime = _endTime,
            _durationMinutes = _durationMinutes,
            _isPublished = _isPublished,
            _createdAt = _createdAt,
            _updatedAt = _updatedAt,
            _isIdSet = _isIdSet,
            _isNameSet = _isNameSet,
            _isOwnerIdSet = _isOwnerIdSet,
            _userGroup = null,
        };
        return clone;
    }

    /// <summary>
    /// 重置所有字段到默认值
    /// </summary>
    public QuizTemplateBuilder Reset()
    {
        _id = null!;
        _name = null!;
        _description = string.Empty;
        _remark = string.Empty;
        _ownerId = null!;
        _attemptLimit = -1;
        _isUnlimitedTime = true;
        _beginTime = DateTimeOffset.MinValue;
        _endTime = DateTimeOffset.MinValue;
        _durationMinutes = 0;
        _isPublished = false;
        _createdAt = DateTimeOffset.UtcNow;
        _updatedAt = DateTimeOffset.UtcNow;
        _userGroup = null;

        _isIdSet = false;
        _isNameSet = false;
        _isOwnerIdSet = false;

        return this;
    }

    // 7. 构建方法

    /// <summary>
    /// 构建单个 QuizTemplate 实例
    /// 自动生成所有未设置的必填字段
    /// </summary>
    public QuizTemplate Build()
    {
        // 自动生成未设置的必填字段
        if (!_isIdSet)
            _id = Guid.NewGuid().ToString();

        if (!_isNameSet)
            _name = Guid.NewGuid().ToString();

        if (!_isOwnerIdSet)
            _ownerId = $"user_{Guid.NewGuid():N}";

        // 验证必要字段
        if (string.IsNullOrWhiteSpace(_id))
            throw new InvalidOperationException("Id cannot be null or empty");
        if (string.IsNullOrWhiteSpace(_name))
            throw new InvalidOperationException("Name cannot be null or empty");
        if (string.IsNullOrWhiteSpace(_ownerId))
            throw new InvalidOperationException("OwnerId cannot be null or empty");

        // 使用无参构造函数 + 对象初始化器方式
        var entity = new QuizTemplate(
            _id,
            _name,
            _description,
            _remark,
            _ownerId,
            _attemptLimit,
            _isUnlimitedTime,
            _beginTime,
            _endTime,
            _durationMinutes,
            _userGroup
        );

        entity.IsPublished = _isPublished;

        return entity;
    }

    /// <summary>
    /// 批量构建 QuizTemplate 实例
    /// </summary>
    /// <param name="count">生成数量</param>
    public List<QuizTemplate> BuildMany(int count)
    {
        return BuildMany(count, null);
    }

    /// <summary>
    /// 批量构建 QuizTemplate 实例，支持自定义配置
    /// </summary>
    /// <param name="count">生成数量</param>
    /// <param name="configure">配置每个 Builder 的回调（参数：builder, 索引）</param>
    public List<QuizTemplate> BuildMany(int count, Action<QuizTemplateBuilder, int>? configure)
    {
        if (count <= 0)
            throw new ArgumentException("Count must be greater than 0", nameof(count));

        var results = new List<QuizTemplate>(count);
        for (int i = 0; i < count; i++)
        {
            var builder = this.Clone();
            configure?.Invoke(builder, i);
            results.Add(builder.Build());
        }
        return results;
    }
}
