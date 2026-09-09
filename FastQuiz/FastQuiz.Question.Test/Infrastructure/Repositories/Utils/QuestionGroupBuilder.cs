using FastQuiz.Question.Domain.Entities;

namespace FastQuiz.Question.Test.Infrastructure.Repositories.Utils;

internal class QuestionGroupBuilder
{
    private string? _id;
    private string? _name;
    private string? _remark;
    private string? _ownerId;

    private QuestionGroupBuilder() { }

    /// <summary>
    /// 创建 Builder 实例
    /// </summary>
    public static QuestionGroupBuilder New() => new();

    /// <summary>
    /// 设置 ID（如果为 null 或不设置，Build 时会自动生成）
    /// </summary>
    public QuestionGroupBuilder WithId(string? id)
    {
        _id = id;
        return this;
    }

    /// <summary>
    /// 设置随机 ID（使用 GUID）
    /// </summary>
    public QuestionGroupBuilder WithRandomId()
    {
        _id = Guid.NewGuid().ToString();
        return this;
    }

    /// <summary>
    /// 设置随机 ID（自定义前缀）
    /// </summary>
    public QuestionGroupBuilder WithRandomId(string prefix)
    {
        _id = $"{prefix}_{Guid.NewGuid():N}";
        return this;
    }

    /// <summary>
    /// 设置名称（如果为 null 或不设置，Build 时会自动生成）
    /// </summary>
    public QuestionGroupBuilder WithName(string? name)
    {
        _name = name;
        return this;
    }

    /// <summary>
    /// 设置备注（如果为 null 或不设置，Build 时保持 null）
    /// </summary>
    public QuestionGroupBuilder WithRemark(string? remark)
    {
        _remark = remark;
        return this;
    }

    /// <summary>
    /// 设置拥有者 ID（如果为 null 或不设置，Build 时会自动生成）
    /// </summary>
    public QuestionGroupBuilder WithOwnerId(string? ownerId)
    {
        _ownerId = ownerId;
        return this;
    }

    /// <summary>
    /// 设置随机拥有者 ID
    /// </summary>
    public QuestionGroupBuilder WithRandomOwnerId()
    {
        _ownerId = Guid.NewGuid().ToString();
        return this;
    }

    /// <summary>
    /// 构建单个实体（所有未设置的属性自动使用 GUID 生成）
    /// </summary>
    public QuestionGroup Build()
    {
        var id = string.IsNullOrEmpty(_id) ? Guid.NewGuid().ToString() : _id;

        var name = string.IsNullOrEmpty(_name) ? $"QuestionGroup_{Guid.NewGuid():N}" : _name;

        var ownerId = string.IsNullOrEmpty(_ownerId) ? Guid.NewGuid().ToString() : _ownerId;

        return new QuestionGroup(id, ownerId, name, _remark);
    }

    /// <summary>
    /// 批量构建实体（每个实体独立生成随机值）
    /// </summary>
    /// <param name="count">生成数量</param>
    /// <returns>实体列表</returns>
    public List<QuestionGroup> BuildMany(int count)
    {
        if (count <= 0)
            throw new ArgumentException("生成数量必须大于0", nameof(count));

        var groups = new List<QuestionGroup>(count);

        for (int i = 0; i < count; i++)
        {
            var currentId = _id;
            var currentName = _name;
            var currentOwnerId = _ownerId;
            var currentRemark = _remark;

            try
            {
                if (string.IsNullOrEmpty(_id))
                    _id = null;

                if (string.IsNullOrEmpty(_name))
                    _name = null;

                if (string.IsNullOrEmpty(_ownerId))
                    _ownerId = null;

                var group = Build();
                groups.Add(group);
            }
            finally
            {
                _id = currentId;
                _name = currentName;
                _ownerId = currentOwnerId;
                _remark = currentRemark;
            }
        }

        return groups;
    }

    /// <summary>
    /// 批量构建实体（使用相同配置，但每个实体的随机属性独立生成）
    /// </summary>
    /// <param name="count">生成数量</param>
    /// <param name="useSameRandomValues">是否使用相同的随机值（false:每个实体独立随机，true:所有实体共享相同的随机值）</param>
    public List<QuestionGroup> BuildMany(int count, bool useSameRandomValues)
    {
        if (count <= 0)
            throw new ArgumentException("生成数量必须大于0", nameof(count));

        if (!useSameRandomValues)
            return BuildMany(count);

        var template = Build();
        var groups = new List<QuestionGroup>(count);

        for (int i = 0; i < count; i++)
        {
            var group = new QuestionGroup(
                template.Id,
                template.OwnerId,
                template.Name,
                template.Remark
            );
            groups.Add(group);
        }

        return groups;
    }
}
