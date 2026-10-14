# 清单同步与回归验证（2026-10-14）

## 范围与基线

- 基于合并 PR #14 后的 `main` / `3ebee714347baecf15aa3b7ae6334dd3590e30a7` 创建独立分支。
- 本次仅更新需求/重构文档与一条已有 API 回归测试，不修改业务实现、配置或数据库模型，不部署、不迁移生产库。
- 本机工具：.NET SDK 10.0.401、Node 26.11.1、npm 12.2.0；使用现有已安装依赖，没有重新安装完整依赖。
- SQLite/API 测试使用隔离测试库；明确排除需要外部 PostgreSQL/MariaDB 的套件，未向任何共享数据库运行测试。

## 本轮执行结果

| 检查 | 结果 | 边界 |
| --- | --- | --- |
| 整个 solution 的 Release 构建 | 通过，0 错误 | 完整编译有 43 条已有警告；后续增量编译只重报 1 条，不表示其他警告已修复 |
| Auth 测试 | 100 通过 | SQLite / 非外部库范围 |
| Question 测试 | 76 通过 | 同上 |
| Quiz 测试 | 157 通过 | 同上，含 SQLite 事务回归 |
| API 测试 | 210 通过 | 含恢复后的批量单份正向回归 |
| 后端合计 | **543 通过、0 失败、0 跳过** | 不是不加过滤条件的三数据库全套测试 |
| Auth / Question / Quiz 的 SQLite 模型与迁移检查 | 三项均无待生成模型变更 | 不执行数据库迁移，不代表 MariaDB 迁移完整 |
| 前端 `npm run typecheck` | 通过 | 静态类型检查，不是浏览器 E2E |
| 前端 `npm run build` | 通过 | Nuxt 静态生产构建 |
| LoadTest `npm run check` | 四个脚本语法通过 | 没有执行 k6 性能/容量测试 |
| 文档相对链接、Git diff 空白与新增内容隐私检查 | 通过 | 未发布本机原始测试日志或含机器路径的 TRX 文件 |

## 批量测试的发现与修正

1. 基线测试先得到 542 通过、1 跳过。跳过项是 `PostBatchQuizAttemptAsync_WhenValidRequest_ShouldReturnNoContent`，理由仍为“方法没有实现”。
2. 临时恢复执行后，它在“预期列表 1 条，实际 0 条”失败。原因不是没有写库：`QuizAttemptQueryService.GetAllQuizAttemptAsync` 会过滤 `AssignId == null` 的未分配试卷，旧测试却用这个列表验证预生成结果。
3. 保留真实 POST API 调用和 204 断言，改为通过测试 DI 查询隔离库，验证一份试卷、正确快照、未分配、Pending 状态，以及关联题目记录；移除过时 Ignore，重命名为 `PostBatchQuizAttemptAsync_WhenOneCopyIsRequested_ShouldCreateUnassignedAttempt`。
4. 修改后重新构建并执行同范围完整回归，得到上表最终结果。没有改变列表过滤行为或业务逻辑。
5. 代码核查另发现 `CreateQuizAttemptCommand.Count` 未被批量生成方法使用；领取 API 仍被注释。这两项**没有在本次修复**，仍列为 [功能待办](../content.md)，不能用单份测试通过证明多份数量/领取流程完成。

## 既有远端 CI（不是本轮本机执行）

核对 [PR #14 提交的 CI 记录](https://github.com/tamakoandkon/fast-quiz-dev/actions/runs/38538388857)：提交 `9ba005dc06e81d6ff7797bbb865d0f22d0d78088` 的 `backend`、`postgres-regression`、`frontend`、`loadtest-syntax` 均成功。

其中 PostgreSQL job 在隔离服务容器中执行 `QuizTransactionTests` 的五个专项场景。这能证明这些专项在该提交通过，不能代表当前本机重新执行了 PostgreSQL、全套 PostgreSQL 仓储测试或 MariaDB 回归。本机 Docker daemon 未运行，本轮未启动容器或测试外部数据库。本次新 PR 的 CI 结果以其检查页为准，不预先标记成功。

## 重现命令

在 `FastQuiz` 目录执行（本机 SDK 不在 PATH 时，先自行配置本机工具路径，不把私密路径写入文档）：

```sh
dotnet build FastQuiz.sln --no-restore --configuration Release --disable-build-servers -m:1
dotnet test FastQuiz.sln --no-build --no-restore --configuration Release --filter 'FullyQualifiedName!~MariaDb&FullyQualifiedName!~PostgreSql&FullyQualifiedName!~PostgreSQL&TestCategory!=PostgreSql'
dotnet ef migrations has-pending-model-changes --project FastQuiz.Auth --context SqliteAuthDbContext --configuration Release --no-build
dotnet ef migrations has-pending-model-changes --project FastQuiz.Question --context SqliteQuestionDbContext --configuration Release --no-build
dotnet ef migrations has-pending-model-changes --project FastQuiz.Quiz --context SqliteQuizDbContext --configuration Release --no-build
```

在 `FastQuiz.Frontend.Server` 执行 `npm run typecheck` 和 `npm run build`；在 `LoadTest` 执行 `npm run check`；仓库根目录执行 `git diff --check`。全新环境需先安装依赖及恢复 .NET 工具；检查外部数据库前必须准备隔离实例并阅读测试的建库/删库行为。

## 本轮没有完成的验收

- MariaDB 考试模块升级迁移及专用回归、三 provider 的 host/DI 矩阵。
- outbox / 任务补偿、多实例与崩溃窗口故障注入。
- 批量多份、输入边界、领取/分配并发及失败回滚。
- CORS/代理/健康故障专项浏览器验收、完整页面 E2E、真实压力测试及备份恢复演练。

这些边界已同步到 [重构清单](refactor-harden-roadmap.md)；没有将未实现的功能或未执行的验收勾成完成。
