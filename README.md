### 概述
* 应用开发实验性项目，基于 .NET + Blazor 实现
* 采用模块化架构，支持单体部署与按服务独立部署两种方式

### 代码结构
* Host
  * 项目的宿主程序, 不包含任何业务逻辑, 仅进行业务应用的服务注册
  * H.AppLab.Host.All 为所有服务的宿主(单体应用), 其他 Host 项目为单服务宿主 (Account、RenderEngine、H.LowCode.Host.All 等)
  * 宿主均采用 Blazor Web App 模式 (Server + WebAssembly Client)

* Components 共享 UI 组件
  * AppDrawer：应用抽屉组件，提供统一的顶部导航、侧边菜单与应用切换能力，供各应用复用

* LowCode
  * 低代码核心项目, 包含核心服务和领域模型
  * Common：元数据 Schema (MetaSchema)、组件基类、默认组件库、实体与应用契约等公共部分
  * DesignEngine：设计引擎，负责页面/组件的可视化设计，产出应用元数据；包含 Workbench、MyApp 等设计端应用
  * RenderEngine：渲染引擎，根据元数据动态渲染出可运行的应用；主题基于 Ant Design Blazor
  * meta：应用与组件的元数据 (JSON) 存放目录
  * 元数据仓储支持 JsonFile / EntityFrameworkCore / RemoteService 多种实现

* Services 基础服务（企业级应用）
  * 按限界上下文划分的业务模块，每个模块遵循 Application.Contracts / Application / EntityFrameworkCore / Web 分层
  * 包含：Account、Organization、Approval、Assistant、Notification、Order、Portal、Setting、SupplyChain、BackgroundTask、Testing

* System 系统级应用
  * Enterprise、SystemPortal，面向平台运营侧
  * 与 Services 下的企业级应用严格隔离

* Tools 数据库迁移工具
  * 各服务对应的 DbMigrator 控制台程序，用于创建/更新数据库结构

* Utils 工具类库
  * H.Abp.Application.Contracts：应用服务基础契约 (IAppService 等)
  * H.Abp.HttpClientProxy：基于 IAppService 接口的 HTTP 动态代理
  * H.Util.Blazor / H.Util.Ids / H.Util.Base：Blazor 工具、ID 生成、基础工具与统一返回包装等

### 本地开发
#### 环境准备
* 基础安装
  * .NET SDK (版本见 src/global.json)
  * WSl + Docker (Engine+Compose) + Portainer
* 启动依赖服务 (Redis、RabbitMQ 等)
  * 将 cd 目录下的 'docker-compose.yml' 文件拷贝到 WSL
  * 运行依赖服务：`sudo docker-compose up -d`

#### 项目启动
* 数据库迁移
通过 Tools 目录下对应的 DbMigrator 创建数据库结构，如：`dotnet run --project src/Tools/H.Account.DbMigrator`

* 启动 Docker：`sudo service docker start`

* 启动主程序：`H.AppLab.Host.All`
  * 命令行方式：`dotnet run --project src/Host/H.AppLab.Host.All/H.AppLab.Host.All --launch-profile HostAll`
  * 默认地址：`http://localhost:5045` (https: 7065)

### 技术特性
#### 应用抽屉
* 由 H.AppDrawer.Components / H.AppDrawer.Model 提供的独立可复用组件，统一各应用的整体布局（顶部导航栏、侧边菜单、应用抽屉面板）
* 应用以元数据方式接入：通过 AppCategoryInfo / AppItemInfo 配置应用分类、图标、跳转地址与打开方式，实现跨应用的快速切换
* 各宿主只需引用组件并提供应用/菜单数据，即可获得一致的导航体验，无需重复实现布局

#### 前端基于 IAppService 动态调用 http
* 由 H.Abp.HttpClientProxy 实现：前端只依赖 Application.Contracts 中的 IAppService 接口，无需手写任何 HttpClient 调用代码
* 基于 DispatchProxy 拦截接口方法调用，按 ABP 路由约定 (AbpUrlConvention) 将 "接口名 + 方法名" 转换为 HTTP 请求（GetXxx → GET、CreateXxx → POST 等），复杂参数自动序列化为请求体
* 通过 `AddHttpClientProxies` 扫描程序集内所有 IAppService 接口并批量注册代理；远程服务地址由配置文件 "RemoteServices" 节点统一管理
* 同一套接口在服务端为进程内实现、在 WebAssembly 客户端为 HTTP 代理实现，业务代码无感知

#### WebAssembly 懒加载
* 首页仅加载必要程序集（Portal、AppDrawer 及少量契约/工具库），其余各应用的 Contracts、LowCode 库及第三方依赖 (如 Markdig、Sqids) 在导航到对应路由时才按需下载
* 在客户端 csproj 中通过 `BlazorWebAssemblyLazyLoad` 显式声明懒加载程序集，并在 Routes.razor 的 `OnNavigateAsync` 中按路由触发程序集加载
* 自研组合式 DI 支撑懒加载后的服务注册：LazyModuleRegistry 管理各路由模块的服务容器，CompositeServiceProvider 提供 "根容器 → 模块容器" 的服务解析回退，实现程序集加载后的延迟服务注册
* Release 模式启用 AOT 与裁剪 (Trimming)，进一步减少下载体积；Debug 模式会额外加载 .pdb 以支持调试
