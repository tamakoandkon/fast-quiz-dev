# FastQuiz 重构 / 稳健性 / 部署 行动清单

> 只读审计结论（2026-09，dev 分支）：建议先停新功能，按此清单推进。
> 每个工作项都写清：改动点(文件:行)、怎么改、怎么验收。完成请勾选 `[x]`。
> 架构分层、测试底座、模块划分均良好——主要债务集中在 **配置/启动、鉴权/正确性、provider 绑死 sqlite、内存态组件** 四类。

准则：
- 每个独立改动前先跑对应测试套件，改动后全量 `dotnet build FastQuiz.sln` + `dotnet test`。
- 涉及安全项（JWT、默认口令）改动后务必重启并重新登录取新 token 验证。
- GUID id / 纯字符串日期等约定照旧。

---

## 阶段一：上线止血（安全 / 部署）—— 改动小、风险低、优先做

### 1.1 把 JWT 密钥 / 连接串 / 内置弱口令从仓库挪到环境变量（不打印）
现状：
- 硬编码密钥与连接串：`FastQuiz.API/appsettings.json:8-21`（`Auth`/`Question`/`Quiz` 三段 sqlite 连接串 + `Jwt:Key/Issuer/Audience` 同值 GUID）。
- 启动日志把 JWT **Key** 与连接串打到 stdout：`FastQuiz/API/Setup/AuthSetupExtensions.cs:24-40`。

做法：
1. `appsettings.json` 里删掉 `Jwt` 段明文 `Key`（可保留结构或移到 `appsettings.Development.json`）。
2. 密钥放入 **user-secrets（dev）** 与 **环境变量 / 容器 Secret（prod）**，通过 `builder.Configuration` 的 `AddEnvironmentVariables()` / 占位符 `Jwt__Key` 注入。
3. 删除 `AuthSetupExtensions.cs:24-40` 的两处 `Console.WriteLine`（连 DB 连接串也不要打，连接串可含口令）。
4. `defaultadmin`（`Auth/Domain/Entities/User.cs:8-9`，口令 `fastquiz`）：
   - 生产源不要提交明文。改为初始化时若口令仍是默认值则强制下一步改密，或把种子密码放入 env；
   - 至少：上线后**立即登录并改掉**该管理员口令。

验收：
- `git grep -n "08D734F1\|fastquiz\|DefaultAdminPassword"` 不再出现明文密钥/口令于入库配置（Domain 常量里保留的仅作兜底判断，需在种子会话判定）。
- 重启后 stdout 不再打印 Key / 连接串。
- 用新 env 签名的 token 能正常鉴权（重新登录）。
- 生产 kill 后重连同一 DB，`defaultadmin` 仍存在且密码已是新值。

### 1.2 收紧 CORS
现状：`Program.cs:39-44` `AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod()` 默认策略。
- 前端 SPA 独立部署（Nuxt 4 SSG，见 `D:\KingProjects\FastQuiz\FastQuiz.Frontend.Server\nuxt.config.ts:3-7`，`ssr:false` + `nitro static`）。

做法：按目标源列表显式授权（读 env `Cors__AllowedOrigins`），非开发环境不要再包 AllowAnyOrigin。
验收：跨域只允许白名单源；未授权源请求被拒（浏览器 console 可见 CORS 报错）。

### 1.3 关闭"启动即自动迁移 + 只跑 sqlite"
现状：`Program.cs:59 ApplyAllMigrations()` → 三个 applier 全部 `GetRequiredService<Sqlite*DbContext>()` 后 `Migrate()`：
- `AuthExtensions.cs:175-180`、`QuizSetupExstensions.cs:150-156`、`QuestionExtensions.cs:135-140`。

做法（二选一）：
1. **推荐**：从 `Program.cs` 移除启动自动迁移，改成一个独立入口/脚本（或 CI 阶段执行 `dotnet ef database update`），让它指向**所选 provider** 的 DbContext。
2. 若坚持启动迁：把 applier 改为按 `settings.Type` 解析对应 provider 的 DbContext（见阶段三 DI 重构），并只在单实例/部署编排里先跑一次，别多实例并发 Migrate。

验收：
- 配 postgres/maria 时能对真实目标库建表（此前只能建 sqlite）。
- 多实例启动不再争锁迁移；重启无 pending migration 抛错。

