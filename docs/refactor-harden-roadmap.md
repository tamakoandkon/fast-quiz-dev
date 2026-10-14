# FastQuiz 重构 / 稳健性 / 部署行动清单

> 更新：2026-10-14；基线 `main` / `3ebee71`（PR #14 已合并）。本版取代 2026-09 历史审计中的过时“现状”；原文保留于 Git 历史。
> 功能范围见 [页面需求](../content.md)，配置契约见 [生产加固说明](production-hardening.md)，实际运行结果见 [测试记录](verification-2026-10-14.md)。

`[x]` 仅用于明确完成的子项。实现、自动测试、生产运行验收是不同层次；未完成或未执行的验收保留 `[ ]`。本文不记录生产地址、连接串、凭据、私密文件位置或部署主机路径。

## 阶段一：安全配置与运维基础

### 1.1 外部密钥与显式管理员初始化：实现完成

- [x] 当前应用配置没有默认 JWT Key / 管理员口令，无效、短或已知占位密钥拒绝启动。
- [x] 签发和验证共用强类型设置；日志不打印密钥与数据库连接字符串。
- [x] bootstrap 默认关闭，显式外部密码才可创建管理员；不覆盖已有账号密码。
- [x] 配置、令牌、初始化及后台任务兼容性有回归。
- [ ] **持续发布检查：** 每个环境确认历史泄露凭据已轮换、旧令牌失效、日志无秘密。不能靠删除当前文件消除历史泄露，也不能用代码测试替代环境验收。

证据：[配置与初始化测试](../FastQuiz/FastQuiz.API.Test/Configuration/ProductionConfigurationTests.cs)、[配置契约](production-hardening.md)。

### 1.2 CORS 与 Host：实现完成，专项验收待补

- [x] 开发与生产策略分离，非开发环境要求 CORS 白名单及非通配 AllowedHosts。
- [ ] 验证真实代理/浏览器下的合法来源、非法来源、预检与凭据请求；本轮未新增该项浏览器测试。

证据：[Program.cs](../FastQuiz/FastQuiz.API/Program.cs)。

### 1.3 显式迁移与 provider 选择：部分完成

- [x] 普通启动迁移默认关闭；缺失配置也不迁移。
- [x] migration-only 模式执行迁移后退出，不监听 API、不启动 worker；迁移按配置选择 provider。
- [x] 三个 SQLite 模型与迁移一致；既有 PostgreSQL 专项 CI 验证五个事务/历史数据升级场景。
- [ ] **未完成：MariaDB 考试模块升级迁移及专用回归。** 已有 DbContext / repository 不等于升级路径完成，考试迁移目录缺少相应 MariaDB 系列。
- [ ] 补齐三种 provider 的空库、旧库升级及多实例部署验收；迁移由受控单次任务执行，未声称并发迁移通过。

证据：[迁移入口](../FastQuiz/FastQuiz.API/Setup/ApplyMigrationsExtensions.cs)、[考试迁移](../FastQuiz/FastQuiz.Quiz/Migrations)、[事务测试](../FastQuiz/FastQuiz.Quiz.Test/Infrastructure/QuizTransactionTests.cs)。

### 1.4 持久化后台调度：基础完成，可靠性待补

- [x] Hangfire 默认持久化 SQLite，非开发环境拒绝 memory；开发环境也须显式选 memory。
- [x] 已入队任务可在重建服务提供器后读取并由真实 worker 执行；兼容原持久化任务入口。
- [ ] **未完成：业务事务与任务入队的可靠衔接。** 生命周期服务在事务提交后调用 Schedule，此间崩溃/入队失败仍可能漏调度；需要事务内任务记录（outbox）及可重试投递/补偿。
- [ ] 多实例共享存储/worker 拓扑、重复投递与幂等故障测试；不承诺 exactly-once。

验收：故障注入覆盖业务提交后未入队、入队失败、worker 重启与重复投递，最终不丢作业、不重复产生有效状态更新。

证据：[Hangfire 配置](../FastQuiz/FastQuiz.API/Setup/AddHangfireServiceExtensions.cs)、[生命周期服务](../FastQuiz/FastQuiz.Quiz/UseCase/Services/QuizAttemptLifecycleService.cs)。

### 1.5 健康检查与日志：基础完成，专项验收待补

- [x] 已有 `/health` 与模块数据库健康检查，旧文“没有健康检查”不再成立。
- [x] 已有 NLog 及 ASP.NET Core 请求起止/耗时日志；缺少 `UseHttpLogging` 不等于没有请求日志。
- [ ] 故障库、5xx 请求追踪、相关 ID、敏感字段脱敏与保留规则尚需专项验收/加强；本轮未逐项注入故障。

证据：[健康检查](../FastQuiz/FastQuiz.API/Setup/AddHealthCheckService.cs)、[日志配置](../FastQuiz/FastQuiz.API/nlog.config)。

## 阶段二：正确性、归属与阅卷

### 2.1 阅卷 / 状态权限：漏洞已修复，声明式重构待做

- [x] 人工判题、完成阅卷校验操作人权限；租户边界及题目、答案、结果可见性有负向回归。
- [ ] 原清单提出的声明式 policy 尚未统一。当前服务端校验有效，不能将这一结构待办描述成权限漏洞仍在。

证据：[阅卷用例](../FastQuiz/FastQuiz.Quiz/UseCase/Services/QuizAttemptReviewService.cs)、[考试 API 测试](../FastQuiz/FastQuiz.API.Test/Quiz/QuizAttemptTests.cs)。

### 2.2 答题归属与角色分配：已修复

