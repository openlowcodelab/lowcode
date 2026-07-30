---
kind: configuration_system
name: 基于 ABP + appsettings.json 的模块化配置系统
category: configuration_system
scope:
    - '**'
source_files:
    - src/Host/H.AppLab.Host.All/H.AppLab.Host.All/appsettings.json
    - src/Host/Account/H.Account.Host/appsettings.json
    - src/Host/RenderEngine/H.LowCode.RenderEngine.Host/appsettings.json
    - src/Tools/H.LowCode.DbMigrator/appsettings.json
    - src/Host/H.AppLab.Host.All/H.AppLab.Host.All/HostAllModule.cs
    - src/Utils/H.Abp.HttpClientProxy/RemoteServiceOptions.cs
    - src/Services/Account/H.Account.Application/ExternalLogin/ExternalLoginOptions.cs
    - src/Agent/McpServers/H.Mcp.YunXiao/YunXiaoOptions.cs
    - src/Tools/H.LowCode.DbMigrator/Program.cs
---

## 配置系统与架构

该仓库采用 **.NET Core 内置配置系统**（Microsoft.Extensions.Configuration）结合 **ABP Framework 模块化架构**，通过 `appsettings.json` 文件进行分层配置管理。

### 核心机制
- **配置文件加载**: 每个 Host/Tool 项目独立维护自己的 `appsettings.json`，通过 `ConfigurationBuilder.AddJsonFile()` 显式加载
- **选项绑定**: 使用 `IConfiguration.GetSection()` 将 JSON 节点强类型绑定到 POCO 类（Options Pattern）
- **依赖注入**: 通过 `Configure<T>()` 方法将配置注册到 DI 容器，支持按命名空间分组
- **环境隔离**: 遵循 ASP.NET Core 约定，支持 `appsettings.Development.json`、`appsettings.Production.json` 等环境特定配置

### 配置层次结构
1. **连接字符串** (`ConnectionStrings`): 每个服务独立的数据库连接，如 `AccountDb`、`DesignEngineDb`、`RenderEngineDb` 等
2. **远程服务** (`RemoteServices`): 微服务间通信地址配置，包含 DesignEngine、RenderEngine、Account 等服务端点
3. **外部登录** (`ExternalLogin`): 第三方登录提供商配置（微信、钉钉），支持启用开关和回调路径
4. **元数据路径** (`Meta`): 低代码引擎的文件存储路径，区分 apps 和 parts 目录
5. **日志配置** (`Logging`/`Serilog`): 结构化日志输出，支持文件滚动和异步写入
6. **站点映射** (`Sites`): 应用 ID 与站点 URL 的映射关系

### 关键实现模式
- **Options 类设计**: 每个配置段都有对应的强类型类，如 `ExternalLoginOptions`、`YunXiaoOptions`、`RemoteServiceOptions`
- **模块内配置**: 在 ABP Module 的 `ConfigureServices()` 中集中配置，如 `HostAllModule.ConfigureExternalLogin()`
- **客户端代理**: `H.Abp.HttpClientProxy` 提供统一的 HTTP 客户端配置，支持动态 BaseUrl 设置
- **迁移工具**: DbMigrator 工具单独加载配置，支持 `appsettings.serilog.json` 扩展配置

### 开发规范
- 配置项必须定义在对应服务的 `appsettings.json` 中
- 敏感信息（如密钥、令牌）应通过环境变量或 Azure Key Vault 等外部配置源注入
- 新增服务需在 `RemoteServices` 中注册其 API 端点
- 外部登录提供商需同时更新 `ExternalLoginOptions` 类和配置文件