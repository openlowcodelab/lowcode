## 1. 使用的系统与方案

仓库是一个 .NET 多项目解决方案（包含大量 `*.csproj`），依赖管理完全基于 **NuGet**，并采用以下机制：
- **Central Package Management (CPM)**：通过 `src/Directory.Packages.props` 集中声明所有第三方包的版本，子项目仅引用包名而不写版本号。
- **SDK 锁定**：`src/global.json` 固定 SDK 为 `10.0.0`，`rollForward=latestFeature`。
- **全局公共属性**：`src/common.props` 统一 `TargetFramework=net10.0`、`LangVersion=preview`、`Nullable=enable`、`Version=0.7.0` 等构建属性。
- **私有源配置**：`src/NuGet.config` 仅启用 `nuget.org` 一个源，无公司私有 feed。
- **打包与发布脚本**：`src/nuget.pack.props` 在 `Pack` 之后增加两个 MSBuild Target：按环境变量 `NUGET_LOCAL_PATH` 复制 nupkg 到本地目录，或按 `NUGET_PUSH=true` + `NUGET_API_KEY` 推送到 `https://api.nuget.org/v3/index.json`。

没有发现前端依赖清单（无 `package.json` / `yarn.lock`），也没有 vendored 的前端库——前端资产以 Blazor 组件 `.razor` 及 `wwwroot` 形式内联在项目中。

## 2. 关键文件

| 文件 | 作用 |
|---|---|
| `src/Directory.Packages.props` | CPM 中央版本表，声明全部第三方包及其精确版本 |
| `src/NuGet.config` | NuGet 源列表（仅 nuget.org） |
| `src/global.json` | 锁定 .NET SDK 版本 |
| `src/common.props` | 全解决方案共享的 MSBuild 公共属性 |
| `src/nuget.pack.props` | 自定义 Pack/Push 目标 |
| `src/AppLab.slnx` | 解决方案定义（MSBuild 通过它聚合各 `*.csproj`） |

## 3. 架构与约定

- **版本集中化**：`Directory.Packages.props` 中每个 `<PackageVersion Include="..." Version="..." />` 对应一个 NuGet 包。新增依赖时在此添加一行，而不是在每个 `*.csproj` 里写版本号。该文件已覆盖三大类依赖：Microsoft/.NET 官方包、Volo.Abp 全家桶、Avalonia 桌面 UI、Elsa/DotNetCore.CAP/Hangfire/Serilog 等业务库。
- **框架对齐**：所有项目统一 target `net10.0`（见 `common.props`），与 EF Core 10、ASP.NET Core 10、Blazor WebAssembly 10 保持版本一致；ABP 使用 10.x 系列，Avalonia 使用 12.x。
- **无锁文件**：仓库未提交 `packages.lock.json`，因此复现构建依赖的是 CPM 中的精确版本，而非 NuGet 解析后的完整依赖树快照。
- **无私有源**：`NuGet.config` 使用 `<clear/>` 后只注册 `nuget.org`，未配置任何企业私有 feed、代理或 `GOPRIVATE` 等价物。
- **自产包分发**：内部 Utils 包（如 `H.Abp.Application.Contracts`、`H.Util.Base`、`H.Util.Blazor`、`H.Util.Ids` 等）通过 `nuget.pack.props` 的 `GeneratePackageOnBuild` 生成 nupkg，并可由 CI 推送至 nuget.org，或由开发机通过 `NUGET_LOCAL_PATH` 输出到本地文件夹。

## 4. 约定与约束

- **CPM 强制生效**：`Directory.Packages.props` 首行设置 `<ManagePackageVersionsCentrally>true</ManagePackageVersionsCentrally>`，这是 MSBuild 内置开关，使所有子项目必须从该文件继承版本。
- **SDK 版本被锁定**：`global.json` 要求 `dotnet` SDK `10.0.0`，CI 或本地环境若安装了其他主版本的 SDK 将无法直接构建。
- **统一框架与语言**：`common.props` 将 `TargetFramework` 固定为 `net10.0`、`LangVersion` 为 `preview`、`Nullable` 为 `enable`，子项目无需重复声明。
- **版本统一策略**：同生态包尽量保持一致主版本（EF Core 10、ASP.NET Core 10、ABP 10.x、Serilog 10.x），但 ABP 不同子包存在 `10.5.0` / `10.6.0` / `10.3.0` 等差异，说明版本同步并非完全自动化，需要人工对齐。
- **NuGet 源唯一性**：`NuGet.config` 显式 `<clear/>` 后仅保留 `nuget.org`，意味着不允许通过 `~/.config/NuGet/NuGet.Config` 或其他位置注入额外源来绕过此限制（除非在更高层级覆盖）。
- **包发布条件**：只有当环境变量 `NUGET_PUSH=true` 且 `NUGET_API_KEY` 已设置时，才会执行 `dotnet nuget push` 到官方源；仅设置 `NUGET_LOCAL_PATH` 则只复制到本地目录，不上传。