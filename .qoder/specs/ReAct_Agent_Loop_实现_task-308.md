# ReAct Agent Loop 实现方案

## Context

当前 Assistant 应用的默认智能体只支持简单 Q&A：用户提问，LLM 直接返回文本。存在以下问题：
1. **无 ReAct 循环** — 没有"思考→行动→观察"的迭代能力
2. **工具调用断裂** — 内置 4 个 Tool 类但从未被注册；LLM Provider 不发送 tools、不解析 tool_calls
3. **MCP Client 缺失** — 有 MCP Server 配置但 Agent 无法作为 Client 连接
4. **无中间步骤可见性** — 无法向前端展示推理过程

目标：实现完整的 ReAct 模式，Agent 可自主决策调用 Tool/Skill/MCP，持续思考-行动直到任务完成，并通过 SSE 流式展示每一步。

## 架构决策

采用**自定义 ReAct 循环**方案，而非仅修复 AIAgent 内置工具管道：
- 可完全控制思考→行动→观察的每一步
- 可将 ReAct 事件实时流式推送到前端
- 直接使用现有 `ILLMProvider`，无需绕道 `IChatClient` 间接层
- 保留 `FrameworkAgent` 作为简单 Q&A 回退路径

---

## Task 1: LLM Provider 工具调用增强

增强 LLM Provider 层以支持 OpenAI tool calling 协议。

### 1.1 扩展数据模型

**修改** `src/Services/Assistant/H.Assistant.Core/LLM/LLMRequest.cs`
- `Message` 类增加 `ToolCalls: List<ToolCall>?` 和 `ToolCallId: string?` 字段
- 对应 OpenAI API 中 assistant 消息的 `tool_calls` 和 tool 消息的 `tool_call_id`

**新建** `src/Services/Assistant/H.Assistant.Core/LLM/LLMStreamChunk.cs`
```
LLMStreamChunk:
  Content: string?              // 文本增量
  ToolCallDelta: ToolCallDelta? // 工具调用增量
  FinishReason: string?         // "stop" | "tool_calls"

ToolCallDelta:
  Index: int
  Id: string?
  FunctionName: string?
  FunctionArgumentsDelta: string?
```

### 1.2 修改 ILLMProvider 接口

**修改** `src/Services/Assistant/H.Assistant.Core/LLM/ILLMProvider.cs`
- `ChatStreamAsync` 返回类型从 `IAsyncEnumerable<string>` 改为 `IAsyncEnumerable<LLMStreamChunk>`

### 1.3 改造 BaiLianLLMProvider

**修改** `src/Services/Assistant/H.Assistant.Core/LLM/Providers/BaiLianLLMProvider.cs`
- `ChatAsync`: payload 增加 `tools` 字段；响应类型增加 `tool_calls`；解析 `finish_reason`
- `ChatStreamAsync`: payload 增加 `tools`；流式 tool_calls 按 index 累积拼接；返回 `LLMStreamChunk`
- 响应类型扩展：`QwenMessage.ToolCalls`、`QwenStreamDelta.ToolCalls`、`QwenChoice.FinishReason`

### 1.4 改造 DeepSeekLLMProvider

**修改** `src/Services/Assistant/H.Assistant.Core/LLM/Providers/DeepSeekLLMProvider.cs`
- 与 BaiLian 对称变更（两者使用相同的 OpenAI-compatible 格式）

---

## Task 2: 工具注册中心

### 2.1 新建 ToolRegistry

**新建** `src/Services/Assistant/H.Assistant.Core/Tools/IToolRegistry.cs` + `ToolRegistry.cs`

核心职责：
- **注册内置工具**：反射扫描 `BrowserTool`、`SearchTool`、`DbTool`、`HttpClientTool` 类中所有带 `[Description]` 的 `public static` 方法，用 `AIFunctionFactory.Create(method, null, options)` 注册
- **注册数据库技能**：修复当前 `FrameworkAgent.CreateToolFromSkill()` 中只查找 `Execute` 方法的问题，改为扫描所有带 `[Description]` 的 public 方法
- **注册 MCP 工具**：接收 MCP Client 工具列表，包装为 `AIFunction`
- 提供 `GetAllTools()`、`GetTool(name)`、`GetToolDefinitions()` 方法

### 2.2 工具定义转换为 OpenAI 格式

ToolRegistry 提供 `GetToolDefinitions()` 方法，将 `AIFunction` 列表转换为 `List<ToolDefinition>`（已在 LLMRequest.cs 中定义），用于 LLM API 请求的 `tools` 参数。

---

## Task 3: MCP Client 集成

**新建** `src/Services/Assistant/H.Assistant.Core/Mcp/McpClientManager.cs`

