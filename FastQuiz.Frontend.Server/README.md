# FastQuiz 前端

使用 Nuxt4、Vue3、Nuxt UI4 和 Pinia4，构建为静态 SPA。版本与升级限制参见 [升级说明](../docs/dependency-modernization.md)。

## 环境与安装

从仓库根目录使用 `.nvmrc` 指定的 Node24 LTS；保留 npm 管理器和 `package-lock.json`，不要混用其他管理器生成锁文件。

```bash
nvm use
npm install --global npm@12.2.0
cd FastQuiz.Frontend.Server
npm ci --ignore-scripts
npm run postinstall
```

依赖安装脚本默认不执行；项目自身的 `nuxt prepare` 单独运行。镜像和 CI 使用相同流程。

## 开发与验证

```bash
npm run dev -- --host 127.0.0.1
npm run typecheck
npm run build
npm run preview
```

开发服务默认端口3000；生产反向代理公开3003。不要把开发服务器直接暴露到公网。Nuxt DevTools暂时关闭，以兼容修补过的simple-git4；原因见升级说明。

## API 客户端生成

后端以 Development 环境启动后：

```bash
npm run generate-api
```

默认读取 `http://localhost:5026/swagger/v1/swagger.json`，生成 `app/utils/backEnd.ts`。其他地址通过环境变量传入：

```bash
FASTQUIZ_OPENAPI_URL=http://127.0.0.1:5026/swagger/v1/swagger.json npm run generate-api
npm run typecheck
```

不要手改生成代码；使用HTTPS时须配置可信证书，不要关闭TLS校验。SDK生成器属于开发依赖，不进入业务运行依赖。

## Apple 风格界面与 API 配置

登录、注册、工作空间及管理界面使用统一浅色主题、响应式侧栏、圆角卡片和蓝色强调色。
字体优先使用 Apple 设备自带的 San Francisco 与苹方；其他平台使用系统字体回退。
项目不下载或分发 Apple 专有字体。

### 本地开发

`public/config.json` 中的 `apiBaseUrl` 默认设为 `/api`，适用于同域反向代理部署。
如果本地开发没有配置代理，请将它改为本地后端地址，例如 `http://localhost:5026`。
后端需要允许前端开发服务器的跨域访问；请勿将该本地地址作为生产配置提交。

### 静态生产部署

`npm run build` 的静态产物位于 `.output/public`。
将其发布到 Nginx 静态目录，保持 `config.json` 的 `apiBaseUrl` 为 `/api`，
并使用下面的路由配置（根据环境调整端口、静态目录和后端地址）：

```nginx
server {
    listen 3003;
    server_name _;
    root /opt/fast-quiz/current/web;
    index index.html;
    client_max_body_size 20m;

    location = /config.json {
        try_files $uri =404;
        add_header Cache-Control "no-store";
    }

    location ^~ /api/ {
        # 末尾 / 用于移除 /api/ 前缀。
        proxy_pass http://127.0.0.1:5026/;
        proxy_http_version 1.1;
        proxy_set_header Host $host;
        proxy_set_header X-Real-IP $remote_addr;
        proxy_set_header X-Forwarded-For $proxy_add_x_forwarded_for;
        proxy_set_header X-Forwarded-Proto $scheme;
        proxy_read_timeout 60s;
    }

    location ~* \.(?:css|js|mjs|png|jpe?g|gif|svg|ico|woff2?)$ {
        try_files $uri =404;
        expires 1y;
        add_header Cache-Control "public, immutable";
    }

    location / {
        try_files $uri $uri/ /index.html;
        add_header Cache-Control "no-cache";
        add_header X-Content-Type-Options "nosniff" always;
        add_header Referrer-Policy "strict-origin-when-cross-origin" always;
    }
}
```

不要将整个 `/tenant` 前缀转发给后端：它也是前端页面路由，会使后台页面直达或刷新失败。
发布后检查 `/api/health` 返回后端健康信息，`/tenant/system_tenant` 返回前端 HTML。
正式生产环境应另行配置 HTTPS。
