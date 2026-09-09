namespace FastQuiz.Quiz.Domain.Entities;

/// <summary>
/// 要兼容几种模式：
/// 1. 可以设置次数：有限次数/无限次数
/// 2. 可以设置考试时间：有限时间/无限时间
/// </summary>
public class QuizTemplate
{
    public QuizTemplate(
        string id,
        string name,
        string description,
        string remark,
        string ownerId,
        int attemptLimit,
        bool isUnlimitedTime,
        DateTimeOffset beginTime,
        DateTimeOffset endTime,
        int durationMinutes,
        string? userGroupId
    )
    {
        Id = id;
        Name = name;
        Description = description;
        Remark = remark;
        OwnerId = ownerId;
        AttemptLimit = attemptLimit;
        IsUnlimitedTime = isUnlimitedTime;
        BeginTime = beginTime;
        EndTime = endTime;
        DurationMinutes = durationMinutes;
        UserGroupId = userGroupId;
    }

    private QuizTemplate() { }

    public string Id { get; set; } = null!;
    public string Name { get; set; } = null!;
    public string Description { get; set; } = string.Empty;
    public string Remark { get; set; } = string.Empty;
    public string OwnerId { get; set; } = null!;

    /// <summary>
    /// 小于0表示无限次数
    /// </summary>
    public int AttemptLimit { get; set; } = -1;

    /// <summary>
    /// true=无限时间，false=有限时间
    /// </summary>
    public bool IsUnlimitedTime { get; set; } = true;

    /// <summary>
    /// 有限时间模式下的开始时间（IsUnlimitedTime=false时有效）
    /// </summary>
    public DateTimeOffset BeginTime { get; set; }

    /// <summary>
    /// 有限时间模式下的结束时间（IsUnlimitedTime=false时有效）
    /// </summary>
    public DateTimeOffset EndTime { get; set; }

    /// <summary>
    /// 限时分钟数
    /// </summary>
    public int DurationMinutes { get; set; } = 0;

    public bool IsPublished { get; set; } = false;
    public string? UserGroupId { get; set; } = null;

    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
    public DateTimeOffset UpdatedAt { get; set; } = DateTimeOffset.UtcNow;
}
