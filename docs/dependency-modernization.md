# 依赖现代化记录

核查与本地验证日期：2026-10-14。Git 分支：`chore/dependency-modernization`，基于已合并 Apple UI 的 `main`（`27a4226`）。

## 选择与版本

| 范围 | 原版本 | 本次版本 |
| --- | --- | --- |
| 11 个后端项目目标框架 | .NET 8 | .NET 10 |
| SDK / ASP.NET Core 运行时 | 未固定 SDK / 8.x | 10.0.401 / 10.0.12 |
| EF Core / SQLite / Design / EF 健康检查 | 8.x、各模块不同补丁 | 9.0.20 |
| MariaDB Pomelo / PostgreSQL Npgsql EF 驱动 | 8.0.3 / 8.0.11 | 9.0.0 / 9.0.4 |
| JWT / API 测试宿主 | 8.x | 10.0.12 |
| Swagger Swashbuckle | 6.6.2 | 10.3.0 |
| FluentValidation / Mapster | 12.1.1 / 10.0.11 | 12.2.0 / 10.0.13 |
| BCrypt / NLog Web / Hangfire | 4.1.0 / 6.2.0 / 1.8.24 | 4.2.1 / 6.2.1 / 1.8.25 |
| NUnit / Adapter / Test SDK | 混用 NUnit 3、4 等版本 | 5.0.0 / 6.3.1 / 18.11.0 |
| coverlet / Moq | 多种补丁 / 4.20.72 | 10.1.0 / 4.21.0 |
| Node.js / npm | 未固定项目运行环境 | 24.21.0 LTS / 12.2.0 |
| Nuxt / Nuxt UI | 4.3.1 / 4.6.1 | 4.6.1 / 4.11.3 |
| Vue / Router | 3.5.32 / 5.0.4 | 3.5.44 / 5.4.0 |
| Pinia / Nuxt 集成 | 3.0.4 / 0.11.3 | 4.0.3 / 1.0.2 |
| Axios / Day.js / PapaParse | 1.15.1 / 1.11.21 / 5.5.4 | 1.20.0 / 1.11.23 / 5.7.0 |
| Tailwind / vue-echarts / SDK 生成器 | 4.2.2 / 8.3.1 / 13.6.10 | 4.3.3 / 8.3.2 / 13.13.0 |
| TypeScript / vue-tsc / Node 类型 | 未显式固定 | 6.0.3 / 3.3.13 / 24.19.2 |
| k6 类型 | 2.2.1 | 2.3.0 |
| 容器 | .NET8 Alpine / 镜像站 Caddy2.9.1 | 官方 .NET10 Debian / Node24 / Caddy2.11.7 |

未发布更高正式版的 EasyCaching、NUlid、BenchmarkDotNet、ECharts、数据库健康检查及 Hangfire 存储等保持现有正式版本；不为了数字变化替换组件。npm 已经是现代包管理器，继续使用 npm，避免无必要的管理器迁移。TypeScript 选择兼容的 6.0 稳定线，没有直接跨到 7。

