按你的顺序来，一步步操作。**第 1 步和第 2 步一定要仔细，尤其是禁用密码登录前必须先测试密钥。**

---

### 第一步：配置 SSH 密钥

#### 1.1 在本地电脑生成密钥

打开你本地电脑的终端（Windows 用 PowerShell 或 Git Bash，Mac/Linux 用自带终端）：

```bash
ssh-keygen -t ed25519 -C "你的备注"
```

- 看到 `Enter file in which to save the key`：**直接按回车**，用默认路径。
- 看到 `Enter passphrase`：**直接按回车两次**（留空，方便后续自动登录）。

#### 1.2 把公钥上传到服务器

```bash
ssh-copy-id root@你的服务器IP
```

会提示输入服务器密码，输完后显示 `Number of key(s) added: 1` 就成功了。

#### 1.3 测试密钥登录（关键一步）

**不要关闭当前服务器终端窗口。** 另外打开一个新的本地终端：

```bash
ssh -o PreferredAuthentications=publickey root@你的服务器IP
```

如果**不需要密码直接进去了**，说明密钥生效。如果还是要密码，**先停下来排查**，不要继续下一步。

#### 1.4 禁用密码登录

确认密钥登录成功后，回到服务器终端操作：

```bash
# 先备份
sudo cp /etc/ssh/sshd_config /etc/ssh/sshd_config.bak

# 禁用密码登录（同时处理主文件和 conf.d 目录下的覆盖文件）
sudo sed -i 's/^#\?PasswordAuthentication.*/PasswordAuthentication no/' /etc/ssh/sshd_config
sudo sed -i 's/^#\?PasswordAuthentication.*/PasswordAuthentication no/' /etc/ssh/sshd_config.d/*.conf 2>/dev/null

# 检查语法
sudo sshd -t

# 没有报错则重启
sudo systemctl restart ssh
```

重启后，**再次新开一个终端用密钥登录测试**。确认能进，再关闭最初的密码会话。

---

### 第二步：安装 .NET 8 运行时

Debian 12 用微软官方源安装：

```bash
# 1. 添加微软包签名密钥和源
wget https://packages.microsoft.com/config/debian/12/packages-microsoft-prod.deb -O packages-microsoft-prod.deb
sudo dpkg -i packages-microsoft-prod.deb
rm packages-microsoft-prod.deb

# 2. 更新并安装 ASP.NET Core 运行时（含 .NET 运行时）
sudo apt update
sudo apt install -y aspnetcore-runtime-8.0
```

安装完成后验证：

```bash
dotnet --list-runtimes
```

应该能看到 `Microsoft.AspNetCore.App 8.0.x` 和 `Microsoft.NETCore.App 8.0.x`。

> 如果你只需要最基础的 .NET 运行时（不含 ASP.NET Core），把 `aspnetcore-runtime-8.0` 换成 `dotnet-runtime-8.0`。但一般后端服务用 `aspnetcore` 更省事。

---

### 第四步：配置 systemd 自动重启

假设你的后端程序发布在 `/var/www/myapp/`，入口文件是 `MyApp.dll`。

#### 4.1 创建 systemd 服务文件

```bash
sudo nano /etc/systemd/system/myapp.service
```

写入以下内容：

```ini
[Unit]
Description=My .NET Backend Service
After=network.target

[Service]
WorkingDirectory=/var/www/myapp
ExecStart=/usr/bin/dotnet /var/www/myapp/MyApp.dll
Restart=always
RestartSec=10
KillSignal=SIGINT
SyslogIdentifier=myapp
User=www-data
Environment=ASPNETCORE_ENVIRONMENT=Production
Environment=DOTNET_NOLOGO=true

[Install]
WantedBy=multi-user.target
```

**需要改的地方：**

- `WorkingDirectory`：改成你程序实际存放的目录
- `ExecStart` 的路径：改成你的 `dll` 文件实际路径
- `User=www-data`：如果程序需要写文件权限，确认 `www-data` 对程序目录有权限

`Restart=always` 和 `RestartSec=10` 就是实现崩溃后 10 秒自动重启的关键。

#### 4.2 启动并设为开机自启

```bash
sudo systemctl daemon-reload
sudo systemctl enable myapp
sudo systemctl start myapp
```

#### 4.3 查看运行状态

```bash
sudo systemctl status myapp
sudo journalctl -u myapp -f
```

`journalctl -f` 可以实时看日志，调试很有用。

---

先按第一步把 SSH 配好，**测试通过后再动第二步**。SSH 这块有任何报错截图发我，我帮你看。
