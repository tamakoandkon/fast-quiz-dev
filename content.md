# 页面组成以及需求

> 更新：2026-10-14；基线为合并 PR #14 后的 `main`（`3ebee71`）。
> 工程债务见 [重构行动清单](docs/refactor-harden-roadmap.md)，实际执行结果见 [测试记录](docs/verification-2026-10-14.md)。

状态约定：`[x]` 表示已找到相关实现，不代表每个页面都经过浏览器端到端验收；`[ ]` 必须结合“未实现 / 部分完成 / 可选”说明阅读。权限以服务端校验为准。原始需求保留在 Git 历史中。

## 用户模块

### 租户管理

- [x] 租户管理总览、创建、修改与详情页面及接口。
- [x] 我的租户快捷列表、切换租户页面与快捷入口；读取当前用户的租户。
- [x] 系统管理员读取所有租户，管理操作有权限校验。
- [ ] **部分完成：添加已有用户到租户 / 添加到其他租户。** 当前可在租户内创建新账号，内部已有成员映射方法，但没有完整的已有账号跨租户加入页面、接口和授权流程。

入口：[租户 API](FastQuiz/FastQuiz.Auth/Interface/TenantApi.cs)、[用户 API](FastQuiz/FastQuiz.Auth/Interface/AuthApi.cs)、[租户页面](FastQuiz.Frontend.Server/app/pages/tenant)。

待办验收：同一全局账号可加入第二个租户而不重复注册；拒绝无权操作、重复加入及跨租户越权，并有 API 回归。

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
- [ ] **部分完成：添加到其他租户。** 与租户管理中的同一待办合并跟踪。
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

### 尚未形成完整交付的流程

- [ ] **部分完成：批量预生成并分配 / 领取试卷。** 已有批量创建 API、未分配试卷及内部绑定方法；领取 API 仍被注释，前端没有完整领取/管理员分配流程。
- [ ] **未完成：按请求的 `Count` 生成指定份数。** 命令定义了数量，但当前 `QuizAttemptCreationService` 每个 command 仅创建一份，未使用 `Count`。
- [x] 批量请求单份试卷的正向 API 回归：已恢复执行，校验未分配 / Pending 试卷与关联题目落库，不再错误读取过滤未分配试卷的公开记录列表。
- [ ] **待验收：多份数量、批量参数、快照所属模板、权限、领取并发与失败回滚。** 单份正向测试通过不代表批量/分配流程完成。
- [ ] **可选：手动组卷。** [原始设计](FastQuiz/README.md) 定义为“未来可能支持”，不是当前主流程的必交付功能。

待办验收：请求 N 份产生 N 份完整试卷；拒绝非法数量及跨模板/租户输入；同一试卷并发领取只能成功一次；前端可完成领取/分配并进入答题。

## 推进与维护

1. 优先批量试卷闭环、可靠任务调度与数据库升级验收，再补已有账号跨租户、我的组织及组织内建用户。
2. 明确销户的数据保留规则后实施；手动组卷另行排期。
3. 后续功能 PR 同步此文、重构清单与实际测试记录；未实现或未执行的验收不得直接勾选。
