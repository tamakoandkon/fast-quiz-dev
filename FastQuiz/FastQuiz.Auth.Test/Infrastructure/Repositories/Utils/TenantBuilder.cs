using FastQuiz.Auth.Domain.Entities;

namespace FastQuiz.Auth.Test.Infrastructure.Repositories.Utils;

internal class TenantBuilder
{
    private string? _id;
    private string? _name;
    private string? _description;
    private DateTimeOffset? _createdAt;
    private DateTimeOffset? _updatedAt;

    private TenantBuilder() { }

    /// <summary>
    /// 创建 Builder 实例
    /// </summary>
    public static TenantBuilder New() => new();

    /// <summary>
    /// 设置 ID（如果为 null 或不设置，Build 时会自动生成）
    /// </summary>
    public TenantBuilder WithId(string? id)
    {
        _id = id;
        return this;
    }

    /// <summary>
    /// 设置随机 ID（使用 GUID）
    /// </summary>
    public TenantBuilder WithRandomId()
    {
        _id = Guid.NewGuid().ToString();
        return this;
    }

    /// <summary>
    /// 设置随机 ID（自定义前缀）
    /// </summary>
    public TenantBuilder WithRandomId(string prefix)
    {
        _id = $"{prefix}_{Guid.NewGuid():N}";
        return this;
    }

    /// <summary>
    /// 设置系统租户 ID（使用预定义的 DefaultTenantId）
    /// </summary>
    public TenantBuilder WithSystemTenantId()
    {
        _id = Tenant.DefaultTenantId;
        return this;
    }

    /// <summary>
    /// 设置名称（如果为 null 或不设置，Build 时会自动生成）
    /// </summary>
    public TenantBuilder WithName(string? name)
    {
        _name = name;
        return this;
    }

    /// <summary>
    /// 设置系统租户名称（使用预定义的 DefaultTenantName）
    /// </summary>
    public TenantBuilder WithSystemTenantName()
    {
        _name = Tenant.DefaultTenantName;
        return this;
    }

    /// <summary>
    /// 设置描述（如果为 null 或不设置，Build 时保持 null）
    /// </summary>
    public TenantBuilder WithDescription(string? description)
    {
        _description = description;
        return this;
    }

    /// <summary>
    /// 设置创建时间（如果为 null 或不设置，Build 时自动使用 UtcNow）
    /// </summary>
    public TenantBuilder WithCreatedAt(DateTimeOffset? createdAt)
    {
        _createdAt = createdAt;
        return this;
    }

    /// <summary>
    /// 设置更新时间（如果为 null 或不设置，Build 时自动使用 UtcNow）
    /// </summary>
    public TenantBuilder WithUpdatedAt(DateTimeOffset? updatedAt)
    {
        _updatedAt = updatedAt;
        return this;
    }

    /// <summary>
    /// 构建单个实体（所有未设置的属性自动生成）
    /// </summary>
    public Tenant Build()
    {
        var id = string.IsNullOrEmpty(_id) ? Guid.NewGuid().ToString() : _id;
        var name = string.IsNullOrEmpty(_name) ? $"Tenant_{Guid.NewGuid():N}" : _name;
        var createdAt = _createdAt ?? DateTimeOffset.UtcNow;
        var updatedAt = _updatedAt ?? DateTimeOffset.UtcNow;

        var tenant = new Tenant(id, name, _description ?? string.Empty)
        {
            CreatedAt = createdAt,
            UpdatedAt = updatedAt,
        };

        return tenant;
    }

    /// <summary>
    /// 批量构建实体（每个实体独立生成随机值）
    /// </summary>
    /// <param name="count">生成数量</param>
    /// <returns>实体列表</returns>
    public List<Tenant> BuildMany(int count)
    {
        if (count <= 0)
            throw new ArgumentException("生成数量必须大于0", nameof(count));

        var tenants = new List<Tenant>(count);

        for (int i = 0; i < count; i++)
        {
            var currentId = _id;
            var currentName = _name;
            var currentDescription = _description;
            var currentCreatedAt = _createdAt;
            var currentUpdatedAt = _updatedAt;

            try
            {
                if (string.IsNullOrEmpty(_id))
                    _id = null;

                if (string.IsNullOrEmpty(_name))
                    _name = null;

                _createdAt = null;
                _updatedAt = null;

                var tenant = Build();
                tenants.Add(tenant);
            }
            finally
            {
                _id = currentId;
                _name = currentName;
                _description = currentDescription;
                _createdAt = currentCreatedAt;
                _updatedAt = currentUpdatedAt;
            }
        }

        return tenants;
    }

    /// <summary>
    /// 批量构建实体（使用相同配置，但每个实体的随机属性独立生成）
    /// </summary>
    /// <param name="count">生成数量</param>
    /// <param name="useSameRandomValues">是否使用相同的随机值（false:每个实体独立随机，true:所有实体共享相同的随机值）</param>
    public List<Tenant> BuildMany(int count, bool useSameRandomValues)
    {
        if (count <= 0)
            throw new ArgumentException("生成数量必须大于0", nameof(count));

        if (!useSameRandomValues)
            return BuildMany(count);

        var template = Build();
        var tenants = new List<Tenant>(count);

        for (int i = 0; i < count; i++)
        {
            var tenant = new Tenant(template.Id, template.Name, template.Description)
            {
                CreatedAt = template.CreatedAt,
                UpdatedAt = template.UpdatedAt,
            };
            tenants.Add(tenant);
        }

        return tenants;
    }
}
