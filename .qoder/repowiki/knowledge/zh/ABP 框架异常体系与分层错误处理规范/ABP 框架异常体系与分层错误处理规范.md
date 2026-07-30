---
kind: error_handling
name: ABP 框架异常体系与分层错误处理规范
category: error_handling
scope:
    - '**'
source_files:
    - src/Services/Approval/H.Approval.Application/Services/ApprovalCategoryAppService.cs
    - src/Services/BackgroundTask/H.BackgroundTask.Application/Services/BackgroundJobAppService.cs
    - src/Services/Notification/H.Notification.Application/Services/NotificationBusinessAppService.cs
    - src/LowCode/DesignEngine/H.LowCode.DesignEngine.Application/AppServices/PageAppService.cs
    - src/Agent/Assistant/H.Assistant.Application/Services/TaskAppService.cs
    - src/LowCode/DesignEngine/H.LowCode.DesignEngine.EntityFrameworkCore/EntityManager/EntityTypeManager.cs
    - src/LowCode/RenderEngine/H.LowCode.RenderEngine.EntityFrameworkCore/EntityManager/EntityTypeManager.cs
---

## 1. 采用的异常体系
本仓库基于 Volo.ABP 框架，统一使用 ABP 内置的异常类型进行错误表达与传播：
- `UserFriendlyException`：用户可感知的业务校验失败或操作拒绝（如参数为空、重复值、权限不足等），由 ABP 全局异常处理器转换为 HTTP 400/422 响应。
- `BusinessException`：领域层业务规则违反（如“内置角色不可删除”“应用不存在”），通常表示调用方传入的业务数据不合法。
- `EntityNotFoundException`：实体未找到（通过 `typeof(Entity)` + id 构造），用于仓储查询返回 null 的场景。
- `ValidationException`：模型/Schema 验证失败（低代码引擎中用于属性必填校验等）。
- 其他 .NET 标准异常（如 `InvalidOperationException`、`ArgumentNullException`、`ArgumentException`）用于编程错误与参数校验。

## 2. 关键文件与位置
- 各 Application 层 AppService 是错误抛出的主要位置，例如：
  - `src/Services/Approval/H.Approval.Application/Services/ApprovalCategoryAppService.cs`
  - `src/Services/BackgroundTask/H.BackgroundTask.Application/Services/BackgroundJobAppService.cs`
  - `src/Services/Notification/H.Notification.Application/Services/NotificationBusinessAppService.cs`
  - `src/LowCode/DesignEngine/H.LowCode.DesignEngine.Application/AppServices/PageAppService.cs`
  - `src/Agent/Assistant/H.Assistant.Application/Services/TaskAppService.cs`
- 低代码引擎的 Entity Manager 中使用 `ValidationException` 做 Schema 校验：
  - `src/LowCode/DesignEngine/H.LowCode.DesignEngine.EntityFrameworkCore/EntityManager/EntityTypeManager.cs`
  - `src/LowCode/RenderEngine/H.LowCode.RenderEngine.EntityFrameworkCore/EntityManager/EntityTypeManager.cs`

## 3. 架构与约定
- **分层职责**：Application Service 负责输入校验与业务规则检查，直接抛出 `UserFriendlyException` / `BusinessException` / `EntityNotFoundException`；Domain/Infrastructure 层一般不捕获这些异常，交由上层或 ABP 全局中间件处理。
- **实体查找模式**：通过 Repository 获取实体后判空，若为 null 则抛 `EntityNotFoundException(typeof(Entity), id)`，保证客户端能区分“找不到实体”和“其他错误”。
- **参数校验模式**：对必填字段、格式限制（如正则）、依赖存在性（外键）等校验失败时，抛 `UserFriendlyException` 并附带中文提示消息。
- **业务规则模式**：对领域约束（如“内置角色不可删除”“分类已存在”）抛 `BusinessException`，语义上区别于用户输入错误。
- **后台任务容错**：在定时任务执行路径中使用 try/catch 包裹核心逻辑，记录日志并写入失败日志，避免单次失败导致整个调度崩溃（见 `TaskAppService.ExecuteTaskInternalAsync`）。
- **全局异常处理**：依赖 ABP 的默认 Exception Handling Middleware，将上述异常统一转换为结构化 JSON 响应，无需在各控制器中手动 try/catch。

## 4. 开发者应遵循的规则
1. **优先使用 ABP 异常类型**：用户可见错误用 `UserFriendlyException`，业务规则违反用 `BusinessException`，实体不存在用 `EntityNotFoundException`，模型校验失败用 `ValidationException`。
2. **不在 Service 层吞掉异常**：不要 catch 后再 return 默认值，应让异常向上冒泡至 ABP 全局处理器。
3. **实体查询必须判空**：Repository 返回 null 时必须抛 `EntityNotFoundException`，禁止返回 null 给调用方。
4. **参数校验集中化**：在方法入口处尽早校验输入，失败即抛 `UserFriendlyException`，保持后续逻辑无防御性分支。
5. **后台任务需兜底**：异步/定时任务内部使用 try/catch 记录错误并继续运行，避免影响调度器。
6. **消息语言**：面向用户的异常消息使用中文，便于前端直接展示；内部调试信息可通过 ILogger 输出。
7. **避免混用 .NET 标准异常与业务异常**：仅对真正的编程错误（如 null 引用、非法参数）使用 `ArgumentNullException`/`ArgumentException`，业务语义错误一律使用 ABP 异常类型。