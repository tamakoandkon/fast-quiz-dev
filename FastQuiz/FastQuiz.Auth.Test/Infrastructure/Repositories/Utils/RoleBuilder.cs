using FastQuiz.Auth.Domain.Entities;

namespace FastQuiz.Auth.Test.Infrastructure.Repositories.Utils;

internal class RoleBuilder
{
    private string? _id;
    private string? _tenantId;
    private string? _name;
    private string? _description;
    private bool? _isSystem;
    private bool? _isEnabled;
    private List<string>? _permissions;
    private DateTimeOffset? _createdAt;
    private DateTimeOffset? _updatedAt;

    private RoleBuilder() { }

    /// <summary>
    /// 创建 Builder 实例
    /// </summary>
    public static RoleBuilder New() => new();

    /// <summary>
    /// 设置 ID（如果为 null 或不设置，Build 时会自动生成）
    /// </summary>
    public RoleBuilder WithId(string? id)
    {
        _id = id;
        return this;
    }

    /// <summary>
    /// 设置随机 ID（使用 GUID）
    /// </summary>
    public RoleBuilder WithRandomId()
    {
        _id = Guid.NewGuid().ToString();
        return this;
    }

    /// <summary>
    /// 设置随机 ID（自定义前缀）
    /// </summary>
    public RoleBuilder WithRandomId(string prefix)
    {
        _id = $"{prefix}_{Guid.NewGuid():N}";
        return this;
    }

    /// <summary>
    /// 设置默认管理员角色 ID
    /// </summary>
    public RoleBuilder WithDefaultAdminRoleId()
    {
        _id = Role.DefaultAdminRoleId;
        return this;
    }

    /// <summary>
    /// 设置租户 ID（必须设置）
    /// </summary>
    public RoleBuilder WithTenantId(string tenantId)
    {
        _tenantId = tenantId;
        return this;
    }

    /// <summary>
    /// 设置租户 ID 为系统租户 ID
    /// </summary>
    public RoleBuilder WithSystemTenantId()
    {
        _tenantId = Tenant.DefaultTenantId;
        return this;
    }

    /// <summary>
    /// 设置名称（如果为 null 或不设置，Build 时会自动生成）
    /// </summary>
    public RoleBuilder WithName(string? name)
    {
        _name = name;
        return this;
    }

    /// <summary>
    /// 设置默认管理员角色名称
    /// </summary>
    public RoleBuilder WithDefaultAdminRoleName()
    {
        _name = Role.DefaultAdminRoleName;
        return this;
    }

    /// <summary>
    /// 设置描述（如果为 null 或不设置，Build 时使用空字符串）
    /// </summary>
    public RoleBuilder WithDescription(string? description)
    {
        _description = description;
        return this;
    }

    /// <summary>
    /// 设置是否为系统角色
    /// </summary>
    public RoleBuilder WithIsSystem(bool isSystem)
    {
        _isSystem = isSystem;
        return this;
    }

    /// <summary>
    /// 设置为系统角色
    /// </summary>
    public RoleBuilder AsSystemRole()
    {
        _isSystem = true;
        return this;
    }

    /// <summary>
    /// 设置是否启用
    /// </summary>
    public RoleBuilder WithIsEnabled(bool isEnabled)
    {
        _isEnabled = isEnabled;
        return this;
    }

    /// <summary>
    /// 设置为启用状态
    /// </summary>
    public RoleBuilder AsEnabled()
    {
        _isEnabled = true;
        return this;
    }

    /// <summary>
    /// 设置为禁用状态
    /// </summary>
    public RoleBuilder AsDisabled()
    {
        _isEnabled = false;
        return this;
    }

    /// <summary>
    /// 设置权限列表
    /// </summary>
    public RoleBuilder WithPermissions(List<string>? permissions)
    {
        _permissions = permissions;
        return this;
    }

    /// <summary>
    /// 设置权限列表（params 方式）
    /// </summary>
    public RoleBuilder WithPermissions(params string[] permissions)
    {
        _permissions = permissions.ToList();
        return this;
    }

    /// <summary>
    /// 添加单个权限
    /// </summary>
    public RoleBuilder AddPermission(string permission)
    {
        _permissions ??= [];
        _permissions.Add(permission);
        return this;
    }

