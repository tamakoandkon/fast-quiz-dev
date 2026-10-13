# Nuxt Minimal Starter

Look at the [Nuxt documentation](https://nuxt.com/docs/getting-started/introduction) to learn more.

## Setup

Make sure to install dependencies:

```bash
# npm
npm install

# pnpm
pnpm install

# yarn
yarn install

# bun
bun install
```

## Development Server

Start the development server on `http://localhost:3000`:

```bash
# npm
npm run dev

# pnpm
pnpm dev

# yarn
yarn dev

# bun
bun run dev
```

## Production

Build the application for production:

```bash
# npm
npm run build

# pnpm
pnpm build

# yarn
yarn build

# bun
bun run build
```

Locally preview production build:

```bash
# npm
npm run preview

# pnpm
pnpm preview

# yarn
yarn preview

# bun
bun run preview
```

Check out the [deployment documentation](https://nuxt.com/docs/getting-started/deployment) for more information.

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
