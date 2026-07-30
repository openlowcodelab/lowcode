# Docker容器化部署

<cite>
**本文引用的文件**   
- [cd/docker-compose.yml](file://cd/docker-compose.yml)
- [README.md](file://README.md)
- [src/Host/H.AppLab.Host.All/H.AppLab.Host.All/appsettings.json](file://src/Host/H.AppLab.Host.All/H.AppLab.Host.All/appsettings.json)
- [src/Host/H.AppLab.Host.All/H.AppLab.Host.All/H.AppLab.Host.All.csproj](file://src/Host/H.AppLab.Host.All/H.AppLab.Host.All/H.AppLab.Host.All.csproj)
- [src/Host/RenderEngine/H.LowCode.RenderEngine.Host/Program.cs](file://src/Host/RenderEngine/H.LowCode.RenderEngine.Host/Program.cs)
- [src/Services/Order/H.Order.Application/OrderApplicationModule.cs](file://src/Services/Order/H.Order.Application/OrderApplicationModule.cs)
</cite>

## 目录
1. [简介](#简介)
2. [项目结构](#项目结构)
3. [核心组件](#核心组件)
4. [架构总览](#架构总览)
5. [详细组件分析](#详细组件分析)
6. [依赖关系分析](#依赖关系分析)
7. [性能考量](#性能考量)
8. [故障排查指南](#故障排查指南)
9. [结论](#结论)
10. [附录](#附录)

## 简介
本文件面向AppLab平台的Docker容器化部署，聚焦于docker-compose.yml的编排与依赖服务（Redis、RabbitMQ）配置，说明各容器的环境变量、端口映射、数据卷挂载等关键设置；同时给出镜像构建最佳实践与自定义Dockerfile编写指南，并补充生产环境下的编排策略（服务发现、负载均衡、健康检查）、网络与安全加固建议。文档内容基于仓库现有配置与说明进行系统化整理，便于开发与生产环境的统一落地。

## 项目结构
- cd/docker-compose.yml：定义本地/开发环境的基础依赖服务（Redis、RabbitMQ），包含镜像版本、重启策略、环境变量、端口映射与持久化卷。
- README.md：提供本地开发步骤，包括依赖服务启动、数据库迁移、主程序运行方式与默认端口。
- src/Host/H.AppLab.Host.All/H.AppLab.Host.All/appsettings.json：聚合所有服务的连接字符串、远程服务Base地址、站点配置、日志级别等。
- src/Host/H.AppLab.Host.All/H.AppLab.Host.All/H.AppLab.Host.All.csproj：单体宿主引用了设计引擎、渲染引擎、账号、组织、审批、通知、企业、订单、设置、供应链、后台任务等多个应用模块。
- src/Host/RenderEngine/H.LowCode.RenderEngine.Host/Program.cs：渲染引擎宿主的启动管线，包含响应压缩等优化。
- src/Services/Order/H.Order.Application/OrderApplicationModule.cs：订单模块的消息总线CAP配置（默认使用内存队列，可切换至RabbitMQ/Kafka）。

```mermaid
graph TB
A["cd/docker-compose.yml"] --> B["redis:8.8-alpine"]
A --> C["rabbitmq:4.0-management-alpine"]
D["H.AppLab.Host.All (单体宿主)"] --> E["appsettings.json<br/>连接字符串/远程服务地址"]
D --> F["多个业务模块(Design/Render/Account/Org/Approval/Notification/Enterprise/Order/Setting/SupplyChain/BackgroundTask)"]
G["订单模块(Order)"] --> H["CAP消息总线<br/>默认In-Memory(可切换RabbitMQ)"]
```

**图表来源** 
- [cd/docker-compose.yml:1-32](file://cd/docker-compose.yml#L1-L32)
- [src/Host/H.AppLab.Host.All/H.AppLab.Host.All/appsettings.json:1-115](file://src/Host/H.AppLab.Host.All/H.AppLab.Host.All/appsettings.json#L1-L115)
- [src/Host/H.AppLab.Host.All/H.AppLab.Host.All/H.AppLab.Host.All.csproj:1-63](file://src/Host/H.AppLab.Host.All/H.AppLab.Host.All/H.AppLab.Host.All.csproj#L1-L63)
- [src/Services/Order/H.Order.Application/OrderApplicationModule.cs:1-49](file://src/Services/Order/H.Order.Application/OrderApplicationModule.cs#L1-L49)

**章节来源**
- [cd/docker-compose.yml:1-32](file://cd/docker-compose.yml#L1-L32)
- [README.md:38-55](file://README.md#L38-L55)
- [src/Host/H.AppLab.Host.All/H.AppLab.Host.All/appsettings.json:1-115](file://src/Host/H.AppLab.Host.All/H.AppLab.Host.All/appsettings.json#L1-L115)
- [src/Host/H.AppLab.Host.All/H.AppLab.Host.All/H.AppLab.Host.All.csproj:1-63](file://src/Host/H.AppLab.Host.All/H.AppLab.Host.All/H.AppLab.Host.All.csproj#L1-L63)
- [src/Services/Order/H.Order.Application/OrderApplicationModule.cs:1-49](file://src/Services/Order/H.Order.Application/OrderApplicationModule.cs#L1-L49)

## 核心组件
- Redis：用于缓存与会话存储等场景，使用alpine精简镜像，通过环境变量或命令行参数设置密码，暴露标准端口。
- RabbitMQ：作为消息中间件，启用管理界面，持久化数据到命名卷，暴露管理与AMQP端口。
- AppLab Host All：单体宿主聚合多模块，通过appsettings.json集中管理连接字符串与远程服务地址。
- 订单模块CAP：默认使用内存消息队列，便于开发无外部依赖；生产可切换为RabbitMQ/Kafka。

**章节来源**
- [cd/docker-compose.yml:1-32](file://cd/docker-compose.yml#L1-L32)
- [src/Host/H.AppLab.Host.All/H.AppLab.Host.All/appsettings.json:1-115](file://src/Host/H.AppLab.Host.All/H.AppLab.Host.All/appsettings.json#L1-L115)
- [src/Services/Order/H.Order.Application/OrderApplicationModule.cs:1-49](file://src/Services/Order/H.Order.Application/OrderApplicationModule.cs#L1-L49)

## 架构总览
下图展示了容器编排中的依赖服务与应用宿主的关系，以及消息总线的可选实现路径。

```mermaid
graph TB
subgraph "容器编排"
R["Redis容器"]
MQ["RabbitMQ容器"]
APP["AppLab Host All容器"]
end
subgraph "应用内部"
CAP["CAP消息总线"]
DB["SQL Server(由appsettings.json配置)"]
end
APP --> DB
APP --> CAP
CAP --> |可选| MQ
APP --> R
```

**图表来源** 
- [cd/docker-compose.yml:1-32](file://cd/docker-compose.yml#L1-L32)
- [src/Host/H.AppLab.Host.All/H.AppLab.Host.All/appsettings.json:1-115](file://src/Host/H.AppLab.Host.All/H.AppLab.Host.All/appsettings.json#L1-L115)
- [src/Services/Order/H.Order.Application/OrderApplicationModule.cs:1-49](file://src/Services/Order/H.Order.Application/OrderApplicationModule.cs#L1-L49)

## 详细组件分析

### docker-compose.yml 详解
- Redis服务
  - 镜像：使用alpine精简版，提升启动速度与体积优势。
  - 容器名与主机名：便于在容器网络中识别。
  - 重启策略：always确保服务异常退出后自动恢复。
  - 环境变量：时区、语言、REDIS_ARGS（推荐方式注入参数）。
  - 端口映射：将容器内6379映射到宿主机，便于调试访问。
  - 命令覆盖：可通过command直接传入redis-server参数（注意与REDIS_ARGS的一致性）。
- RabbitMQ服务
  - 镜像：management-alpine版本，内置Web管理界面。
  - 重启策略：on-failure仅失败时重启，避免不必要的频繁重启。
  - 端口映射：15672（管理界面）、5672（AMQP协议）。
  - 数据卷：持久化到rabbitmq_data卷，保障消息与配置不丢失。
  - 环境变量：默认用户、密码与时区。

```mermaid
flowchart TD
Start(["启动compose"]) --> Redis["启动Redis容器<br/>设置密码/端口/时区"]
Start --> MQ["启动RabbitMQ容器<br/>设置凭据/端口/持久化卷"]
Redis --> Ready["就绪监听6379"]
MQ --> Ready["就绪监听5672/15672"]
Ready --> App["应用容器连接依赖服务"]
```

**图表来源** 
- [cd/docker-compose.yml:1-32](file://cd/docker-compose.yml#L1-L32)

**章节来源**
- [cd/docker-compose.yml:1-32](file://cd/docker-compose.yml#L1-L32)

### 应用宿主与配置
- 单体宿主（H.AppLab.Host.All）
  - 通过appsettings.json集中配置各服务的连接字符串（OrganizationDb、AccountDb、DesignEngineDb、RenderEngineDb、ApprovalDb、NotificationDb、AssistantDb、EnterpriseDb、OrderDb、SettingDb、SupplyChainDb、TestingDb、BackgroundTaskDb）。
  - RemoteServices节点统一管理各子服务的Base URL，便于跨服务调用。
  - Sites节点定义站点映射（AppId与SiteUrl）。
  - Logging节点控制日志级别。
  - ExternalLogin节点配置第三方登录开关与回调路径。
- 渲染引擎（RenderEngine Host）
  - Program.cs中启用响应压缩（Brotli），提升传输效率。
  - 使用Autofac与ABP模块化加载。

```mermaid
sequenceDiagram
participant Client as "客户端"
participant Host as "AppLab Host All"
participant Service as "远程服务(BaseUrl)"
participant DB as "数据库"
Client->>Host : HTTP请求
Host->>Service : 根据RemoteServices.BaseUrl转发
Service->>DB : 读取/写入连接字符串配置
DB-->>Service : 返回数据
Service-->>Host : 响应结果
Host-->>Client : 返回最终响应
```

**图表来源** 
- [src/Host/H.AppLab.Host.All/H.AppLab.Host.All/appsettings.json:1-115](file://src/Host/H.AppLab.Host.All/H.AppLab.Host.All/appsettings.json#L1-L115)
- [src/Host/RenderEngine/H.LowCode.RenderEngine.Host/Program.cs:1-49](file://src/Host/RenderEngine/H.LowCode.RenderEngine.Host/Program.cs#L1-L49)

**章节来源**
- [src/Host/H.AppLab.Host.All/H.AppLab.Host.All/appsettings.json:1-115](file://src/Host/H.AppLab.Host.All/H.AppLab.Host.All/appsettings.json#L1-L115)
- [src/Host/RenderEngine/H.LowCode.RenderEngine.Host/Program.cs:1-49](file://src/Host/RenderEngine/H.LowCode.RenderEngine.Host/Program.cs#L1-L49)

### 消息总线与RabbitMQ集成
- 订单模块（Order Application）
  - 使用CAP框架，默认配置SqlServer作为Outbox存储，消息队列使用In-Memory（开发环境无需外部MQ）。
  - 生产环境可将UseInMemoryMessageQueue替换为UseRabbitMQ或UseKafka，保持代码一致性。
  - 失败重试次数与间隔可配置，增强可靠性。

```mermaid
flowchart TD
A["订单事件产生"] --> B["CAP Outbox写入SQL Server"]
B --> C{"消息队列实现"}
C --> |开发| D["In-Memory队列"]
C --> |生产| E["RabbitMQ/Kafka"]
D --> F["消费者处理"]
E --> F
F --> G["业务完成/失败重试"]
```

**图表来源** 
- [src/Services/Order/H.Order.Application/OrderApplicationModule.cs:1-49](file://src/Services/Order/H.Order.Application/OrderApplicationModule.cs#L1-L49)

**章节来源**
- [src/Services/Order/H.Order.Application/OrderApplicationModule.cs:1-49](file://src/Services/Order/H.Order.Application/OrderApplicationModule.cs#L1-L49)

## 依赖关系分析
- 容器层依赖
  - Redis：缓存/会话等轻量级KV存储。
  - RabbitMQ：异步消息与事件驱动（可选，当前默认内存队列）。
- 应用层依赖
  - 单体宿主聚合多模块，通过appsettings.json集中管理连接字符串与远程服务地址。
  - 订单模块CAP支持多种消息队列实现，便于从开发到生产的平滑过渡。

```mermaid
graph LR
Redis["Redis"] --> App["AppLab Host All"]
MQ["RabbitMQ"] --> App
App --> DB["SQL Server(连接字符串)"]
App --> CAP["CAP消息总线"]
CAP --> MQ
```

**图表来源** 
- [cd/docker-compose.yml:1-32](file://cd/docker-compose.yml#L1-L32)
- [src/Host/H.AppLab.Host.All/H.AppLab.Host.All/appsettings.json:1-115](file://src/Host/H.AppLab.Host.All/H.AppLab.Host.All/appsettings.json#L1-L115)
- [src/Services/Order/H.Order.Application/OrderApplicationModule.cs:1-49](file://src/Services/Order/H.Order.Application/OrderApplicationModule.cs#L1-L49)

**章节来源**
- [cd/docker-compose.yml:1-32](file://cd/docker-compose.yml#L1-L32)
- [src/Host/H.AppLab.Host.All/H.AppLab.Host.All/appsettings.json:1-115](file://src/Host/H.AppLab.Host.All/H.AppLab.Host.All/appsettings.json#L1-L115)
- [src/Services/Order/H.Order.Application/OrderApplicationModule.cs:1-49](file://src/Services/Order/H.Order.Application/OrderApplicationModule.cs#L1-L49)

## 性能考量
- 镜像选择：Redis与RabbitMQ均使用alpine精简镜像，减少体积与启动时间。
- 响应压缩：渲染引擎启用Brotli压缩，降低带宽占用，提升前端加载速度。
- 懒加载与AOT：Blazor WebAssembly按需加载程序集，Release模式启用AOT与裁剪，减小下载体积。
- 消息队列：开发使用In-Memory队列，生产切换为RabbitMQ/Kafka，提高吞吐与可靠性。

**章节来源**
- [src/Host/RenderEngine/H.LowCode.RenderEngine.Host/Program.cs:1-49](file://src/Host/RenderEngine/H.LowCode.RenderEngine.Host/Program.cs#L1-L49)
- [README.md:69-74](file://README.md#L69-L74)

## 故障排查指南
- 依赖服务未就绪
  - 检查Redis/RabbitMQ容器状态与端口映射是否正确。
  - 确认环境变量（如密码、用户）与连接字符串一致。
- 应用无法连接数据库
  - 核对appsettings.json中的连接字符串是否指向正确的SQL Server实例。
  - 确认防火墙与网络策略允许访问。
- 消息队列问题
  - 开发环境默认In-Memory队列，若切换到RabbitMQ，需确保RabbitMQ容器可用且凭据正确。
  - 查看CAP失败重试记录与日志定位消费失败原因。
- 端口冲突
  - 修改docker-compose.yml中的端口映射以避免宿主机端口冲突。

**章节来源**
- [cd/docker-compose.yml:1-32](file://cd/docker-compose.yml#L1-L32)
- [src/Host/H.AppLab.Host.All/H.AppLab.Host.All/appsettings.json:1-115](file://src/Host/H.AppLab.Host.All/H.AppLab.Host.All/appsettings.json#L1-L115)
- [src/Services/Order/H.Order.Application/OrderApplicationModule.cs:1-49](file://src/Services/Order/H.Order.Application/OrderApplicationModule.cs#L1-L49)

## 结论
通过docker-compose.yml统一编排Redis与RabbitMQ等依赖服务，结合appsettings.json集中管理连接字符串与远程服务地址，AppLab平台实现了从开发到生产的一致部署体验。订单模块的CAP消息总线支持无缝切换消息队列实现，满足不同环境的性能与可靠性需求。建议在后续迭代中完善健康检查、服务发现与负载均衡配置，进一步提升生产环境的稳定性与可观测性。

## 附录
- 镜像构建最佳实践
  - 使用多阶段构建，分离编译与运行时环境，减小最终镜像体积。
  - 固定基础镜像版本，避免不可控更新导致的不兼容。
  - 最小化权限运行，避免以root身份运行应用。
- 自定义Dockerfile编写指南
  - 明确入口点与健康检查端点。
  - 合理设置环境变量与配置文件挂载。
  - 使用.dockerignore排除无关文件，加速构建。
- 生产环境编排策略
  - 服务发现：引入Consul或Kubernetes Service Discovery。
  - 负载均衡：使用Nginx/HAProxy或云厂商LB。
  - 健康检查：为每个服务定义HTTP健康检查端点。
  - 安全加固：限制容器网络访问、启用TLS、最小化端口暴露。

[本节为通用指导，不直接分析具体文件]