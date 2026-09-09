using FastQuiz.Auth.Domain.Entities;

namespace FastQuiz.Auth.Test.Infrastructure.Repositories.Utils;

internal class UserBuilder
{
    private string? _id;
    private string? _username;
    private string? _email;
    private string? _passwordHash;

    private UserBuilder() { }

    /// <summary>
    /// 创建 Builder 实例
    /// </summary>
    public static UserBuilder New() => new();

    /// <summary>
    /// 设置 ID（如果为 null 或不设置，Build 时会自动生成）
    /// </summary>
    public UserBuilder WithId(string? id)
    {
        _id = id;
        return this;
    }

    /// <summary>
    /// 设置随机 ID（使用 GUID）
    /// </summary>
    public UserBuilder WithRandomId()
    {
        _id = Guid.NewGuid().ToString();
        return this;
    }

    /// <summary>
    /// 设置用户名（如果为 null 或不设置，Build 时会自动生成）
    /// </summary>
    public UserBuilder WithUsername(string? username)
    {
        _username = username;
        return this;
    }

    /// <summary>
    /// 设置邮箱（如果为 null 或不设置，Build 时会自动生成）
    /// </summary>
    public UserBuilder WithEmail(string? email)
    {
        _email = email;
        return this;
    }

    /// <summary>
    /// 设置密码哈希（如果为 null 或不设置，Build 时会自动生成）
    /// </summary>
    public UserBuilder WithPasswordHash(string? passwordHash)
    {
        _passwordHash = passwordHash;
        return this;
    }

    /// <summary>
    /// 构建单个实体（所有未设置的属性自动生成）
    /// </summary>
    public User Build()
    {
        var id = string.IsNullOrEmpty(_id) ? Guid.NewGuid().ToString() : _id;
        var username = string.IsNullOrEmpty(_username) ? $"User_{Guid.NewGuid():N}" : _username;
        var email = string.IsNullOrEmpty(_email) ? $"{Guid.NewGuid()}@example.com" : _email;
        var passwordHash = string.IsNullOrEmpty(_passwordHash)
            ? Guid.NewGuid().ToString()
            : _passwordHash;

        return new User(id, username, email, passwordHash);
    }

    /// <summary>
    /// 批量构建实体（每个实体独立生成随机值）
    /// </summary>
    /// <param name="count">生成数量</param>
    /// <returns>实体列表</returns>
    public List<User> BuildMany(int count)
    {
        if (count <= 0)
            throw new ArgumentException("生成数量必须大于0", nameof(count));

        var users = new List<User>(count);

        for (int i = 0; i < count; i++)
        {
            var currentId = _id;
            var currentUsername = _username;
            var currentEmail = _email;
            var currentPasswordHash = _passwordHash;

            try
            {
                if (string.IsNullOrEmpty(_id))
                    _id = null;

                if (string.IsNullOrEmpty(_username))
                    _username = null;

                if (string.IsNullOrEmpty(_email))
                    _email = null;

                if (string.IsNullOrEmpty(_passwordHash))
                    _passwordHash = null;

                var user = Build();
                users.Add(user);
            }
            finally
            {
                _id = currentId;
                _username = currentUsername;
                _email = currentEmail;
                _passwordHash = currentPasswordHash;
            }
        }

        return users;
    }

    /// <summary>
    /// 批量构建实体（使用相同配置，但每个实体的随机属性独立生成）
    /// </summary>
    /// <param name="count">生成数量</param>
    /// <param name="useSameRandomValues">是否使用相同的随机值（false:每个实体独立随机，true:所有实体共享相同的随机值）</param>
    public List<User> BuildMany(int count, bool useSameRandomValues)
    {
        if (count <= 0)
            throw new ArgumentException("生成数量必须大于0", nameof(count));

        if (!useSameRandomValues)
            return BuildMany(count);

        var template = Build();
        var users = new List<User>(count);

        for (int i = 0; i < count; i++)
        {
            var user = new User(
                template.Id,
                template.Username,
                template.Email,
                template.PasswordHash
            );
            users.Add(user);
        }

        return users;
    }
}