.NET 10 LTS 支持至 2028-11-14，而 .NET8 和 EF9 所属支持周期在 2026-11-10 结束。[微软支持政策](https://dotnet.microsoft.com/en-us/platform/support/policy)

**EF9 是用户确认保留 MariaDB 后选择的过渡方案。** Pomelo 最新正式版只支持 EF9，不应混用 EF10。需要在 2026-11-10 前重新评估：等待兼容的正式驱动，或另行决定数据库/提供程序方案。这次不能被当作“数据库依赖全部进入长期支持”。[Pomelo 兼容表](https://github.com/PomeloFoundation/Pomelo.EntityFrameworkCore.MySql#compatibility)

## 配套调整

- `FastQuiz/Directory.Packages.props` 集中管理 NuGet 版本，避免测试、业务模块与数据库驱动各自漂移。
- `FastQuiz/global.json` 固定 SDK；本地 `dotnet-ef` 工具固定 9.0.20，允许在新运行时上运行，版本与 EF 库匹配。Visual Studio 的旧 EF Tools 引用改为 Design。
- 移除未使用的 `Microsoft.AspNetCore.OpenApi` 引用，继续单一 Swashbuckle 文档生成链。
- NUnit5 异步异常断言增加 `await`，保留真实测试断言。
- 后端间接依赖 Newtonsoft.Json 从有高危公告的 11.0.1 固定到 13.0.4。
- Nuxt 兼容日期更新、Pinia 配置适配；新版 UI 使用语义颜色，弹窗模型、答案数组、DTO 空值等与新类型一致。
- 删除后端从未支持的 `CaseStudy` 题型选项，避免发送无效值；CSV 匹配策略改为真实 DTO 的 `matchStrategy`，校验枚举值。
- PapaParse 删除不存在的 `trimHeaders` 参数；SDK 查询参数与新版 Swagger 的类型、枚举、大小写保持一致。
- 新增前端 `typecheck`；SDK 从隔离 .NET10 API 重新生成，生成脚本可通过 `FASTQUIZ_OPENAPI_URL` 指定地址，不再全局关闭 TLS 校验。
- 锁文件重建到官方 npm 源。npm12 安装时禁止依赖安装脚本，单独执行经过检查的项目 `postinstall`。
- k6 脚本项目改为符合实际 `import` 的 ES 模块，增加只检查语法、不执行压测的 `check`。
- Docker 修复项目引用还原、非 root 目录权限；Compose 固定公开 3003 并配置同域 API 代理，生产配置需提供密钥与域名。
- 新增 GitHub Actions 构建、SQLite/API 回归、类型检查、模型快照检查、压测语法检查和依赖审计。
- 新增每周 Dependabot 更新配置；EF 和驱动成组更新，跨主版本必须先人工核对兼容性。

## 前端安全与开发工具限制

前端锁文件审计最初有 15 项（含 2 critical）；将 `simple-git` 固定到 4.0.2、`esbuild` 固定到 0.28.2 后，没有 critical 或 low，仍有 **11 项 high**。这些是两条上游根公告与其传递依赖链的汇总，并不是 11 条独立漏洞：

- `braces@3.0.3` 的嵌套模式栈耗尽：[上游公告](https://github.com/advisories/GHSA-vfj7-8cjw-p6xm)。
- `node-forge@1.4.0` 的签名验证问题：[上游公告](https://github.com/advisories/GHSA-86w9-cpqp-85rv)。

截至核查，两者都没有更高正式版修补。它们在 Nuxt/Nitro 构建或开发工具依赖链中；当前静态部署不安装 Node 依赖，也不运行 Nitro 服务。**不能据此视为开发/构建环境无风险**：开发服务只绑定可信本机，不处理不可信 glob 输入，不使用该开发依赖做安全敏感签名验证，后续持续跟进补丁。

稳定版 Nuxt DevTools 仍默认导入 `simple-git`，而修补版4移除了默认导出。本次暂时关闭 DevTools，避免退回有漏洞的3.x或引入 beta；不影响业务页面，Vue 调试工具与正常热更新仍可使用。待上游正式版兼容后再恢复。

CI 完整打印审计报告、对 critical 失败；未修补的 high 并未隐瞒或标记解决。不要使用 `npm audit fix --force`：当前审计建议会将 Nuxt 降到旧版，破坏本次升级。

## 本地验证

> 以下为依赖升级当时的阶段记录，不是当前累计状态。后续代码已合并、CI 已运行；当前测试数量、PostgreSQL 专项结果及仍未验证的范围见 [2026-10-14 测试记录](verification-2026-10-14.md)。当时新增的 Dependabot 配置后来已按项目决定禁用，不能据此判断当前仍开启。

- 升级前 .NET8 基线：Auth99、Question76、Quiz141、API164，共 **480通过、2原有跳过**。
- 升级后 .NET10：同样 **480通过、2原有跳过**，编译0错误；原有48个可空/未使用字段等警告尚在。
- 三个 SQLite 上下文 `has-pending-model-changes` 均无差异；未新增、删除或重写历史迁移。
- 使用旧 .NET8/EF8 程序在临时目录创建数据库，再由 .NET10/EF9 启动读取；四项健康检查均 Healthy。没有连接生产数据库。
- 新 .NET10/EF9 在全新临时库执行三个模块的完整历史迁移成功。
- NuGet 直接/间接审计：11个项目没有报告已知易受攻击包。
- Node24 下 `npm ci --ignore-scripts`、Nuxt prepare、完整 typecheck 与静态生产构建通过。
- 新 Swashbuckle 输出生成的 TypeScript SDK通过类型与构建验证。
- 内置浏览器验证隔离环境登录、用户列表、后台页面刷新和个人资料弹窗开关；控制台没有错误或警告。
- linux-x64 自包含发布构建通过；产物仅生成到本地临时目录，没有发布到服务器。
- k6 四个脚本语法检查通过，k6 类型依赖审计0。
- Docker Compose 配置校验通过；Docker daemon未启动，因此镜像实构建、容器启动、实际 MariaDB/PostgreSQL 回归 **未验证**，不能把“驱动编译成功”当成数据库验收通过。
- GitHub Actions和Dependabot配置仅在本地新增，尚未推送，所以不能声称远端 CI 已通过。

## 开发命令

后端：

```bash
cd FastQuiz
dotnet tool restore
dotnet restore FastQuiz.sln
dotnet build FastQuiz.sln -c Release --no-restore
dotnet test FastQuiz.sln -c Release --no-build --filter 'FullyQualifiedName!~MariaDb&FullyQualifiedName!~PostgreSql&FullyQualifiedName!~PostgreSQL'
dotnet ef migrations has-pending-model-changes --project FastQuiz.Auth --context SqliteAuthDbContext -c Release --no-build
dotnet list FastQuiz.sln package --vulnerable --include-transitive
```

前端参见模块README；部署参见 `Deploy/minimal.md`。生产发布必须另行批准，先备份和验证实际数据库，再切换新发布目录，保留旧版本回滚。
