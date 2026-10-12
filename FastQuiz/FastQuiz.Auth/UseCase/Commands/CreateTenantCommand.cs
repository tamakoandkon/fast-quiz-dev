namespace FastQuiz.Auth.UseCase.Commands;

/// <summary>
/// 创建租户命令，同时携带该租户的初始管理员账号信息。
/// 租户管理员由系统在创建租户时一并创建，外部不能单独将用户加入租户。
/// </summary>
public record CreateTenantCommand(
    string Name,
    string Description,
    string AdminUsername,
    string AdminPassword,
    string? AdminEmail
);
