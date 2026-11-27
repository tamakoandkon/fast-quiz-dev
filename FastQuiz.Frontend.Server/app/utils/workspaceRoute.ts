const dashboardModules = [
  { label: "概览", segment: "", icon: "i-lucide-house", section: "工作空间" },
  { label: "考试中心", segment: "/quiz-attempt", icon: "i-lucide-graduation-cap", section: "工作空间" },
  { label: "我的考试", segment: "/my-quiz-attempt", icon: "i-lucide-chart-no-axes-column-increasing", section: "工作空间" },
  { label: "试卷管理", segment: "/dash/quiz-template", icon: "i-lucide-file-text", section: "内容管理", permission: "quiz-template" },
  { label: "题库管理", segment: "/dash/question-group", icon: "i-lucide-library", section: "内容管理", permission: "question" },
  { label: "组织管理", segment: "/dash/organization", icon: "i-lucide-building-2", section: "团队与设置", permission: "organization" },
  { label: "用户管理", segment: "/dash/user", icon: "i-lucide-users", section: "团队与设置", permission: "user" },
  { label: "角色管理", segment: "/dash/role", icon: "i-lucide-shield-check", section: "团队与设置", permission: "role" },
] as const;

const tenantList = { label: "我的租户", icon: "i-lucide-grid-2x2", to: "/tenant", section: "切换空间" };
const tenantManagement = {
  label: "租户管理", icon: "i-lucide-settings-2", to: "/tenant/manage",
  section: "团队与设置", permission: "tenant",
};

interface WorkspaceNavigationItem {
  label: string;
  icon: string;
  section: string;
  to: string;
  permission?: string;
}

export function workspaceNavigation(base: string, tenantOnly: boolean, showTenantManagement: boolean): WorkspaceNavigationItem[] {
  if (tenantOnly) {
    return showTenantManagement ? [tenantList, tenantManagement] : [tenantList];
  }
  return [
    ...dashboardModules.map(({ segment, ...item }) => ({ ...item, to: base + segment })),
    tenantManagement,
    tenantList,
  ];
}

interface WorkspaceCrumb {
  label: string;
  to: string;
}

export function workspaceLocation(path: string) {
  const normalized = path.replace(/\/+$/, "");
  const base = normalized.match(/^\/tenant\/[^/]+/)?.[0] ?? "";
  const relative = normalized.slice(base.length);
  const modulePath = relative.startsWith("/dash/user-records/") ? "/dash/user" : relative;
  const module = dashboardModules.find(item => item.segment &&
    (modulePath === item.segment || modulePath.startsWith(item.segment + "/"))) ?? dashboardModules[0];
  const root = normalized === tenantManagement.to ? tenantManagement
    : normalized === tenantList.to ? tenantList
      : { ...module, to: base + module.segment };

  const parents: WorkspaceCrumb[] = [];
  let title: string = root.label;
  if (normalized === root.to || !base) return { root, parents, title };

  parents.push({ label: root.label, to: root.to });
  if (relative.startsWith("/dash/user-records/")) return { root, parents, title: "考试成绩" };

  const [entity, tab, nested] = normalized.slice(root.to.length + 1).split("/");
  if (module.segment === "/dash/quiz-template") {
    title = "试卷详情";
    const detail = root.to + "/" + entity;
    if (tab) {
      parents.push({ label: title, to: detail });
      title = tab === "snapshots" ? "试卷样本" : tab === "review" ? "批改试卷" : "考试成绩";
      if (nested) {
        parents.push({ label: title, to: detail + "/" + tab });
        title = tab === "snapshots" ? "样卷详情" : "答卷详情";
      }
    }
  } else if (module.segment === "/quiz-attempt") {
    if (tab === "result") {
      parents[0] = { label: "我的考试", to: base + "/my-quiz-attempt" };
      return { root: { ...dashboardModules[2], to: base + "/my-quiz-attempt" }, parents, title: "考试结果" };
    }
    title = tab === "review" ? "答卷详情" : "正在答题";
  } else {
    title = module.segment === "/dash/user" ? "用户资料"
      : module.segment === "/dash/organization" ? "组织成员"
        : module.segment === "/dash/question-group" ? "题库详情" : root.label;
  }

  return { root, parents, title };
}
