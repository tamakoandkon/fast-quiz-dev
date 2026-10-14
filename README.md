# FastQuiz

多租户题库、考试与阅卷系统。后端 ASP.NET Core 10，前端 Nuxt 4 / Vue 3；保留 SQLite、PostgreSQL 和 MariaDB 支持。

## 开发环境

- .NET SDK 10.0.401（`FastQuiz/global.json`，允许同一 feature band 的新补丁）
- Node.js 24.21.0 LTS（根目录 `.nvmrc`）
- npm 12.2.0（前端和压测项目的 `packageManager`）
- EF Core 9.0.20 + Pomelo 9.0.0：MariaDB 稳定驱动暂不支持 EF Core 10，因此这是过渡组合，不是完整的 EF10 长期支持升级。

后端构建、测试与数据库工具从 `FastQuiz/` 执行；前端从 `FastQuiz.Frontend.Server/` 执行。请勿在根目录执行 `dotnet build` 绕过 SDK 固定配置。

## 文档

- [本次依赖升级与验证记录](docs/dependency-modernization.md)
- [前端开发与同域代理配置](FastQuiz.Frontend.Server/README.md)
- [部署与回滚注意事项](Deploy/minimal.md)
- [生产密钥、首次初始化与考试服务拆分](docs/production-hardening.md)
- [开发预期](content.md)
- [后端结构](FastQuiz/README.md)

## 特点

1. 内存缓存优化
2. 多种数据库支持
3. 业务逻辑模型
4. 升级迭代路径
5. 前端提交答题优化
6. 后端提交答题优化
