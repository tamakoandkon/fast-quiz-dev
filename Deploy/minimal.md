# FastQuiz 部署（.NET 10）

本次升级修改源码与构建配置，**不代表现有生产实例已升级**。端口仍为 3003；现有服务器继续沿用原部署，需另行确认发布。

## 发布前

1. 查看 [升级说明](../docs/dependency-modernization.md) 的兼容性与未完成验证。
2. 验证 SQLite、PostgreSQL 或 MariaDB 中实际使用的那种数据库；测试只能使用隔离库。仓库的数据库测试会删除/重建测试库，不能指向生产。
3. 备份数据库、运行配置、JWT 密钥与当前发布目录。SQLite 在线备份必须使用 SQLite backup API，或停服务后完整备份数据库和 WAL/SHM 文件；不要只复制正在写入的主库文件。
4. 保持数据库路径和端口不变。仓库历史中的 JWT 密钥及默认口令已公开：发布时必须换成私有随机密钥，并通过现有账号管理流程更换旧管理员密码。轮换密钥会使旧令牌失效，需重新登录；初始化不会重置已有账号。
5. 业务迁移沿用已合并的 PR #13，升级前显式执行迁移；不要重建数据库或删除迁移。普通启动不再自动迁移。

## 配置与初始化

详见 [生产配置及服务拆分说明](../docs/production-hardening.md)。JWT 密钥可以用 `openssl rand -base64 48` 生成，保存到私有环境配置/secret store，不要提交。Hangfire 默认持久化到 `data/hangfire.db`（配置项是文件路径，不是连接字符串），生产环境不允许 `memory`。保留整个 data 目录的写权限和持久卷。

非容器环境创建可写的 `data` 目录，通过 `Jwt__Key` 注入密钥；首次启动前设置 `Migrations__RunMigrationsOnly=true` 执行一次程序。需要首次管理员时再同时设置 `BootstrapAdmin__Enabled=true` 和 `BootstrapAdmin__Password`。完成后移除这三个初始化变量，再以常规服务启动。初始化失败必须先排查，不能忽略退出码启动服务。

文件日志目录使用 `FASTQUIZ_LOG_DIR`。采用 systemd 只读发布目录时，将其设为 `/var/log/fast-quiz`，由部署工具预先创建并授权服务用户写入，同时在服务的 `ReadWritePaths` 放行该日志目录。发布后必须实际请求接口，确认该目录中的 `all.log` / `system.log` 文件有新增内容；不能只凭目录权限或 journal 正常就认定文件日志落盘成功。未设置变量的本地开发仍写入程序目录下的 `logs`。

## 非容器发布

在开发机或构建机的仓库内执行：

```bash
cd FastQuiz
dotnet restore FastQuiz.sln
dotnet build FastQuiz.sln -c Release --no-restore
dotnet test FastQuiz.sln -c Release --no-build --filter 'FullyQualifiedName!~MariaDb&FullyQualifiedName!~PostgreSql&FullyQualifiedName!~PostgreSQL'
dotnet publish FastQuiz.API/FastQuiz.API.csproj -c Release -r linux-x64 --self-contained true -o ../artifacts/api
```

自包含发布不需要服务器额外安装 .NET 运行时；将 API 产物写入**新的**发布目录，不覆盖正在运行的版本。若选择 framework-dependent 发布，服务器需安装 ASP.NET Core 10 运行时（`aspnetcore-runtime-10.0`），.NET 8 运行时不能运行本次产物。

前端构建：

```bash
nvm use
npm install --global npm@12.2.0
cd FastQuiz.Frontend.Server
npm ci --ignore-scripts
npm run postinstall
npm run typecheck
npm run build
```

将 `.output/public/` 发布到新的 web 目录。保留 `config.json` 的 `apiBaseUrl: "/api"`；Nginx 在 3003 服务 SPA，`/api/` 移除前缀代理到私有 API 端口。前端 `/tenant/` 不应被整段代理到后端。详见 [前端 README](../FastQuiz.Frontend.Server/README.md)。

Nginx 必须显式启用预压缩资源协商，不能只生成 `.gz` 文件。参见 [前端加载优化及验证](../docs/frontend-performance.md)
和本应用 `server` 块使用的 [压缩片段](nginx-static-performance.conf)。

## Docker Compose（新环境）

仓库的镜像构建使用微软、Node 和 Caddy 官方源；API 非 root 运行，数据与日志目录可写。仅前端公开 3003，后端通过 Compose 内部网络访问。

```bash
cp Deploy/.env.example Deploy/.env
# 编辑 Deploy/.env：设置真实的随机 JWT 密钥、主机名/IP 和完整公开 URL。
docker compose -f Deploy/docker-compose.yml config --quiet
docker compose -f Deploy/docker-compose.yml build
# 显式迁移（无 API 监听，也不启动 Hangfire worker）
docker compose -f Deploy/docker-compose.yml run --rm -e Migrations__RunMigrationsOnly=true fqz-core

# 仅首次创建管理员时执行：交互输入 12–20 字符密码，不写入仓库或命令历史。
read -r -s BootstrapAdmin__Password # Bash；Zsh 请用 read -r -s "BootstrapAdmin__Password?管理员密码: "
export BootstrapAdmin__Password
docker compose -f Deploy/docker-compose.yml run --rm \
  -e Migrations__RunMigrationsOnly=true -e BootstrapAdmin__Enabled=true \
  -e BootstrapAdmin__Password fqz-core
unset BootstrapAdmin__Password

docker compose -f Deploy/docker-compose.yml up -d
```

不要在已有 Nginx 占用 3003 的生产机直接运行该命令。现有 `/var/lib/fast-quiz` 数据不会自动进入 Docker 命名卷；如需改为容器部署，应先单独制定数据迁移、权限与回滚方案。不要执行 `down -v` 删除数据卷。

## 发布验收与回滚

- API `/health` 和公开 `/api/health` 均应健康。
- 登录、获取当前用户、题库、考试记录及阅卷流程正常。
- 后台页面直达与刷新返回 SPA，而不是 API 404。
- 观察错误日志、后台答题持久化及数据库健康状态。
- 失败时恢复原服务执行路径与 web 发布路径，使用原运行配置。若后续有数据库迁移或产生新数据，须评估数据库回退，不能盲目用旧备份覆盖新数据。

当前服务器采用的自包含发布可以保留旧版本用于回滚；本次工作没有重启生产服务或修改生产数据库。