### 1.4 Hangfire 换持久化存储（当前业务调度会丢）
现状：`QuizSetupExstensions.cs:129-138` `UseInMemoryStorage()` + `AddHangfireServer()`（进程内 worker）。阅卷后台调度见 `QuizAttemptService.cs:498-520`。`:158-160` 残留一次启动入队 `Console.WriteLine("Hello world from Hangfire!")`。

做法：
1. 删除 `QuizSetupExstensions.cs:158-160` 的样板 job。
2. 若继续用进程内 worker：把存储接到一个持久化目标（配成与业务同库或独立 Hangfire 库，`UseSqlServerStorage`/`UseNpgsqlStorage`/`UseStorage`）。多实例下选**共享存储 + 中心 worker** 或专用 worker 节点，不要每实例都 run 进程内重活。
3. 若暂不想引入外部 broker：至少在 README 注明"Hangfire 为进程内内存存储，阅卷作业在单实例重启窗口可能丢失"，别再当生产默认。

验收：
- 提交一份含 Manual/自动题目的答题后，**重启 API 再等调度窗口**，阅卷作业仍能执行（内存存储则丢）。
- 多实例下只调度一次、不重复阅卷。

### 1.5 加 health 检查 + 请求日志（运维可观测）
现状：全仓无 `AddHealthChecks/MapHealthChecks`，无 `UseHttpLogging`/请求日志中间件，ILogger 已在 service 用但无请求级追踪。
做法：
1. `AddHealthChecks()`（含各 DbContext 的 `AddDbContextCheck`），`MapHealthChecks("/healthz")`。
2. 加 `UseHttpLogging()`（或用精简请求中间件），输出 method/path/status/耗时。避免把 body/敏感头打出来。
验收：`curl /healthz` 返回 Healthy；日志能看到 5xx 的请求路径与状态。

---

## 阶段二：正确性漏洞（鉴权 / 归属 / 阅卷守卫）—— 先补测试再改

### 2.1 阅卷 / 状态端点加评审角色鉴权
现状：整组 PUT `QuizAttemptApi.cs:18-27` 仅组级 `RequireAuthorization()`；`ManualReviewQuestionRecordAsync`（`QuizAttemptService.cs:534-548`）与该项 PUT 均无"评审人/阅卷权限"校验 —— 任意登录用户可翻 isCorrect、置任意状态(含 Graded)。

做法：
- 在**组或端点**加显式权限/角色策略（对齐你现有 `HasPermissionAsync` 的权限集，但改成声明式 policy 而非散在 service 里）。
- `ManualReviewQuestionRecordAsync`、`QuizAttemptApi.cs` PUT status 两处都验权限后才动作。

验收：无评审/阅卷权限的普通用户调用返回 403（先写个负向测试）。

### 2.2 答题端点校验 attempt 归属
现状：`AnswerQuestionAsync`（`QuizAttemptService.cs:353-413`）只查状态与时间窗，未校验 `questionRecord` / attempt 属于**当前登录 userId**；`UpdateQuizAttemptStatusAsync`（`:476`）已查 `AssignId`。

做法：进入 `AnswerQuestionAsync` 时从 attempt/record 校验 `AssignId == callerId`（拿 HttpContext/Claims 里的 userId），不匹配抛权限/404。

验收：用 B 用户的 token 写 A 用户 in-progress 的 attempt 被拒（先写负向测试）。

### 2.3 Graded 前校验 Manual 题已全部人工复核（并拦重复 Graded）
现状：PUT→Graded 触发 `CompleteReviewAsync`（`QuizReviewService.cs:68-92`）只按当前 `IsCorrect` 求和；未复核的 Manual 记 0。现成 helper `HasManualQuestionAsync`（`:94-98`）**未被调用**。对已 Graded 再 PUT 是静默 no-op（`:74-77`），非明确错误。

做法（符合你 MEMORY 里"Submitted==待人工阅卷"的模型）：
- 在 `→Graded` 前调用 `HasManualQuestionAsync`，若仍有 Manual 题的 `IsCorrect` 未复核/未表态 → 拒绝并返回明确提示（不能把"待人工"当 0 分落库）。
- 对已 `Graded` 再置 Graded：返回 409 而非静默吞。