    /// <summary>
    /// 添加多个权限
    /// </summary>
    public RoleBuilder AddPermissions(IEnumerable<string> permissions)
    {
        _permissions ??= [];
        _permissions.AddRange(permissions);
        return this;
    }

    /// <summary>
    /// 设置创建时间（如果为 null 或不设置，Build 时自动使用 UtcNow）
    /// </summary>
    public RoleBuilder WithCreatedAt(DateTimeOffset? createdAt)
    {
        _createdAt = createdAt;
        return this;
    }

    /// <summary>
    /// 设置更新时间（如果为 null 或不设置，Build 时自动使用 UtcNow）
    /// </summary>
    public RoleBuilder WithUpdatedAt(DateTimeOffset? updatedAt)
    {
        _updatedAt = updatedAt;
        return this;
    }

    /// <summary>
    /// 构建单个实体（所有未设置的属性自动生成）
    /// </summary>
    public Role Build()
    {
        if (string.IsNullOrEmpty(_tenantId))
        {
            _tenantId = Guid.NewGuid().ToString();
        }

        var id = string.IsNullOrEmpty(_id) ? Guid.NewGuid().ToString() : _id;
        var name = string.IsNullOrEmpty(_name) ? $"Role_{Guid.NewGuid():N}" : _name;
        var description = _description ?? string.Empty;
        var isSystem = _isSystem ?? false;
        var isEnabled = _isEnabled ?? true;
        var createdAt = _createdAt ?? DateTimeOffset.UtcNow;
        var updatedAt = _updatedAt ?? DateTimeOffset.UtcNow;

        var role = new Role(id, _tenantId, name, description, isSystem, isEnabled)
        {
            CreatedAt = createdAt,
            UpdatedAt = updatedAt,
        };

        if (_permissions != null && _permissions.Count != 0)
        {
            role.Permissions = [.. _permissions];
        }

        return role;
    }

    /// <summary>
    /// 批量构建实体（每个实体独立生成随机值）
    /// </summary>
    /// <param name="count">生成数量</param>
    /// <returns>实体列表</returns>
    public List<Role> BuildMany(int count)
    {
        if (count <= 0)
            throw new ArgumentException("生成数量必须大于0", nameof(count));

        var roles = new List<Role>(count);

        for (int i = 0; i < count; i++)
        {
            var currentId = _id;
            var currentName = _name;
            var currentDescription = _description;
            var currentIsSystem = _isSystem;
            var currentIsEnabled = _isEnabled;
            var currentPermissions = _permissions;
            var currentCreatedAt = _createdAt;
            var currentUpdatedAt = _updatedAt;

            try
            {
                if (string.IsNullOrEmpty(_id))
                    _id = null;

                if (string.IsNullOrEmpty(_name))
                    _name = null;

                _description = null;
                _isSystem = null;
                _isEnabled = null;
                _permissions = null;
                _createdAt = null;
                _updatedAt = null;

                var role = Build();
                roles.Add(role);
            }
            finally
            {
                _id = currentId;
                _name = currentName;
                _description = currentDescription;
                _isSystem = currentIsSystem;
                _isEnabled = currentIsEnabled;
                _permissions = currentPermissions;
                _createdAt = currentCreatedAt;
                _updatedAt = currentUpdatedAt;
            }
        }

        return roles;
    }

    /// <summary>
    /// 批量构建实体（使用相同配置，但每个实体的随机属性独立生成）
    /// </summary>
    /// <param name="count">生成数量</param>
    /// <param name="useSameRandomValues">是否使用相同的随机值（false:每个实体独立随机，true:所有实体共享相同的随机值）</param>
    public List<Role> BuildMany(int count, bool useSameRandomValues)
    {
        if (count <= 0)
            throw new ArgumentException("生成数量必须大于0", nameof(count));

        if (!useSameRandomValues)
            return BuildMany(count);

        var template = Build();
        var roles = new List<Role>(count);

        for (int i = 0; i < count; i++)
        {
            var role = new Role(
                template.Id,
                template.TenantId,
                template.Name,
                template.Description,
                template.IsSystem,
                template.IsEnabled
            )
            {
                CreatedAt = template.CreatedAt,
                UpdatedAt = template.UpdatedAt,
                Permissions = [.. template.Permissions],
            };
            roles.Add(role);
        }

        return roles;
    }
}
