import type { RoleDto } from "./backEnd";

type RoleIdentity = Pick<RoleDto, "id" | "name" | "isSystem">;
type RoleDescription = RoleIdentity & Pick<RoleDto, "description" | "tenantId">;

const tenantAdminDescription = "管理此租户的用户、组织、角色、题库和考试。";
const permissionNames: Readonly<Record<string, string>> = {
  system: "系统管理",
  tenant: "租户管理",
  organization: "组织管理",
  role: "角色管理",
  user: "用户管理",
  question: "题库管理",
  "quiz-template": "试卷管理",
  "quiz-snapshot": "生成试卷与快照管理",
  "quiz-attempt": "答卷、阅卷与成绩管理",
};

function isTenantAdmin(role: RoleIdentity) {
  return role.isSystem === true && role.name === "tenant admin";
}

export function getRoleDisplayName(role: RoleIdentity): string {
  if (isTenantAdmin(role)) return "租户管理员";
  if (role.id === "default_admin" && role.name === "system admin") return "系统管理员";
  return role.name?.trim() || "未命名角色";
}

export function getRoleDisplayDescription(role: RoleDescription, tenantName?: string | null): string {
  const description = role.description?.trim() || "";
  const legacyDescription = role.tenantId
    ? `租户 ${role.tenantId} 的租户级管理员`
    : undefined;

  if (role.isSystem && (
    description === legacyDescription ||
    description === tenantAdminDescription ||
    (isTenantAdmin(role) && !description)
  )) {
    return tenantName?.trim()
      ? `管理「${tenantName.trim()}」的用户、组织、角色、题库和考试。`
      : tenantAdminDescription;
  }
  if (role.id === "default_admin" && !description) return "管理系统设置和所有租户。";
  return description;
}

export function getPermissionDisplayName(permission: string): string {
  return Object.hasOwn(permissionNames, permission) ? permissionNames[permission]! : "其他权限";
}
