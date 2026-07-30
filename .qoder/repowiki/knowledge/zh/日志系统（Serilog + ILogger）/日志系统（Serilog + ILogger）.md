---
kind: logging_system
name: 日志系统（Serilog + ILogger）
category: logging_system
scope:
    - '**'
source_files:
    - src/Tools/H.LowCode.DbMigrator/Program.cs
    - src/Tools/H.LowCode.DbMigrator/appsettings.serilog.json
    - src/Host/RenderEngine/H.LowCode.RenderEngine.Host/appsettings.json
    - src/Agent/Assistant/H.Assistant.Core/Agents/AgentFactory.cs
    - src/Agent/Assistant/H.Assistant.Application/Services/ChatMessageAppService.cs
---

本仓库采用 **Serilog** 作为核心日志框架，结合 .NET 内置的 `ILogger` 抽象进行统一记录。日志配置通过 `appsettings.json` / `appsettings.serilog.json` 文件集中管理，支持异步写入、文件滚动与控制台输出。

### 1. 使用的框架与工具
- **Serilog**：结构化日志库，提供丰富的 Sinks（File、Console、Async 等）
- **Serilog.Settings.Configuration**：从配置文件读取 Serilog 设置
- **Serilog.Extensions.Logging**：将 Serilog 接入 .NET `ILogger` 管道
- **Microsoft.Extensions.Logging**：标准日志抽象，在各模块中通过依赖注入使用 `ILogger<T>`

### 2. 关键文件与位置
- **H.LowCode.DbMigrator/Program.cs**：控制台程序入口，手动初始化 `Log.Logger`，加载 `appsettings.json` 和 `appsettings.serilog.json`
- **H.LowCode.DbMigrator/appsettings.serilog.json**：独立的 Serilog 配置，定义 MinimumLevel、Sinks 及输出模板
- **Host/RenderEngine/H.LowCode.RenderEngine.Host/appsettings.json**：Web Host 内嵌 Serilog 配置，包含 File + Console Sink
- 各业务模块（如 Assistant.Core、Assistant.Application）通过构造函数注入 `ILogger<T>` 使用日志

### 3. 架构与约定
- **双轨配置**：控制台/迁移工具使用独立 `appsettings.serilog.json`；Web Host 将 Serilog 配置直接放在 `appsettings.json` 的 `Serilog` 节点下
- **默认日志级别**：Default = Information，System/Microsoft = Warning，EF Core 根据环境在 Information/Warning 间切换
- **输出目标**：统一使用 Async → File（按日滚动、单文件 10MB、保留 50 个）+ Console（开发环境 Information，生产 Error）
- **结构化字段**：输出模板包含 `{Timestamp}`、`{Level}`、`{SourceContext}`、`{EventId}`、`{Message}`、`{Exception}`，便于后续采集与分析
- **DI 集成**：通过 `AddSerilog()` 将 Serilog 注册到 `IServiceCollection`，业务代码仅依赖 `ILogger<T>`，实现解耦

### 4. 开发者应遵循的规则
- 在类中通过构造函数注入 `ILogger<T>` 实例，不要直接使用 `Log.Logger`（除控制台工具外）
- 使用结构化日志消息，避免拼接字符串；必要时以参数形式传入上下文数据
- 日志级别选择：Information 记录关键业务流程，Warning 记录可恢复异常，Error 记录失败路径
- 不要在日志中输出敏感信息（密码、Token、用户隐私等）
- 新增模块时保持与现有 `appsettings.json` 中 `Serilog` 节点一致的 MinimumLevel 与 Sink 配置