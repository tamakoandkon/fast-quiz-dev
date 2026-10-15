# 2026-10-15 安全复核问题修复

基于 `main` 的 `d86c65eff4da`，对应同日安全复核报告的 #1–#8，以及报告保留的当前业务代码问题。本次提交面向 `main`；其他历史分支需合并本次修复后再发布。

## 修复对应关系

| 问题 | 实现与回归 |
| --- | --- |
| #1 租户更新 | 校验租户管理权限；普通管理者还须属于该租户，系统管理员保留跨租户管理能力；校验名称、描述长度；拒绝请求不会写入。 |
| #2 已发布考试列表 | 校验租户成员身份；普通考生仍能查看本租户的已发布考试。 |
| #3 考试及成绩列表 | 查询记录前验证模板属于请求租户；补齐 main 尚未合入的两条 records 修复；按用户查询在三个数据库仓储中先限制模板 OwnerId，再分页，返回层再次过滤。 |
| #4 快照列表 | 列表查询前校验模板所属租户，保留快照管理权限要求。 |
| #5 角色授予 | 服务与仓储均验证目标成员及角色启用状态；授予和成员移除在可串行化事务内锁定同一成员行；重复授予幂等。历史孤立映射不会参与授权；移除成员会删除其角色映射。 |
| #6 组织树 | 读取子组织同时检查父节点归属并按租户过滤；创建子节点也拒绝外租户父节点。 |
| #7 个人改密 | `/auth/me` 显式要求认证，只更新资料；新 `PUT /auth/me/password` 要求原密码，使用旧哈希条件更新阻止并发覆盖，返回替换令牌。真实 JWT 验证会检查当前凭据版本。 |
| #8 题目 XSS | 题干、选项九处渲染与通用内容查看器统一经过 Markdown 解析、DOMPurify 白名单清洗，覆盖旧 HTML 和快照；构建生成 CSP，禁止任意内联脚本和 eval，保留 Nuxt 启动脚本的精确哈希。 |
| 填空答案泄露 | 作答 DTO 不再返回填空参考答案；选择题选项正常返回，批阅及已评分结果沿用其权限控制。 |
| 角色禁用、编辑与撤权 | 权限查询排除禁用角色和非成员映射；PUT 角色原子更新权限列表；授权实时读取数据库，避免多实例的长时间权限缓存残留。 |
| Regex 耗尽资源 | 新规则拒绝非法正则；历史规则匹配设 100ms 超时，超时或非法规则返回可处理的业务异常。 |
| 条件性 Hangfire 后台暴露 | 移除公开 Dashboard 注册，后台任务工作进程继续运行；匿名 `/hangfire` 返回 404。 |

## 验证结果

- macOS / .NET SDK 10.0.401：586 项通过（Auth 103、Question 76、Quiz 160、API 247），无跳过；排除 MariaDB/PostgreSQL provider 专项。
- 隔离 Linux x64 / SDK 10.0.302：同一业务与测试源码的 586 项通过；仅隔离副本的 SDK 选择文件调整为服务器已有版本。
- 独立 PostgreSQL：新增认证安全仓储 3 项、既有考试事务 6 项全部通过，使用测试专属随机数据库并清理，不连接生产库。
- 前端 Node 26.11.1 / npm 12.2.0：19 项测试、typecheck、静态构建及加载预算通过。
- 独立 Chrome：未清洗对照事件可执行，经过实际共享清洗函数后事件不执行；启用生成的 CSP 后应用进入登录页、无运行时异常，额外未授权内联脚本被拦截。
- 认证、题库、考试 SQLite 模型无待生成迁移。本次无需新增数据库迁移。
- 服务器原服务运行状态及健康检查正常；没有切换生产发布目录。

运行回归：

```sh
cd FastQuiz
dotnet test FastQuiz.sln --filter 'TestCategory!=MariaDb&TestCategory!=PostgreSql'
# 指向独立 PostgreSQL 实例，测试会创建并删除随机测试库：
FASTQUIZ_TEST_POSTGRES='Host=127.0.0.1;Username=...;Password=...' \
  dotnet test FastQuiz.Auth.Test --filter 'FullyQualifiedName~SecurityRepositoryTests&TestCategory=PostgreSql'
cd ../FastQuiz.Frontend.Server
npm ci --ignore-scripts
npm run postinstall
npm test
npm run typecheck
npm run build
npm run check:loading
```

## 升级与运行注意事项

1. 新服务会拒绝没有凭据版本声明的旧令牌，升级时所有用户需要重新登录；改密和管理员重设密码也会使原令牌失效。前后端应一起升级。个人资料接口携带非空 password 现在返回 422；改密使用新接口 `{ currentPassword, newPassword }`，成功后保存响应 `accessToken`。
2. 静态 SPA 继续通过 JS 可读令牌构造 Bearer 请求。改成 HttpOnly 会话需要 BFF/服务端会话及 CSRF 配套设计；当前修复通过清洗全部展示入口与 CSP 缩小 XSS 风险，没有声称令牌架构已迁移。
3. Docker 镜像自动导入 `.output/security/caddy-csp.conf`。自有 Nginx 部署须把同次构建的 `.output/security/nginx-csp.conf` 放到受控位置并在应用的 `server` 中 include，再执行 `nginx -t`。哈希配置必须和静态产物一起更新；CSP 的 `connect-src` 为兼容运行时独立 API 配置保留 http/https，部署时可限制为实际 API origin。
4. 实时授权增加数据库读取，换取各实例在下一请求中看到禁用、权限编辑与成员移除；高并发生产环境需观测数据库负载。成员授权竞争时数据库可拒绝冲突事务，请重新读取当前成员状态后重试。
5. MariaDB 实现已同步修改并编译，本次未运行真实 MariaDB。未对历史 14 个 tip 分别改写提交；旧分支中的默认密钥/初始化账号与旧考试缺陷不能由 main 的修复自动消除。历史部署若曾使用默认凭据，应在实际环境轮换密钥和密码。
6. `npm audit` 仍报告 11 个既有 high 依赖告警（Nuxt 构建链等），本次没有将其标为解决，CI 继续输出审计结果并对 critical 失败。上游情况见 [依赖升级说明](dependency-modernization.md)。本次未证明服务器 RCE。
