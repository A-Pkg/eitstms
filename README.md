# EITSTMS 企业 IT 服务工单管理平台

基于 ASP.NET Core MVC 的个人项目，用于管理企业内部 IT 服务工单，覆盖员工提交、管理员分配、技术人员处理和工单历史查询等流程。

## 技术栈

- .NET 10 / ASP.NET Core MVC
- Entity Framework Core 10 / SQL Server 2022
- ASP.NET Core Identity：用户认证与角色授权
- Razor Views / Bootstrap / jQuery
- Docker / Docker Compose

## 主要功能

- 用户登录、退出、个人资料维护与密码修改
- Employee、Technician、Admin 三类角色的权限控制
- 工单创建、分配、补充信息、处理和完成
- 工单操作历史查询与到期自动关闭
- 用户管理与 IT 资产管理

## 项目结构

| 路径                   | 说明                                 |
| ---------------------- | ------------------------------------ |
| `Controllers/`         | MVC 控制器                           |
| `Models/`              | 实体、枚举和视图模型                 |
| `Views/`               | Razor 页面                           |
| `Data/`                | DbContext、实体配置和初始化逻辑      |
| `Migrations/`          | EF Core 数据库迁移                   |
| `Services/`            | 自定义 Claims 工厂与工单自动关闭服务 |
| `wwwroot/`             | CSS、JavaScript 等静态资源           |
| `docker/sqlserver/`    | 数据库应用账号初始化脚本             |
| `Dockerfile`           | Web 镜像构建配置                     |
| `Dockerfile.migration` | 数据库迁移程序镜像构建配置           |
| `docker-compose.yml`   | 服务编排、数据卷和 secrets 配置      |

## Docker 启动

安装并启动 Docker，确保 Docker Compose 可用，并使用 Linux 容器模式。以下命令在包含 `docker-compose.yml` 的项目根目录执行。

### 1. 准备机密文件

在项目目录的上一级创建 `eitstms-secrets` 文件夹，使其与项目目录同级。按 Compose 中的路径准备以下文件：

| 文件                       | 内容                                 |
| -------------------------- | ------------------------------------ |
| `sa-password.txt`          | SQL Server 的 sa 密码                |
| `app-db-password.txt`      | 应用数据库账号 `eitstms_app` 的密码  |
| `migration-connection.txt` | 使用 sa 的数据库迁移连接字符串       |
| `web-connection.txt`       | 使用 `eitstms_app` 的 Web 连接字符串 |
| `seed-admin-password.txt`  | 网站初始化管理员密码                 |

迁移连接字符串模板：

```text
Server=sqlserver;Database=EITSTMSDb;User Id=sa;Password="替换为sa密码";TrustServerCertificate=True;
```

Web 连接字符串模板：

```text
Server=sqlserver;Database=EITSTMSDb;User Id=eitstms_app;Password="替换为应用数据库密码";TrustServerCertificate=True;
```

每个文件使用 UTF-8 无 BOM 编码，内容写在一行内。两条连接字符串的密码必须分别与对应密码文件一致。机密文件不提交到 Git。

### 2. 构建并启动

```bash
docker compose config -q
docker compose up -d --build
docker compose ps -a
```

Compose 按依赖顺序启动 SQL Server、执行数据库迁移、创建应用数据库账号，最后启动 Web。`migration` 和 `db-init` 完成后显示 `Exited (0)` 属于正常情况；`sqlserver` 和 `web` 应保持运行。

### 3. 访问项目

浏览器打开：<http://localhost:8080>

首次初始化管理员用户名为 `Admin`，密码来自 `seed-admin-password.txt`。修改该文件不会自动重置数据库中已存在的管理员密码。

## 常用命令

```bash
# 查看所有服务状态，包括已经退出的一次性任务
docker compose ps -a

# 查看 Web 日志
docker compose logs --tail=100 web

# 修改源码后重新构建并启动
docker compose up -d --build

# 停止服务，保留容器
docker compose stop

# 删除服务容器和默认网络，保留数据库命名卷
docker compose down
```

## 配置说明

- Docker 中使用 `Production` 环境，通过 Compose secrets 提供数据库连接字符串和管理员密码。
- Windows 本机开发可使用用户机密或本机的 `appsettings.Development.json`；后者已从 Git 和 Docker 构建上下文中排除。
- 容器内通过服务名 `sqlserver` 连接数据库；本机 SSMS 可通过 `localhost,1433` 连接容器数据库。
- 数据库使用命名卷 `eitstms_sqlserver_data_v1`。重建容器会复用已有数据，修改 sa 密码文件不会自动重置已有数据库的 sa 密码。
- 普通 Compose 的文件型 secrets 不会加密宿主机上的源文件，需要自行妥善保存。
