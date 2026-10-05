## 1. 总体方案

本项目为 .NET + Blazor 多模块平台（ABP Framework），错误处理采用 **三层混合模式**：
- **Web 层**：ASP.NET Core 中间件 + ABP `UserFriendlyException`/`ValidationException`
- **应用服务层**：业务校验抛 `UserFriendlyException`，数据约束抛 `ValidationException`
- **客户端/工具层**：返回统一的 `BaseOutput<T>` 结果对象（含 `Success`/`Code`/`Message`）

未定义项目自有的异常类型，也未使用 `panic/recover`（Go 语言概念，本仓库为 C#）。全局无自定义 ExceptionFilter；错误最终由 ASP.NET Core `UseExceptionHandler("/Error")` 兜底。

## 2. 关键文件与包

| 层级 | 文件/包 | 职责 |
|---|---|---|
| Web 启动 | `src/Host/H.AppLab.Web.Host/H.AppLab.Web.Host/Program.cs` | 注册 `UseExceptionHandler("/Error")`、`UseStatusCodePagesWithReExecute("/not-found")`、SignalR `EnableDetailedErrors`（仅开发环境） |
| 错误页面 | `src/Host/H.AppLab.Web.Host/H.AppLab.Web.Host/Components/Pages/Error.razor` | 生产环境统一错误页，展示 RequestId 和开发提示 |
| 业务异常 | ABP 框架的 `UserFriendlyException` / `ValidationException` | 业务校验失败（用户可理解）、参数/模型验证失败 |
| 统一输出 | `src/Utils/H.Util.Base/BaseOutput.cs` | 轻量级 `BaseOutput` / `BaseOutput<T>`，`code==0` 即成功 |
| HTTP 代理 | `src/Utils/H.Abp.HttpClientProxy/HttpClientProxyInterceptor.cs` | 调用远端 API 时 `EnsureSuccessStatusCode()` 抛出异常，作为跨服务错误入口 |
| Toast 通知 | `src/Utils/H.Util.Blazor/HToastService.cs` | 前端统一 `Success/Error/Warning/Info` 消息弹窗 |
| 迁移工具 | `src/Tools/*/Program.cs` | 数据库迁移失败时 `catch (Exception ex)` 打印堆栈后退出 |

## 3. 架构与约定

### 3.1 Web 管道中的错误传播
`Program.cs` 中按环境分支：
```csharp
if (app.Environment.IsDevelopment())
{
    app.UseWebAssemblyDebugging();
}
else
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    app.UseHsts();
}
```
非开发环境所有未处理异常均被重定向到 `/Error` Razor 页面；4xx 状态码通过 `UseStatusCodePagesWithReExecute("/not-found", ...)` 路由到通用 NotFound 页。SignalR 在开发环境开启 `EnableDetailedErrors = true`，生产关闭。

### 3.2 应用服务层的异常用法
业务校验失败直接抛 `UserFriendlyException`，例如配置管理、测试模板等模块大量使用：
```csharp
throw new UserFriendlyException("配置名称不能为空");
throw new UserFriendlyException($"配置项 "{name}"（提供者 {providerName}）已存在");
```
数据/模型校验使用 ABP 内置 `ValidationException`（如 `EntityTypeManager.cs` 抛 `primary is required`）。

### 3.3 统一返回体 `BaseOutput<T>`
`BaseOutput` 提供 `Success`、`Code`、`Message` 三个字段，默认构造 `Success=true, Code=0`；带 message 的构造设置 `Success=false, Code=1`。泛型版本额外提供 `Data`。该类型在 SystemPortal 等服务中被广泛用作方法返回值，表示“操作成功/失败”而非通过异常表达正常业务流程分支。

### 3.4 外部调用与日志
- `YunXiaoApiClient.cs` 在每次远程调用外层 `try/catch (Exception ex)`，记录 `_logger.LogError(ex, ...)` 并返回错误字符串给上层，避免异常穿透 MCP 协议。
- `BrowserSessionPool.cs`、`SpreadsheetTool.cs`、`OfficeDocumentTool.cs` 中对资源释放使用空 catch（`catch { /* 关闭尽力而为 */ }`），属于“best-effort cleanup”模式。
- `HttpClientProxyInterceptor` 对远端响应调用 `response.EnsureSuccessStatusCode()`，将非 2xx 转为异常，交由上层或中间件处理。

### 3.5 前端错误展示
`HToastService` 提供 `Success/Error/Warning/Info` 四类消息，通过事件驱动 UI 更新，是 Blazor 组件显示业务错误的主要手段。

## 4. 观察到的约定与约束

- **业务校验**：在服务方法内直接 `throw new UserFriendlyException(中文消息)`，不封装自定义异常类型。
- **模型/参数校验**：使用 ABP 的 `ValidationException`，由 ABP MVC 自动转换为 400 响应。
- **跨服务调用**：HTTP 客户端一律通过 `HttpClientProxyInterceptor`，由拦截器统一 `EnsureSuccessStatusCode()` 转异常，禁止在调用点自行解析 HTTP 状态码。
- **清理代码**：Dispose/Close 等幂等资源释放处普遍用空 `catch` 包裹，注释标明“尽力而为”，不应让清理失败影响主流程。
- **CLI/迁移工具**：`catch (Exception ex)` 后仅 `Console.WriteLine` 堆栈，不吞异常也不向上抛。
- **未覆盖点**：仓库未定义全局 ExceptionFilter/ExceptionMiddleware，也没有统一的 error code 枚举——`BaseOutput.Code` 目前仅在构造时硬编码 `0/1`，未被业务逻辑广泛区分。
- **强制规则**：生产环境禁用详细异常信息（`UseExceptionHandler` 指向 `/Error` 且 SignalR `EnableDetailedErrors` 仅开发启用），这是由 Program.cs 显式分支保证的行为约束。

## 5. 结论

该仓库的错误处理以 ABP 框架为基础，辅以自研的 `BaseOutput<T>` 统一返回体和 `HToastService` 前端通知。没有建立独立的错误分类体系（如错误码表、自定义异常类），而是依赖 ABP 内置异常类型与 ASP.NET Core 中间件完成从业务层到 HTTP 层的错误传播。整体设计简洁、约定清晰，适合中小型多模块平台，但在错误可观测性（结构化错误码、统一错误追踪）方面仍有扩展空间。