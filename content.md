# 页面组成以及需求

> 更新：2026-10-14；本次成员与预生成试卷功能基于合并 PR #15 后的 `main`（`ba78b89`）。
> 工程债务见 [重构行动清单](docs/refactor-harden-roadmap.md)，实际执行结果见 [测试记录](docs/verification-2026-10-14.md)。

状态约定：`[x]` 表示已找到相关实现，不代表每个页面都经过浏览器端到端验收；`[ ]` 必须结合“未实现 / 部分完成 / 可选”说明阅读。权限以服务端校验为准。原始需求保留在 Git 历史中。

## 用户模块

### 租户管理

- [x] 租户管理总览、创建、修改与详情页面及接口。
- [x] 我的租户快捷列表、切换租户页面与快捷入口；读取当前用户的租户。
- [x] 系统管理员读取所有租户，管理操作有权限校验。
- [x] 添加已有用户到租户 / 添加到其他租户：系统管理员在租户管理列表点击“添加已有账号”，输入完整用户名即可加入，不重复注册、不修改密码、不自动授予角色；重复/并发请求幂等。

入口：[租户 API](FastQuiz/FastQuiz.Auth/Interface/TenantApi.cs)、[用户 API](FastQuiz/FastQuiz.Auth/Interface/AuthApi.cs)、[租户页面](FastQuiz.Frontend.Server/app/pages/tenant)。

验收：跨租户加入、原成员关系及密码保留、不额外授权、重复/并发请求、非系统管理员、非法用户名、未知账号/租户与未登录场景有 API 回归；隔离浏览器已验证新租户出现在原账号列表中。完整契约见 [功能与验证说明](docs/tenant-membership-and-prebuilt-quiz.md)。

### 组织管理

- [x] 租户组织总览、增加、修改、删除组织的页面与接口。
- [x] 组织详情、子组织列表及组织内用户列表。
- [x] 将已有用户添加到组织、从组织移除用户。
- [ ] **未实现：我的组织及其中所有用户。** 管理员组织列表不等于按当前用户成员关系查询的专用页面和接口。
- [ ] **未实现：从组织直接创建用户。** 当前弹窗只能选择已有用户，没有“新建账号并加入当前组织”的组合流程。

入口：[组织 API](FastQuiz/FastQuiz.Auth/Interface/OrganizationApi.cs)、[组织页面](FastQuiz.Frontend.Server/app/pages/tenant/[tenantId]/dash/organization)、[添加成员弹窗](FastQuiz.Frontend.Server/app/components/Organization/AddUserToOrgModal.vue)。

待办验收：我的组织仅返回本人所属组织，成员可见范围经服务端授权；组织内创建用户同时验证用户与组织管理权限，并定义创建或入组失败的回滚规则。

### 用户管理与个人账号

- [x] 用户列表、详情、新建、修改页面与接口；可从组织页面添加已有成员。
- [x] 管理员移除租户用户。**语义是解除租户成员关系，不是删除共享的全局账号。** 受保护账号及跨租户操作有回归。
- [x] 登录、注册、读取与修改个人信息；退出登录清理客户端登录状态。
- [x] CSV 导入用户入口；复用逐个创建接口，不代表服务端整批事务导入。
- [x] 添加到其他租户：复用租户管理的系统管理员入口，与上方同一功能合并跟踪，不是向普通租户管理员开放全局账号搜索。
- [ ] **未实现：用户本人注销账号。** 原需求“注销用户”按销户理解，区别于退出登录和管理员移除成员。实施前需明确身份复核、历史考试数据保留及共享账号规则。

入口：[用户服务](FastQuiz/FastQuiz.Auth/UseCase/Services/UserApplicationService.cs)、[用户页面](FastQuiz.Frontend.Server/app/pages/tenant/[tenantId]/dash/user)、[认证页面](FastQuiz.Frontend.Server/app/pages/(auth))、[安全测试](FastQuiz/FastQuiz.API.Test/Auth/UserManagementSecurityTests.cs)。

### 角色管理