- 使用 `ModelContextProtocol` v1.3.0 NuGet 包（已安装）创建 MCP Client 连接
- 从 `IMcpServerAppService` 获取已启用的 MCP 服务器列表
- 调用 `ListToolsAsync()` 获取每个 MCP 服务器暴露的工具
- 将 MCP 工具（name, description, inputSchema）转换为 `AIFunction`
- 执行 MCP 工具调用 `CallToolAsync(toolName, arguments)`
- 管理连接生命周期（超时 30s，连接复用）

**修改** `src/Services/Assistant/H.Assistant.Core/H.Assistant.Core.csproj`
- 添加 `<PackageReference Include="ModelContextProtocol" />`

---

## Task 4: ReAct Agent 核心实现

### 4.1 ReAct 事件模型

**新建** `src/Services/Assistant/H.Assistant.Core/Agents/ReactEvents.cs`

```csharp
abstract class ReactEvent { Type, Timestamp, Iteration }
class ThinkingEvent : ReactEvent { Content }        // LLM 思考（流式增量）
class ToolCallingEvent : ReactEvent { ToolName, ToolCallId, Arguments }
class ToolResultEvent : ReactEvent { ToolName, ToolCallId, Result, IsError }
class FinalAnswerEvent : ReactEvent { Content }     // 最终回答（流式增量）
class ErrorEvent : ReactEvent { Message, IsFatal }
```

### 4.2 工具执行器

**新建** `src/Services/Assistant/H.Assistant.Core/Agents/ToolExecutor.cs`

- 根据 LLM 返回的 `function.name` 在 ToolRegistry 中查找 `AIFunction`
- 解析 `function.arguments` JSON 为参数
- 调用 `AIFunction.InvokeAsync(arguments)`
- 捕获异常返回错误信息（不抛异常，让 LLM 自行判断下一步）
- 工具结果超过 4000 字符时截断
- 每个工具调用 60 秒超时

### 4.3 ReactAgent 核心循环

**新建** `src/Services/Assistant/H.Assistant.Core/Agents/ReactAgent.cs`

```
async IAsyncEnumerable<ReactEvent> RunAsync(userMessage, history, tools, maxIter=10):
  messages = [system_prompt, ...history, user_message]
  
  for iteration 1..maxIter:
    request = LLMRequest { Messages=messages, Tools=toolDefs }
    
    // 流式调用 LLM
    contentBuffer = ""
    toolCallsMap = {}  // index → accumulated ToolCall
    
    foreach chunk in provider.ChatStreamAsync(request):
      if chunk.Content:
        contentBuffer += chunk.Content
        yield ThinkingEvent(chunk.Content)
      if chunk.ToolCallDelta:
        累积 toolCallsMap[index]
    
    if toolCallsMap 为空:
      yield FinalAnswerEvent(contentBuffer)
      break
    
    // 将 assistant tool_calls 消息加入 history
    messages.Add(assistantMessage with tool_calls)
    
    // 执行每个工具
    foreach toolCall in toolCallsMap:
      yield ToolCallingEvent(name, args)
      result = await toolExecutor.Execute(toolCall)
      yield ToolResultEvent(name, result, isError)
      messages.Add(toolMessage with tool_call_id + result)
  
  else: // 超过 maxIter
    yield ErrorEvent("达到最大迭代次数", isFatal=true)
```

### 4.4 ReactAgentInstance 包装器

**新建** `src/Services/Assistant/H.Assistant.Core/Agents/ReactAgentInstance.cs`
- 实现 `IAgentInstance` + `IStreamingAgent`
- `ProcessMessageStreamAsync` 内部调用 `ReactAgent.RunAsync()`，将 `ReactEvent` 序列化为 JSON 字符串 yield return
- `ProcessMessageAsync` 收集所有事件，拼接最终答案

### 4.5 更新 AgentFactory

**修改** `src/Services/Assistant/H.Assistant.Core/Agents/AgentFactory.cs`
- 注入 `IToolRegistry` 和 `McpClientManager`
- 创建 `ReactAgentInstance` 替代 `FrameworkAgentInstance`
- 默认 Agent 的 SystemPrompt 更新为包含工具使用指导：

```
你是一个具备推理和行动能力的智能助手。你可以使用各种工具来完成任务。
当需要获取信息、执行操作或分析数据时，请主动使用合适的工具。
请用简洁清晰的方式回答，并在需要时分步骤完成任务。
```

---

## Task 5: SSE 流式协议更新

### 5.1 事件协议

新的 SSE 协议（所有事件为 JSON，带 `type` 字段）：

```
data: {"type":"session","sessionId":"guid"}\n\n
data: {"type":"thinking","content":"...","iteration":1}\n\n
data: {"type":"tool_call","toolName":"SearchAsync","arguments":"{...}","iteration":1}\n\n
data: {"type":"tool_result","toolName":"SearchAsync","result":"...","isError":false,"iteration":1}\n\n
data: {"type":"answer","content":"..."}\n\n
data: {"type":"error","message":"...","isFatal":true}\n\n
data: [DONE]\n\n
```

### 5.2 后端更新

