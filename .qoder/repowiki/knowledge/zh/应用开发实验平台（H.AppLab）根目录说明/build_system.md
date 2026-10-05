## 1. 使用的系统与工具

- **SDK**: .NET SDK `10.0.0`，通过 `src/global.json` 锁定并启用 `rollForward=latestFeature`。
- **目标框架**: 全部项目统一 `net10.0`，由共享属性文件 `src/common.props` 集中声明。
- **语言特性**: `LangVersion=preview`、`Nullable=enable`、`ImplicitUsings=enable`。
- **解决方案管理**: 使用 `.slnx`（`.sln` 的扩展/替代格式），入口为 `src/AppLab.slnx`；同时提供 `src/AppLab.slnLaunch`。
- **包版本管理**: 使用 MSBuild Central Package Management（`Directory.Packages.props` + `<ManagePackageVersionsCentrally>true</ManagePackageVersionsCentrally>`）。
- **NuGet 源**: 仅 `nuget.org`，在 `src/NuGet.config` 中显式配置。
- **桌面端打包**: Avalonia (`H.AppLab.Desktop`)，不依赖外部打包脚本。
- **容器编排**: `cd/docker-compose.yml` 仅提供 Redis / RabbitMQ / MinIO 等运行时依赖，不包含应用镜像构建。
- **CI/发布流水线**: 仓库中未发现 GitHub Actions、Azure Pipelines、Jenkinsfile 等 CI 配置文件。

## 2. 关键文件

| 文件 | 作用 |
|---|---|
| `src/AppLab.slnx` | 解决方案清单，按 `Agent/`、`Components/`、`Host/`、`LowCode/`、`Services/`、`System/`、`Tools/`、`Utils/` 八个逻辑域组织所有 `.csproj` |
| `src/global.json` | 锁定 .NET SDK 10.0.0 |
| `src/common.props` | 全局 MSBuild 属性：TargetFramework、LangVersion、Nullable、默认 Version=`0.7.0`、Authors/Company/Description、SatelliteResourceLanguages=`zh-Hans`、`GeneratePackageOnBuild=False` |
| `src/Directory.Packages.props` | 所有第三方 NuGet 包的中央版本定义（ABP、EF Core、Avalonia、Serilog、CAP、Hangfire、Elsa、Playwright 等） |
| `src/nuget.pack.props` | 可选的打包增强：`GeneratePackageOnBuild=true`、本地复制 nupkg、条件推送到 nuget.org |
| `src/NuGet.config` | 唯一 NuGet 源 `nuget.org` |
| `cd/docker-compose.yml` | 开发环境依赖（Redis/RabbitMQ/MinIO），非应用构建产物 |

## 3. 架构与约定

### 3.1 解决方案拓扑
`solution(AppLab.slnx)` 是唯一的构建入口。每个业务域遵循 ABP 风格分层：
- `Application.Contracts` / `Application` / `EntityFrameworkCore` / `Web`（部分域还包含 `Client`）
- `Tools/*DbMigrator` 作为各域的独立迁移可执行项目
- `Utils/` 提供跨域基础库（`H.Util.Base`、`H.Util.Blazor`、`H.Util.Ids`、`H.Abp.*`）
- `Host/` 承载宿主进程（Web Host + Avalonia Desktop）

### 3.2 版本策略
- 应用二进制版本来自 `common.props` 中的 `<Version>0.7.0</Version>`。
- 包版本来自 `Directory.Packages.props`，单个 csproj 不再声明版本号。
- 新增包时应添加到 `Directory.Packages.props` 而非 csproj。

### 3.3 打包与分发
- 默认构建不会自动打 nupkg（`common.props` 中 `GeneratePackageOnBuild=False`）。
- 若需要打 nupkg，引入 `nuget.pack.props`（`<ProjectReference Include="../nuget.pack.props"/>` 或 Implicit Usings 已覆盖），它将：
  - 开启 `GeneratePackageOnBuild=true`
  - 当环境变量 `NUGET_LOCAL_PATH` 存在时，把生成的 `.nupkg` 复制到该目录
  - 当环境变量 `NUGET_PUSH=true` 且 `NUGET_API_KEY` 存在时，调用 `dotnet nuget push ... -k $(NUGET_API_KEY) -s https://api.nuget.org/v3/index.json --skip-duplicate`

### 3.4 运行时代码生成
- `src/AppLab.slnx` 被标记为 `<Solution>` 根元素，属于 Visual Studio 2022+ 支持的 `.slnx` 方案格式，用于在 IDE 中聚合所有项目而无需传统 `.sln` 转换。

## 4. 约定与约束

- **SDK 固定**：所有开发者必须安装 .NET SDK 10.0.0（`global.json` 强制，`rollForward=latestFeature` 允许补丁升级）。
- **统一框架**：新增项目应继承 `common.props` 的 `net10.0`、`LangVersion=preview`、`Nullable=enable`，不要在各 csproj 中重复声明。
- **包版本集中化**：所有第三方依赖的版本必须在 `Directory.Packages.props` 中声明，禁止在 csproj 中写死版本号。
- **NuGet 源限制**：`NuGet.config` 使用 `<clear/>` 清空默认源后只保留 `nuget.org`，因此私有源需修改该文件。
- **默认不打包**：常规 `dotnet build` 不会产出 nupkg；需要包输出时必须引入 `nuget.pack.props` 或通过 `dotnet pack` 显式触发。
- **发布到 nuget.org**：仅在 `NUGET_PUSH=true` 且 `NUGET_API_KEY` 已设置时才会推送，避免误操作。
- **本地开发依赖**：`cd/docker-compose.yml` 启动 Redis/RabbitMQ/MinIO，但应用本身未在此文件中以服务形式声明，部署流程未在仓库中体现。
- **无内置测试任务**：未发现 test project 集合或 dotnet test 相关 target，测试运行方式未在构建系统中声明。
- **无 Dockerfile / CI 文件**：仓库没有容器镜像构建脚本和持续集成流水线配置，部署步骤不在本仓库内。