- [x] 角色列表、增加、修改、删除页面与接口。
- [x] 权限列表、角色权限详情、用户角色分配与撤销。
- [x] 分配/撤销校验操作管理员而非被操作用户，变更后失效权限缓存。

入口：[角色 API](FastQuiz/FastQuiz.Auth/Interface/RoleApi.cs)、[角色页面](FastQuiz.Frontend.Server/app/pages/tenant/[tenantId]/dash/role)、[角色测试](FastQuiz/FastQuiz.API.Test/Auth/RoleServiceTests.cs)。

## 题库模块

原文此处只有标题；以下补录当前已有范围，不表示重新承诺额外功能。

- [x] 题库分组列表、创建、编辑、删除与详情。
- [x] 分组内题目列表、新增、编辑、删除，题型及答案配置。
- [x] 前端 CSV 导入题目入口；不等同于服务端原子批量导入。

入口：[题库模块](FastQuiz/FastQuiz.Question)、[题库页面](FastQuiz.Frontend.Server/app/pages/tenant/[tenantId]/dash/question-group)、[API 测试](FastQuiz/FastQuiz.API.Test/Question)。

## 考试模块

### 已实现的主流程

- [x] 试卷模板管理、抽题规则、快照生成与发布。
- [x] 我的考试、可用考试列表、单个考试与试题列表。
- [x] 创建本人考试、进入答题、保存答案、提交与合法状态转换。
- [x] 多选完整答案集保存、空数组清空、请求内事务落库；提交后禁止改答案。
- [x] 成绩/结果页、本人历史记录、管理员记录与阅卷页面。
- [x] 客观评分、人工判题、阅卷进度及完成守卫；未判定为 `null`、判错为 `false`。
- [x] 答题归属、租户边界、阅卷权限与结果可见性校验。

入口：[考试 API](FastQuiz/FastQuiz.Quiz/Interface/QuizAttemptApi.cs)、[用例服务](FastQuiz/FastQuiz.Quiz/UseCase/Services)、[考试页面](FastQuiz.Frontend.Server/app/pages/tenant/[tenantId]/quiz-attempt)、[API 测试](FastQuiz/FastQuiz.API.Test/Quiz)。

### 预生成试卷与可选能力

- [x] 按 `Count` 批量生成：每批总计 1–100 份，校验快照属于当前模板；所有试卷与题目记录在一个事务内创建，后续失败整批回滚。
- [x] 管理员在样本列表点击“批量生成”，查看按游标分页的待领取库存；仅有考试管理权限且属于当前租户的用户可操作。
- [x] 考生在考试中心点击“领取预生成试卷”，系统从当前考试库存绑定一份给本人并进入答题；原指定 ID 领取接口也已开放并校验身份。
- [x] 领取验证租户、发布状态、开放时间、指定组织、未结束考试和次数限额；普通动态创建与领取共用模板级数据库锁，避免并发抢卷和绕过限额。
- [x] 数量/输入、跨模板/租户、分页、权限、失败回滚、并发抢同一份卷、普通创建与领取并发等有回归；隔离浏览器验证批量生成、库存刷新、领取进入答题。
- [ ] **扩展而非本次范围：管理员指定他人分配。** 本次交付的是考生自助领取，不新增代领或强制“只许预生成卷”模式；原动态创建考试入口保留。
- [ ] **可选：手动组卷。** [原始设计](FastQuiz/README.md) 定义为“未来可能支持”，不是当前主流程的必交付功能。

契约、操作步骤与测试边界见 [成员加入与预生成试卷说明](docs/tenant-membership-and-prebuilt-quiz.md)。PostgreSQL 专项 CI、MariaDB 升级验收与多实例故障演练仍不能用 SQLite/API 和浏览器通过替代。

## 推进与维护

1. 继续推进可靠任务调度与数据库升级验收，再补我的组织及组织内建用户。
2. 明确销户的数据保留规则后实施；手动组卷另行排期。
3. 后续功能 PR 同步此文、重构清单与实际测试记录；未实现或未执行的验收不得直接勾选。