**修改** `src/Services/Assistant/H.Assistant.Application/Services/ChatMessageAppService.cs`
- `SendMessageStreamAsync` 中调用 ReactAgentInstance，将 ReactEvent 序列化为 JSON 字符串 yield return
- 保存最终答案到数据库

**修改** `src/Services/Assistant/H.Assistant.Application/Controllers/ChatController.cs`
- session 事件格式更新为 `{"type":"session","sessionId":"..."}`
- 错误事件格式更新为 `{"type":"error","message":"..."}`

---

## Task 6: 前端 ReAct 步骤展示

**修改** `src/Services/Assistant/H.Assistant.Web/Pages/Chat.razor`

### 6.1 数据模型
```csharp
class ReactStep {
    Type: string       // thinking, tool_call, tool_result, answer, error
    Content: string?   // thinking/answer 增量
    ToolName: string?
    Arguments: string?
    Result: string?
    IsError: bool
    Iteration: int
}
```

### 6.2 SSE 解析更新
在 `OnChunk` 回调中：
- 尝试 JSON 解析，检查 `type` 字段
- `thinking` → 追加到 `reactThinkingText` 缓冲区
- `tool_call` → 添加到 `reactSteps` 列表，显示工具调用卡片
- `tool_result` → 更新对应 tool_call 的结果
- `answer` → 追加到 `streamingResponse`
- `error` → 显示错误提示
- 向后兼容：如果 JSON 解析失败或无 `type`，按旧格式纯文本处理

### 6.3 UI 渲染
在 assistant 消息区域内新增 ReAct 步骤渲染：
- **思考阶段**：带折叠的思考内容块，"Thinking..." 动画指示器
- **工具调用卡片**：显示工具名称 + 参数 + 执行状态（spinner/成功/失败）+ 可折叠结果
- **最终回答**：正常 Markdown 渲染

---

## Task 7: 错误处理与循环安全

集成在 Task 4 的实现中：
- **最大迭代次数**：默认 10，可通过 AgentDto.Metadata JSON 配置
- **工具执行超时**：60 秒/次，超时返回错误信息给 LLM
- **工具执行异常**：捕获返回错误信息，不中断循环
- **LLM 调用失败**：重试 1 次（指数退避），失败则终止
- **Token 保护**：messages 过长时截断，工具结果超 4000 字符截断

---

## 文件变更汇总

### 新建 (7 个)
| 文件 | 用途 |
|------|------|
| `H.Assistant.Core/Agents/ReactEvents.cs` | ReAct 事件类型 |
| `H.Assistant.Core/Agents/ReactAgent.cs` | ReAct 循环核心 |
| `H.Assistant.Core/Agents/ReactAgentInstance.cs` | Agent 实例包装器 |
| `H.Assistant.Core/Agents/ToolExecutor.cs` | 工具执行器 |
| `H.Assistant.Core/LLM/LLMStreamChunk.cs` | 流式 chunk 类型 |
| `H.Assistant.Core/Tools/IToolRegistry.cs` | 工具注册中心接口 |
| `H.Assistant.Core/Tools/ToolRegistry.cs` | 工具注册中心实现 |
| `H.Assistant.Core/Mcp/McpClientManager.cs` | MCP Client 管理器 |

### 修改 (9 个)
| 文件 | 变更 |
|------|------|
| `H.Assistant.Core/LLM/LLMRequest.cs` | Message 加 ToolCalls/ToolCallId |
| `H.Assistant.Core/LLM/ILLMProvider.cs` | ChatStreamAsync 返回 LLMStreamChunk |
| `H.Assistant.Core/LLM/Providers/BaiLianLLMProvider.cs` | 支持 tools + tool_calls |
| `H.Assistant.Core/LLM/Providers/DeepSeekLLMProvider.cs` | 同上 |
| `H.Assistant.Core/Agents/AgentFactory.cs` | 注入 ToolRegistry + McpClientManager |
| `H.Assistant.Core/H.Assistant.Core.csproj` | 添加 ModelContextProtocol 包 |
| `H.Assistant.Application/Services/ChatMessageAppService.cs` | 流式 ReAct 事件 |
| `H.Assistant.Application/Controllers/ChatController.cs` | SSE 结构化事件 |
| `H.Assistant.Web/Pages/Chat.razor` | ReAct UI 渲染 |

---

## 验证方式

1. **编译验证**：`dotnet build src/H.LowCode.slnx` 无报错
2. **单元测试**：启动应用后，在 Chat 页面发送需要工具调用的消息（如"帮我搜索最新的 .NET 10 特性"），验证：
   - 前端显示 "Thinking" 步骤
   - 前端显示工具调用卡片（工具名、参数、结果）
   - 最终回答正确包含工具返回的信息
3. **MCP 验证**：配置云效 MCP Server 后，发送"查询云效项目列表"验证 MCP 工具被调用
4. **循环安全验证**：发送可能导致无限循环的消息，确认最大迭代次数限制生效