- [x] 答题核对考生、租户、状态与时间窗，提交后拒绝改答案；有越权/错租户回归。
- [x] 角色分配/撤销检查操作管理员而非被操作用户，变更后失效权限缓存。
- [x] 移除租户成员不删除共享账号；跨租户、全局管理员和共享账号保护有测试。

证据：[答题服务](../FastQuiz/FastQuiz.Quiz/UseCase/Services/QuizAnswerService.cs)、[角色测试](../FastQuiz/FastQuiz.API.Test/Auth/RoleServiceTests.cs)、[用户安全测试](../FastQuiz/FastQuiz.API.Test/Auth/UserManagementSecurityTests.cs)。

### 2.3 人工阅卷完成守卫：核心已修复，重复请求契约待定

- [x] 可空 `IsCorrect` 区分未判定、判错、判对；人工题未全判不得完成阅卷。
- [x] 客观评分、人工判定保留与汇总、历史待判记录修复及已完成成绩保留有回归。
- [ ] **契约差异：** 旧清单要求重复 Graded 返回 409，目前同状态请求幂等返回。需决定保留幂等还是改 409，并锁定接口测试；不等于人工守卫未实现。

证据：[领域阅卷](../FastQuiz/FastQuiz.Quiz/Domain/Services/QuizReviewService.cs)、[生命周期](../FastQuiz/FastQuiz.Quiz/UseCase/Services/QuizAttemptLifecycleService.cs)、[事务测试](../FastQuiz/FastQuiz.Quiz.Test/Infrastructure/QuizTransactionTests.cs)。

## 阶段三：结构重构与持久化

### 3.1 多 provider 真实绑定：已修复，去重与三库验收待补

- [x] Auth / Question / Quiz 按配置绑定同 provider 的上下文与仓储，不再绑死 SQLite。
- [ ] 抽共享注册 helper、减少重复 switch，修正 `QuizSetupExstensions` 等拼写。
- [ ] 补三种 provider 的 host/DI 解析矩阵测试；MariaDB 升级回归见 1.3。已有 PostgreSQL 五个专项测试不等于全部仓储测试通过。

证据：[Auth 注册](../FastQuiz/FastQuiz.Auth/Interface/Extensions/AuthExtensions.cs)、[Question 注册](../FastQuiz/FastQuiz.Question/Interface/Setup/QuestionExtensions.cs)、[Quiz 注册](../FastQuiz/FastQuiz.Quiz/Interface/Setup/QuizSetupExstensions.cs)。

### 3.2 考试服务拆分：主体完成，兼容层清理待做

- [x] 拆为创建、查询、题目查询、答题、生命周期及阅卷六个独立服务与接口。
- [x] 原 `QuizAttemptService` 为约 129 行 facade，兼容端点与旧任务；新任务调用独立阅卷接口。
- [ ] 清理未使用的 `QuizService` / `IQuizService` 骨架，制定 `IQuizAtteptService` 拼写兼容/淘汰方案；不得破坏已持久化任务引用。

证据：[服务目录](../FastQuiz/FastQuiz.Quiz/UseCase/Services)、[兼容测试](../FastQuiz/FastQuiz.API.Test/Configuration/ProductionConfigurationTests.cs)。

### 3.3 答案可靠保存与快照事务：原缺陷已修复

- [x] 不再使用易失内存答案缓冲；保留 `AnswerBufferService` 名称兼容，完整答案集在请求内事务替换。
- [x] 多选、清空、失败回滚、并发答案集不混合有单元/API/事务回归。
- [x] 答题与提交/阅卷通过数据库事务和锁协调；快照生成有统一事务边界。
- [ ] 数据库故障、备份恢复等运维演练持续执行；后台漏调度见 1.4，不能再称为“答案还在内存里”。

证据：[答案保存](../FastQuiz/FastQuiz.Quiz/UseCase/Services/AnswerBufferService.cs)、[快照](../FastQuiz/FastQuiz.Quiz/UseCase/Services/QuizSnapshotService.cs)、[事务实现](../FastQuiz/FastQuiz.Quiz/Infrastructure/Repositories/EfQuizUnitOfWork.cs)。

### 3.4 异常、JSON 与日志约定：未统一收尾

- [x] 已有全局异常处理和 ProblemDetails 基础。
- [ ] 统一权限/业务/未找到异常与状态码，补契约测试。
- [ ] 确认 Minimal API 与 MVC 的枚举需求后收敛双重配置，不能直接删除就当完成。
- [ ] 清理生产路径裸 Console、旧命名及构建警告；完整 Release 构建仍有 43 条已有警告。

## 测试与功能剩余项

- [x] 本轮执行 SQLite/API 回归、SQLite 模型检查、前端类型检查/构建、压测脚本语法检查，结果见独立记录。
- [x] 恢复原批量单份正向测试，修正其错误查询未分配试卷的断言来源，并校验试卷状态与关联题目实际落库；最终 SQLite/API 回归 543 通过、0 失败、0 跳过。
- [ ] 批量 `Count`、领取/分配流程、参数、跨模板/租户、并发与回滚测试待补。
- [ ] 已有用户跨租户、我的组织、组织内建用户、自助销户及可选手动组卷，见 [功能清单](../content.md)。

## 推进与维护规则

1. 优先可靠任务调度、MariaDB 升级与批量试卷闭环，再补成员管理新功能。
2. 测试只连接隔离数据库；过滤条件明确排除未配置的外部数据库，严禁使用共享/生产库。
3. 功能 PR 同步两份清单和验证记录，勾选附实现与测试证据，没运行的验收注明限制。
4. PR、文档、日志、截图均不公开环境标识、真实连接参数、秘密及私密路径；部署和凭据管理需独立授权。
