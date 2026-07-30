# SystemManagement 模块重命名为 Notification

## Context

将 `SystemManagement` 应用模块重命名为 `Notification`，涵盖目录、文件、命名空间、类名、连接字符串及所有跨模块引用。数据库名称一并从 `SystemManagementDb` 改为 `NotificationDb`。

影响范围：5 个项目目录 + 1 个 DbMigrator 工具目录，约 30 个文件需修改内容，14 个文件需重命名，9 个跨模块文件需更新引用。

---

## Task 1: 修改模块内所有文件内容（路径不变）

按依赖从底层到上层顺序，将所有文件中的 `SystemManagement` 替换为 `Notification`（命名空间、类名、连接字符串、ProjectReference 路径等）。

### 1.1 Application.Contracts（7 个文件）
- `.csproj` — RootNamespace
- `SystemManagementApplicationContractsModule.cs` — namespace + class name
- `Dtos/NotificationDtos.cs`、`Dtos/SettingDtos.cs`、`Enums/NotificationMethodType.cs`、`Services/INotificationAppService.cs`、`Services/ISettingAppService.cs` — namespace

### 1.2 EntityFrameworkCore（4 个文件）
- `.csproj` — RootNamespace + ProjectReference 路径
- `SystemManagementEntityFrameworkCoreModule.cs` — namespace + class name + `"SystemManagementDb"` → `"NotificationDb"`
- `SystemManagementDbContext.cs` — namespace + class name + ConnectionStringName
- `Entities/NotificationEntities.cs` — namespace

### 1.3 Application（5 个文件）
- `.csproj` — RootNamespace + 2 个 ProjectReference 路径
- `SystemManagementApplicationModule.cs` — using/namespace/class name/DependsOn/AddMaps
- `Mapping/SystemManagementProfile.cs` — using/namespace/class name
- `Services/NotificationAppService.cs` — using/namespace
- `Services/SettingAppService.cs` — using/namespace

### 1.4 Web（7 个文件，SystemLogin.razor 不改）
- `.csproj` — ProjectReference 路径
- `SystemManagementWebModule.cs` — namespace + class name
- `_Imports.razor` — 2 处 @using
- `Layout/SystemManagementLayout.razor` — @namespace
- `Layout/SystemLoginLayout.razor` — @namespace
- `Pages/NotificationManagement.razor` — @layout 引用
- `Pages/Settings.razor` — @layout 引用

### 1.5 DbMigrator（7 个文件）
- `.csproj` — ProjectReference 路径
- `Program.cs` — using/namespace/DbContext 类名/连接字符串键
- `SystemManagementDbContextFactory.cs` — using/namespace/class name/泛型参数
- `appsettings.json` — 连接字符串键 + 数据库名
- `Migrations/20260517142122_Init.cs` — namespace
- `Migrations/20260517142122_Init.Designer.cs` — namespace + using + typeof + 实体命名空间字符串
- `Migrations/SystemManagementDbContextModelSnapshot.cs` — namespace + using + class name + typeof + 实体命名空间字符串

## Task 2: 修改跨模块引用文件

### 2.1 Host Server（4 个文件）
- `H.AppLab.Host.All.csproj` — 注释 + 2 个 ProjectReference 路径
- `HostAllModule.cs` — using + 注释 + typeof ×2
- `Program.cs` — typeof 程序集引用
- `appsettings.json` — 连接字符串键/数据库名 + 远程服务键

### 2.2 Host Client（4 个文件）
- `H.AppLab.Host.All.Client.csproj` — LazyLoad dll 名 + 注释 + ProjectReference 路径
- `HostAllClientModule.cs` — using + 常量名/值 + typeof
- `Routes.razor` — dll 程序集名
- `wwwroot/appsettings.json` — 远程服务键

### 2.3 解决方案文件
- `AppLab.slnx` — Folder Name + 5 个 Project Path

## Task 3: 重命名文件（14 个）

使用 `git mv` 保留 git 历史：
- 5 个 `.csproj` 文件
- 4 个 Module 类文件（ApplicationModule、ContractsModule、EFCoreModule、WebModule）
- `SystemManagementDbContext.cs` → `NotificationDbContext.cs`
- `SystemManagementProfile.cs` → `NotificationProfile.cs`
- `SystemManagementDbContextFactory.cs` → `NotificationDbContextFactory.cs`
- `SystemManagementDbContextModelSnapshot.cs` → `NotificationDbContextModelSnapshot.cs`
- `SystemManagementLayout.razor` → `NotificationLayout.razor`

## Task 4: 重命名目录（6 个）

使用 `git mv`：
1. 4 个 Services 子项目目录
2. 1 个 Tools DbMigrator 目录
3. 最后重命名父目录 `Services/SystemManagement` → `Services/Notification`

## Task 5: 清理与验证

- 删除所有受影响项目的 `bin/` 和 `obj/` 构建缓存
- `dotnet restore` + `dotnet build` 验证编译通过
- 全局搜索 `SystemManagement` 确认无残留引用
