using FastQuiz.Common.Models;

namespace FastQuiz.Auth.Infrastructure.Repositories;

public class AuthDbContextSettings
{
    public string ConnectionString { get; set; } = null!;
    public DatabaseType Type { get; set; } = DatabaseType.Sqlite;
    public string UserTableName { get; set; } = "Users";
    public string TenantTableName { get; set; } = "Tenants";
    public string OrganizationTableName { get; set; } = "Organizations";
    public string RoleTableName { get; set; } = "Roles";
    public string UserRoleTableName { get; set; } = "UserRoles";
    public string RolePermissionTableName { get; set; } = "RolePermissions";
    public string TenantUserMapTableName { get; set; } = "TenantUserMaps";
    public string OrganizationUserMapTableName { get; set; } = "OrganizationUserMaps";
}
