# 生产配置与考试服务拆分

## 配置契约

- `Jwt:Key` 没有默认值：从环境变量 `Jwt__Key`、开发 user-secrets 或部署 secret store 注入至少 32 字节的随机密钥；空值、短值、GUID、replace 占位值会拒绝启动。Issuer/Audience 仍显式配置，签发和验证读取同一份强类型设置。
- 仓库历史中的签名密钥及管理员初始口令应视为已泄露。必须在发布窗口轮换，不能靠删除当前代码消除历史泄露。不要恢复使用旧密钥；旧令牌失效是预期行为。
- `BootstrapAdmin:Enabled` 默认 false。只有显式启用且管理员不存在时，才用外部 `BootstrapAdmin__Password` 创建账号 `defaultadmin`；密码须 12–20 字符（与当前登录校验上限一致），推荐密码管理器生成。缺失/非法密码在种子写入前失败。已有管理员不重置密码，也不需要再次提供密码。
- `Migrations:RunMigrationsOnStartup` 默认 false，包括配置缺失时；数据库更新须通过一次性 `Migrations__RunMigrationsOnly=true` 任务完成。该任务可同时执行显式管理员初始化，然后退出，不监听 API、不启动后台 worker。
- Hangfire 默认 `sqlite`，文件 `data/hangfire.db`。SQLite 的 `Hangfire:ConnectionString` 实际接受**文件路径**；MariaDB/PostgreSQL 接受数据库连接字符串，必须外部提供。仅 Development 允许显式 memory；生产启动会拒绝内存存储。
- Auth / Question / Quiz 数据库与 Hangfire 存储均需持久化及备份。答案直接事务落库，没有独立的内存缓冲表。持久化任务调度不等于调度与业务事务原子提交：提交后、入队前崩溃仍是待改进的 outbox 边界，不宣称 exactly-once。

## 开发启动

在 API 目录通过 `dotnet user-secrets set Jwt:Key <私有随机值>` 设置密钥。首次本地初始化同生产一样显式设置迁移/BootstrapAdmin 变量；不要把密码写到 appsettings 或 launchSettings。测试工厂仅向隔离测试环境注入临时随机凭据，显式开启迁移、初始化和内存任务存储。

## 考试服务职责

`QuizAttemptService` 保留为兼容 facade，原接口、端点与已持久化 Hangfire 任务的公开方法不变。具体逻辑拆分为：

- `QuizAttemptCreationService`：创建、批量创建、内部领取方法（领取 API 尚未开放，批量 `Count` 尚未生效，见功能清单）。
- `QuizAttemptQueryService`：考试/记录列表及详情。
- `QuestionRecordQueryService`：题目与答案可见性查询。
- `QuizAnswerService`：答题归属校验、请求内答案持久化。
- `QuizAttemptLifecycleService`：考试状态转换、延迟阅卷调度。
- `QuizAttemptReviewService`：人工判定与后台阅卷编排。

各服务使用独立接口和按需依赖。新后台任务依赖 `IQuizAttemptReviewService`，旧任务仍经 facade 转发；不改变 PR #13 的权限、状态守卫和事务边界。现有 API/安全回归测试继续覆盖兼容入口。

部署命令和首次初始化步骤见 [部署说明](../Deploy/minimal.md)。本次源码修改不代表生产实例已经升级。

## PR #14 源码阶段验证记录（2026-10-14，历史）

以下记录仅描述该阶段当时执行的验证，不是当前累计状态。合并后的再次核查、前端构建、既有 PostgreSQL CI 和批量测试恢复结果见 [最新测试记录](verification-2026-10-14.md)；当前待办以 [重构行动清单](refactor-harden-roadmap.md) 和 [页面需求](../content.md) 为准。

- Release 构建通过，0 错误；仍有 43 条原有警告，主要为未使用依赖及测试构造器空值告警。
- Release SQLite/API 回归：542 通过、1 条原有批量创建测试跳过；新增 23 条配置/初始化/后台任务兼容测试。
- 独立子进程执行迁移及管理员初始化，验证退出成功、密码仅哈希入库、不监听 API、不启动 Hangfire worker。
- SQLite Hangfire 延迟阅卷任务在重建服务提供器后仍可读取，并由真实 worker 执行。
- 拆分前后 18 个方法体对照一致（仅将新任务调度目标改为独立阅卷接口）；权限与事务逻辑保留。
- 压测 JavaScript 语法检查、Git diff 空白检查通过。
- 本次未重跑 PostgreSQL/MariaDB 专用套件，未构建前端或部署生产；数据库模型和前端未修改。