验收：含 Manual 未复核的卷子无法 PUT→Graded；已判卷再判返回 409。用测试把判卷/取消判卷/裁判一致性锁定。

---

## 阶段三：结构性重构（整形）——改动较大，建议在二阶段测试齐全后做

### 3.1 把"多 provider DI 绑死 sqlite"改成按配置真实绑定
现状（跨 3 模块，同类/同病）：
- `QuizSetupExstensions.cs:108-123`、`AuthExtensions.cs:121-133`、`QuestionExtensions.cs:114-118`：`AddDbContext` 按 `settings.Type` 切换了，但 `AddScoped<I…Repo, Sqlite…Repo>` **无条件硬编码** → 配 postgres/maria 时进程注册了对应 DbContext 却永远被 sqlite repo 引用，实际只有 sqlite 端到端能用；三套 provider 的 repo + 测试是"运行时永不绑定"的死分支。

做法：
- 在每个模块的 `switch (settings.Type)` 里，同一个 case 同时完成：注册该 provider 的 DbContext **并** 把该组 repository/serivce 绑到同 provider 的实现。把 repo/context 绑定从 switch 外的静态链移进对应 case。
- 抽一个共享 helper 收敛三段几乎重复的 switch（AuthExtensions.cs 现已 ~x4 拷贝模板）。
- 顺带修正模块/API setup 分层与文件名拼写（`QuizSetupExstensions`→`QuizSetupExtensions`）。

验收：
- **写一个走 DI 的集成测试**：分别配 sqlite/postgres/maria 各起一个 host，`GetRequiredService<IQuizTemplateRepository>` 实为对应 provider 的实现（现在这个测试会失败/暴露死绑定）。这是"重启前锁死"的关键测试。
- `dotnet test` 全绿；本地仍默认 sqlite 端到端可用。

### 3.2 `QuizAttemptService`(730 行) 拆分 + 依赖方向理顺
现状：服务混答题生命周期 + 答缓存 + 阅卷转移；阅卷已被抽到 `Domain/Services/QuizReviewService`(197 行)却由 UseCase `QuizAttemptService` 经调度反向调用，依赖方向缠结。

建议（如果这段是你的重构重点，另开会话专门排）：
- 先把可测的行为用已有 `*TestsBase` 底座补上测试，再按聚合根拆（Attempt 生命周期 / Review 编排各自一个 service，别互相持有对方大文件）。
- 参考 `dotnet-test-coverage-audit` skill 找出当前活的未测方法。

### 3.3 答题缓存从"单实例内存 + 崩溃丢窗"解耦
现状：`AnswerBufferService.cs`（`ConcurrentDictionary` + ~1s `Task.Run` 冲刷，`:184-191` 自带 `⚠️ 数据丢失风险`）；失败会把记录移出缓冲后直接丢，仅单实例可靠。

建议：为持久化缓冲设回退通道：写失败进 dedicated outbox/表或死信队列，提交时同步/半同步落盘关键答案、并做重试；或把高频提交合并批量写而保留单条确认。此点想清楚"到底允许多久丢窗口"再动。

### 3.4 异常语法收敛
- 异常基类与 ProblemDetails 已不错（`GlobalExceptionHandler.cs` 400/404/403/422）。
- 统一 `PermissionException` vs `BusinessException`(中文) vs `ResourceNotFoundException` 的使用口径；删 `Program.cs:17-25` 里重复注册的枚举转换器（STJ 与 MVC 二选一即可）；清理生产路径的裸 `Console.WriteLine`。

---

## 建议分工给"你自己做"时的顺序
1. `1.1 → 1.2 → 1.3 → 1.4 → 1.5`（阶段一，小步提交，一条一个 commit，现象都能立刻自验）
2. `2.1/2.2/2.3`（每条**先写失败测试 → 改实现 → 转绿**，参考 `test-driven-development` skill）
3. `3.1`（先写"DI 按 provider 真实绑定"的失败测试作为锚，再动手）→ `3.2/3.3/3.4`

> 若中途想让我直接执行某条，直接说条目号即可（如 "帮我做 1.1"）。安全/DB 相关改动我会先跟你确认不会碰你没备份的数据。
