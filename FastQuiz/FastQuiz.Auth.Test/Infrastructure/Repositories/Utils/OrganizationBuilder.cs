using FastQuiz.Auth.Domain.Entities;

namespace FastQuiz.Auth.Test.Infrastructure.Repositories.Utils;

internal class OrganizationBuilder
{
    private string? _id;
    private string? _tenantId;
    private string? _parentId;
    private string? _name;
    private DateTimeOffset? _createdAt;
    private DateTimeOffset? _updatedAt;

    private OrganizationBuilder() { }

    /// <summary>
    /// 创建 Builder 实例
    /// </summary>
    public static OrganizationBuilder New() => new();

    /// <summary>
    /// 设置 ID（如果为 null 或不设置，Build 时会自动生成）
    /// </summary>
    public OrganizationBuilder WithId(string? id)
    {
        _id = id;
        return this;
    }

    /// <summary>
    /// 设置随机 ID（使用 GUID）
    /// </summary>
    public OrganizationBuilder WithRandomId()
    {
        _id = Guid.NewGuid().ToString();
        return this;
    }

    /// <summary>
    /// 设置随机 ID（自定义前缀）
    /// </summary>
    public OrganizationBuilder WithRandomId(string prefix)
    {
        _id = $"{prefix}_{Guid.NewGuid():N}";
        return this;
    }

    /// <summary>
    /// 设置默认组织 ID（使用预定义的 DefaultOrganizationId）
    /// </summary>
    public OrganizationBuilder WithDefaultOrganizationId()
    {
        _id = Organization.DefaultOrganizationId;
        return this;
    }

    /// <summary>
    /// 设置租户 ID（如果为 null 或不设置，Build 时会自动生成）
    /// </summary>
    public OrganizationBuilder WithTenantId(string? tenantId)
    {
        _tenantId = tenantId;
        return this;
    }

    /// <summary>
    /// 设置随机租户 ID（使用 GUID）
    /// </summary>
    public OrganizationBuilder WithRandomTenantId()
    {
        _tenantId = Guid.NewGuid().ToString();
        return this;
    }

    /// <summary>
    /// 设置父级 ID
    /// </summary>
    public OrganizationBuilder WithParentId(string? parentId)
    {
        _parentId = parentId;
        return this;
    }

    /// <summary>
    /// 设置名称（如果为 null 或不设置，Build 时会自动生成）
    /// </summary>
    public OrganizationBuilder WithName(string? name)
    {
        _name = name;
        return this;
    }

    /// <summary>
    /// 设置默认组织名称（使用预定义的 DefaultOrganizationName）
    /// </summary>
    public OrganizationBuilder WithDefaultOrganizationName()
    {
        _name = Organization.DefaultOrganizationName;
        return this;
    }

    /// <summary>
    /// 设置创建时间（如果为 null 或不设置，Build 时自动使用 UtcNow）
    /// </summary>
    public OrganizationBuilder WithCreatedAt(DateTimeOffset? createdAt)
    {
        _createdAt = createdAt;
        return this;
    }

    /// <summary>
    /// 设置更新时间（如果为 null 或不设置，Build 时自动使用 UtcNow）
    /// </summary>
    public OrganizationBuilder WithUpdatedAt(DateTimeOffset? updatedAt)
    {
        _updatedAt = updatedAt;
        return this;
    }

    /// <summary>
    /// 构建单个实体（所有未设置的属性自动生成）
    /// </summary>
    public Organization Build()
    {
        var id = string.IsNullOrEmpty(_id) ? Guid.NewGuid().ToString() : _id;
        var tenantId = string.IsNullOrEmpty(_tenantId) ? Guid.NewGuid().ToString() : _tenantId;
        var name = string.IsNullOrEmpty(_name) ? $"Organization_{Guid.NewGuid():N}" : _name;
        var createdAt = _createdAt ?? DateTimeOffset.UtcNow;
        var updatedAt = _updatedAt ?? DateTimeOffset.UtcNow;

        var organization = new Organization(id, tenantId, _parentId, name)
        {
            CreatedAt = createdAt,
            UpdatedAt = updatedAt,
        };

        return organization;
    }

    /// <summary>
    /// 批量构建实体（每个实体独立生成随机值）
    /// </summary>
    /// <param name="count">生成数量</param>
    /// <returns>实体列表</returns>
    public List<Organization> BuildMany(int count)
    {
        if (count <= 0)
            throw new ArgumentException("生成数量必须大于0", nameof(count));

        var organizations = new List<Organization>(count);

        for (int i = 0; i < count; i++)
        {
            var currentId = _id;
            var currentTenantId = _tenantId;
            var currentParentId = _parentId;
            var currentName = _name;
            var currentCreatedAt = _createdAt;
            var currentUpdatedAt = _updatedAt;

            try
            {
                if (string.IsNullOrEmpty(_id))
                    _id = null;

                if (string.IsNullOrEmpty(_tenantId))
                    _tenantId = null;

                if (string.IsNullOrEmpty(_name))
                    _name = null;

                _createdAt = null;
                _updatedAt = null;

                var organization = Build();
                organizations.Add(organization);
            }
            finally
            {
                _id = currentId;
                _tenantId = currentTenantId;
                _parentId = currentParentId;
                _name = currentName;
                _createdAt = currentCreatedAt;
                _updatedAt = currentUpdatedAt;
            }
        }

        return organizations;
    }

    /// <summary>
    /// 批量构建实体（使用相同配置，但每个实体的随机属性独立生成）
    /// </summary>
    /// <param name="count">生成数量</param>
    /// <param name="useSameRandomValues">是否使用相同的随机值（false:每个实体独立随机，true:所有实体共享相同的随机值）</param>
    public List<Organization> BuildMany(int count, bool useSameRandomValues)
    {
        if (count <= 0)
            throw new ArgumentException("生成数量必须大于0", nameof(count));

        if (!useSameRandomValues)
            return BuildMany(count);

        var template = Build();
        var organizations = new List<Organization>(count);

        for (int i = 0; i < count; i++)
        {
            var organization = new Organization(
                template.Id,
                template.TenantId,
                template.ParentId,
                template.Name
            )
            {
                CreatedAt = template.CreatedAt,
                UpdatedAt = template.UpdatedAt,
            };
            organizations.Add(organization);
        }

        return organizations;
    }
}
