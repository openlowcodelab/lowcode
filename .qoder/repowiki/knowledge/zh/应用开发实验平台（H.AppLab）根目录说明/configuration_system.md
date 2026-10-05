## 概述

H.AppLab 仓库采用 .NET 标准的 `Microsoft.Extensions.Configuration` 作为统一配置加载框架，所有宿主进程（Web Host、Desktop、DbMigrator、RenderEngine Host）均通过 `appsettings.json` 及其环境变体（`appsettings.Development.json`、`appsettings.serilog.json`）进行运行时配置。配置按“连接串 + 业务选项 + 远程服务地址”三类组织，并通过强类型 Options 类注入到各模块。

## 关键文件与位置

- Web 宿主入口：`src/Host/H.AppLab.Web.Host/H.AppLab.Web.Host/Program.cs`
- 桌面宿主入口：`src/Host/H.AppLab.Desktop/Program.cs`
- Web 宿主默认配置：`src/Host/H.AppLab.Web.Host/H.AppLab.Web.Host/appsettings.json`
- 桌面默认配置：`src/Host/H.AppLab.Desktop/appsettings.json`
- DbMigrator 通用模板：`src/Tools/H.*.DbMigrator/appsettings.json`（每个领域一个）
- Serilog 独立日志配置：`src/Tools/H.LowCode.DbMigrator/appsettings.serilog.json`
- Blazor Client 端配置：`src/Host/H.AppLab.Web.Host/H.AppLab.Web.Host.Client/wwwroot/appsettings.json`
- HttpClientProxy 配置模型：`src/Utils/H.Abp.HttpClientProxy/RemoteServiceOptions.cs`
- 外部集成选项：`src/Agent/McpServers/H.Mcp.YunXiao/YunXiaoOptions.cs`

## 架构与约定

### 1. 配置源层次

- 标准 ASP.NET Core / Generic Host：`Host.CreateDefaultBuilder(args)` 自动从 `appsettings.json` → `appsettings.{Environment}.json` → 环境变量 → CLI args 顺序加载，遵循 .NET 默认优先级。
- DbMigrator 自定义加载：`src/Tools/H.LowCode.DbMigrator/Program.cs` 中手动构建 `ConfigurationBuilder`，显式添加 `appsettings.json`（required）和 `appsettings.serilog.json`（optional），再交给 Serilog 的 `ReadFrom.Configuration`。
- Blazor WebAssembly 客户端：使用 `wwwroot/appsettings.json` 由浏览器在运行时 `fetch` 加载，与服务端 `appsettings.json` 结构对称但仅包含 `RemoteServices` 节点。

### 2. 配置分类与键命名

`appsettings.json` 顶层键按语义分组：

| 键路径 | 用途 | 示例 |
|---|---|---|
| `ConnectionStrings.*` | 各领域数据库连接串（OrganizationDb、AccountDb、WorkbenchDb 等） | LocalDB 默认值 |
| `Meta.appsFilePath` / `Meta.partsFilePath` | LowCode 元数据 JSON 文件目录 | 相对路径 `../../../LowCode/meta/apps` |
| `Testing.templatesPath` | 测试模板目录 | 相对路径 |
| `Workbench.*` | Workbench 工具超时、隔离、预算、Git、Browser、Notify 等运行参数 | `ToolTimeoutSeconds=660` |
| `RemoteServices.*.BaseUrl` | 微服务 URL（DesignEngine、RenderEngine、Account、Organization …） | `https://localhost:7065` |
| `Sites[]` | 已注册站点映射（AppId → SiteUrl） | 数组项含 AppId/SiteUrl |
| `Logging.LogLevel.*` | 结构化日志级别（Default/System/Microsoft/Volo） | Information/Warning |
| `YunXiao.*` | 云效 MCP 集成凭据（OrganizationId、PersonalAccessToken、Endpoint） | 敏感字段 |
| `ExternalLogin.WeChat/DingTalk.*` | 第三方登录开关、ClientId、ClientSecret、CallbackPath | Enabled=false 占位 |
| `Minio.*` | 对象存储 Endpoint、AccessKey、SecretKey、UseSsl、ExternalEndpoint | 本地 MinIO 默认值 |
| `Http.AllowInvalidCertificates` | 桌面端允许自签名证书 | Avalonia 专用 |

### 3. 强类型 Options 绑定

- `RemoteServiceOptions`（`src/Utils/H.Abp.HttpClientProxy/RemoteServiceOptions.cs`）：提供 `this[string name]` 索引访问和 `GetBaseUrl(serviceName)`，对未配置的服务名返回空字符串而非抛异常，体现“缺省即空”的安全策略。
- `YunXiaoOptions`（`src/Agent/McpServers/H.Mcp.YunXiao/YunXiaoOptions.cs`）：声明 `SectionName = "YunXiao"`，配合 `IConfiguration.GetSection("YunXiao")` 绑定，是外部 MCP 集成的配置契约。
- 其他 Options 类（`MinioOptions`、`ExternalLoginOptions`、`WorkbenchToolOptions`）在各应用层按同名 Section 自行绑定。

### 4. 前端与后端配置同步约定

Blazor Client 端的 `wwwroot/appsettings.json` 与服务端 `appsettings.json` 共享 `RemoteServices` 节点，用于前后端同时解析远端 API BaseUrl；服务端额外承载连接串、路径、开关等不应暴露给浏览器的配置。

## 约定与约束

- **配置文件格式**：所有运行时配置以 JSON（`appsettings*.json`）表达，未发现 YAML/TOML/.env 用法。
- **环境分离**：通过 `appsettings.Development.json` 覆盖开发期行为（如 SignalR DetailedErrors、调试模式），部署时由环境变量或替换后的 `appsettings.json` 提供生产值——这是 .NET 默认机制，非自定义实现。
- **敏感信息**：`PersonalAccessToken`、`ClientSecret`、`SecretKey`、`ConnectionStrings` 等敏感字段均以明文占位形式存在于源码中，仓库未引入 `.gitignore` 保护之外的 secret manager；这些字段应通过部署时环境变量或容器 Secret 注入覆盖。
- **远程服务地址集中化**：新增业务服务需在服务端与客户端两份 `appsettings.json` 的 `RemoteServices` 节点下同时增加 `{ServiceName.BaseUrl}`，否则 HttpClientProxy 无法找到该服务。
- **路径为相对路径**：`Meta.*`、`Testing.*` 中的文件系统路径均为相对于可执行目录的相对路径（如 `../../../LowCode/meta/apps`），部署时需保证工作目录一致。
- **AbpModule 装配**：Web 宿主通过 `builder.AddApplicationAsync<HostAllModule>()` 将 ABP 模块注册到 DI，配置由 ABP 的 `IConfiguration` 注入到各 Module 的 Options 类，而非直接读 `Configuration` 树。
- **日志配置独立文件**：Serilog 配置位于单独的 `appsettings.serilog.json`，由 DbMigrator 的 `ConfigSerilog()` 单独读取，与主 `appsettings.json` 解耦。
- **无 feature flag 系统**：仓库未发现统一的特性开关/Feature Flag 框架；功能开关以布尔配置键（如 `Workbench.ToolIsolationEnabled`、`ExternalLogin.*.Enabled`、`Workbench.Verification.Enabled`）的形式内联于各模块配置中。