# 企业管理 (Enterprise Management) 实施计划

## Context

项目需要新增"企业管理"功能，实现多租户系统。企业对应多租户中的租户概念，支持企业注册、激活（设置共用/独立数据库）、启用/禁用等操作。同时需要与现有登录流程和其他服务（Organization、Approval、AutoTest）集成，实现数据隔离。

基于 ABP 多租户架构 (`Volo.Abp.MultiTenancy`)，通过 TenantId 实现数据过滤隔离。

---

## Task 1: 基础设施准备

### 1.1 添加 NuGet 包版本声明
**文件**: `src/Directory.Packages.props`
- 添加 `Volo.Abp.MultiTenancy` 版本 `10.4.1`

### 1.2 创建 Enterprise 服务目录结构
```
src/Services/Enterprise/
  H.Enterprise.EntityFrameworkCore/
  H.Enterprise.Application.Contracts/
  H.Enterprise.Application/
  H.Enterprise.Web/
```

### 1.3 在解决方案中注册新项目
**文件**: `src/H.LowCode.slnx`
- 添加 `/Services/Enterprise/` 目录及 4 个项目引用
- 添加 `/Tools/` 下的 DbMigrator 项目引用

---

## Task 2: EntityFrameworkCore 层

### 2.1 项目文件
**新建**: `src/Services/Enterprise/H.Enterprise.EntityFrameworkCore/H.Enterprise.EntityFrameworkCore.csproj`
- 引用: `Volo.Abp.EntityFrameworkCore`, `Volo.Abp.EntityFrameworkCore.SqlServer`, `Volo.Abp.MultiTenancy`

### 2.2 EnterpriseEntity 实体
**新建**: `src/Services/Enterprise/H.Enterprise.EntityFrameworkCore/Entities/EnterpriseEntity.cs`

| 属性 | 类型 | 说明 |
|------|------|------|
| Id | Guid | 企业ID (=TenantId) |
| Name | string | 企业名称 (Required, Max 200) |
| Code | string? | 企业编码 (Max 50, Unique) |
| Description | string? | 描述 (Max 1000) |
| Logo | string? | Logo URL (Max 500) |
| ContactName | string? | 联系人 (Max 100) |
| ContactPhone | string? | 联系电话 (Max 20) |
| ContactEmail | string? | 联系邮箱 (Max 100) |
| Status | EnterpriseStatus | 状态: Pending/Active/Disabled |
| DatabaseMode | DatabaseMode | Shared/Independent (激活后不可改) |
| ConnectionString | string? | 独立数据库连接串 (Max 1000) |
| IsActivated | bool | 是否已激活 |
| ActivatedAt | DateTime? | 激活时间 |
| ActivatedBy | Guid? | 激活人 |
| CreatedAt/UpdatedAt/CreatedBy/UpdatedBy/Remark | - | 审计字段 |

### 2.3 EnterpriseUserEntity 实体
**新建**: `src/Services/Enterprise/H.Enterprise.EntityFrameworkCore/Entities/EnterpriseUserEntity.cs`

| 属性 | 类型 | 说明 |
|------|------|------|
| Id | Guid | 关联ID |
| EnterpriseId | Guid | 企业ID (FK) |
| UserId | Guid | 用户ID |
| UserName | string | 用户名 (冗余) |
| Role | string | 角色: Owner/Admin/Member |
| IsDefault | bool | 是否默认企业 |
| JoinedAt/CreatedAt/CreatedBy | - | 审计字段 |

唯一约束: `(EnterpriseId, UserId)`

### 2.4 EnterpriseDbContext
**新建**: `src/Services/Enterprise/H.Enterprise.EntityFrameworkCore/EnterpriseDbContext.cs`
- 继承 `DbContext`（遵循 Organization 模式）
- 表名: `Enterprise_Enterprises`, `Enterprise_EnterpriseUsers`
- 注入 `ICurrentTenant`，在 `SaveChangesAsync` 中自动填充 TenantId（不适用于 Enterprise 表本身）
- Enterprise 和 EnterpriseUser 表不应用租户过滤（它们是跨租户的）

### 2.5 EF Core 模块
**新建**: `src/Services/Enterprise/H.Enterprise.EntityFrameworkCore/EnterpriseEntityFrameworkCoreModule.cs`
- `[DependsOn(typeof(AbpEntityFrameworkCoreModule))]`
- 连接字符串名: `EnterpriseDb`

### 2.6 EnterpriseTenantStore (ITenantStore 实现)
**新建**: `src/Services/Enterprise/H.Enterprise.EntityFrameworkCore/EnterpriseTenantStore.cs`
- 实现 `ITenantStore` 接口
- 查询 EnterpriseDbContext 中的激活企业
- 对于 Independent 模式返回自定义 ConnectionString
- 对于 Shared 模式返回 null（使用默认数据库）
- 查询时使用 Host 上下文（不受租户过滤）

---

## Task 3: Application.Contracts 层

### 3.1 项目文件
**新建**: `src/Services/Enterprise/H.Enterprise.Application.Contracts/H.Enterprise.Application.Contracts.csproj`

### 3.2 枚举
**新建**: `Enums/EnterpriseStatus.cs` (Pending=0, Active=1, Disabled=2)
**新建**: `Enums/DatabaseMode.cs` (Shared=0, Independent=1)

### 3.3 DTO
**新建**: `Dtos/EnterpriseDto.cs` — EnterpriseDto, CreateEnterpriseDto, UpdateEnterpriseDto, EnterpriseQueryParams, ActivateEnterpriseDto
**新建**: `Dtos/EnterpriseUserDto.cs` — EnterpriseUserDto, AddEnterpriseUserDto
- 复用现有 `PagedResult<T>` 模式

### 3.4 服务接口
**新建**: `Services/IEnterpriseService.cs`
```
GetListAsync, GetByIdAsync, CreateAsync, UpdateAsync, DeleteAsync,
ActivateAsync(Guid id, DatabaseMode), EnableAsync, DisableAsync,
GetMyEnterprisesAsync(Guid userId), SelectEnterpriseAsync(Guid enterpriseId),
GetCurrentEnterpriseAsync()
```

**新建**: `Services/IEnterpriseUserService.cs`
```
GetEnterpriseUsersAsync, AddUserAsync, RemoveUserAsync,
SetDefaultEnterpriseAsync, UpdateUserRoleAsync
```

### 3.5 模块类
**新建**: `EnterpriseApplicationContractsModule.cs`（空模块）

---

## Task 4: Application 层

### 4.1 项目文件
**新建**: `src/Services/Enterprise/H.Enterprise.Application/H.Enterprise.Application.csproj`
- 引用 Contracts + EntityFrameworkCore + Account.Application.Contracts

### 4.2 EnterpriseService 实现
**新建**: `Services/EnterpriseService.cs`

关键实现逻辑:
- **CreateAsync**: 创建企业(Status=Pending)，创建者自动成为 Owner
- **ActivateAsync**: 超管操作，设置 DatabaseMode，若 Independent 则创建数据库+迁移+存储连接串
- **SelectEnterpriseAsync**: 验证用户归属 → 读取现有 Cookie Claims → 追加 TenantId/EnterpriseName Claim → 重新 SignIn 更新 Cookie
- **GetMyEnterprisesAsync**: 跨租户查询 EnterpriseUser 关联
- **GetCurrentEnterpriseAsync**: 从 Cookie Claims 中读取当前 EnterpriseId

### 4.3 EnterpriseUserService 实现
**新建**: `Services/EnterpriseUserService.cs` — 标准 CRUD

### 4.4 模块类
**新建**: `EnterpriseApplicationModule.cs`
- `[DependsOn(ContractsModule, EfCoreModule)]`
- 注册 IEnterpriseService, IEnterpriseUserService

---

## Task 5: Web (Blazor) 层

### 5.1 项目文件
**新建**: `src/Services/Enterprise/H.Enterprise.Web/H.Enterprise.Web.csproj`
- SDK: `Microsoft.NET.Sdk.Razor`
- 引用: AppDrawer.Components, Contracts, H.Util.Blazor

### 5.2 布局
**新建**: `Layout/EnterpriseLayout.razor`
- 使用 DefaultLayoutComponent + 侧边栏菜单（企业列表、创建企业）

### 5.3 页面
| 文件 | 路由 | 功能 |
|------|------|------|
| `Pages/EnterpriseList.razor` | `/enterprise` | 企业列表(搜索+分页+状态筛选) |
| `Pages/EnterpriseCreate.razor` | `/enterprise/create` | 创建企业表单 |
| `Pages/EnterpriseDetail.razor` | `/enterprise/{id:guid}` | 企业详情+编辑+成员管理+激活操作 |
| `Pages/EnterpriseSelect.razor` | `/enterprise/select` | 登录后企业选择页(0→引导创建/1→自动选/多→手动选) |

### 5.4 Razor imports
**新建**: `_Imports.razor`, `EnterpriseWebModule.cs`（空模块）

---

## Task 6: Host 配置集成

### 6.1 HostAllModule 修改
**修改**: `src/Host/H.LowCode.Host.All/H.LowCode.Host.All/HostAllModule.cs`
- `using H.Enterprise.Application;`
- `[DependsOn]` 追加 `typeof(EnterpriseApplicationModule)`
- `ConfigureAutoApiControllers` 追加 Enterprise 程序集的控制器注册
- 新增多租户配置: `Configure<AbpMultiTenancyOptions>(o => o.IsEnabled = true)`
- 注册 `ITenantStore` → `EnterpriseTenantStore`
- 注册自定义 `TenantResolveContributor`（从 Cookie Claims 读取 TenantId）

### 6.2 Host csproj 追加引用
**修改**: `src/Host/H.LowCode.Host.All/H.LowCode.Host.All/H.LowCode.Host.All.csproj`
- 追加 `H.Enterprise.Application` 项目引用
- 追加 `Volo.Abp.MultiTenancy` 包引用

### 6.3 appsettings.json 追加配置
**修改**: `src/Host/H.LowCode.Host.All/H.LowCode.Host.All/appsettings.json`
- ConnectionStrings 追加 `"EnterpriseDb"`
- RemoteServices 追加 `"Enterprise": { "BaseUrl": "https://localhost:7065" }`

### 6.4 Program.cs 追加 Razor 组件程序集
**修改**: `src/Host/H.LowCode.Host.All/H.LowCode.Host.All/Program.cs`
- `AddAdditionalAssemblies` 追加 `typeof(H.Enterprise.Web._Imports).Assembly`

### 6.5 HostAllClientModule 修改
**修改**: `src/Host/H.LowCode.Host.All/H.LowCode.Host.All.Client/HostAllClientModule.cs`
- 追加 `EnterpriseRemoteServiceName = "Enterprise"`
- `ConfigureHttpClientProxies` 追加 Enterprise Contracts 代理注册

### 6.6 Client csproj 追加引用和懒加载
**修改**: `src/Host/H.LowCode.Host.All/H.LowCode.Host.All.Client/H.LowCode.Host.All.Client.csproj`
- 追加 `H.Enterprise.Web` 项目引用
- 追加 `BlazorWebAssemblyLazyLoad` 项 `H.Enterprise.Web.dll`

### 6.7 Routes.razor 追加懒加载映射
**修改**: `src/Host/H.LowCode.Host.All/H.LowCode.Host.All.Client/Routes.razor`
- `LazyAssemblies` 追加 `["enterprise"] = ["H.Enterprise.Web.dll"]`

---

## Task 7: 登录流程改造

### 7.1 Login.razor 修改
**修改**: `src/Services/Account/H.Account.Web/Pages/Login.razor`
- 登录成功后跳转到 `/enterprise/select`（而非直接跳转 returnUrl）

### 7.2 DefaultLayoutComponent 修改
**修改**: `src/Components/AppDrawer/H.AppDrawer.Components/Components/DefaultLayoutComponent.razor`
- 认证通过后检查 Cookie 中是否有 TenantId claim
- 若无 TenantId 且不在 `/enterprise/select` 页面 → 重定向到 `/enterprise/select`
- 从 Claims 读取企业名称传递给 TopNavbar

### 7.3 TopNavbar 修改
**修改**: `src/Components/AppDrawer/H.AppDrawer.Components/Components/TopNavbar.razor`
- 新增 `EnterpriseName` 参数
- 在用户下拉菜单中显示当前企业名称和"切换企业"入口

### 7.4 AccountAppService 修改
**修改**: `src/Services/Account/H.Account.Application/Services/AccountAppService.cs`
- `LoginAsync` 方法使用 `ICurrentTenant.Change(null)` 包裹（跨租户查找用户）
- Account EF Core csproj 追加 `Volo.Abp.MultiTenancy` 包引用

---

## Task 8: 其他服务数据隔离

### 8.1 Organization 服务
**修改文件**:
- `Entities/OrganizationEntity.cs`, `MemberEntity.cs`, `RoleEntity.cs` — 追加 `Guid? TenantId` 属性
- `OrganizationDbContext.cs` — 在 `SaveChangesAsync` 中通过 `ICurrentTenant` 自动填充 TenantId；在查询中添加全局过滤
- `OrganizationEntityFrameworkCoreModule.cs` — 追加 `AbpMultiTenancyModule` 依赖
- `H.Organization.EntityFrameworkCore.csproj` — 追加 `Volo.Abp.MultiTenancy` 包
- 所有 Service 类 — 查询时使用 TenantId 过滤（或改为 AbpDbContext 自动过滤）

**注意**: OrganizationDbContext 当前继承 `DbContext`（非 AbpDbContext），ABP 自动过滤不生效。两种方案:
- **方案A**: 改继承 `AbpDbContext<T>`，实体实现 `IMultiTenant` → 自动过滤
- **方案B**: 保持 `DbContext`，手动在 SaveChanges + 查询中添加过滤

推荐方案A，与 Approval 的 AbpDbContext 模式一致。

### 8.2 Approval 服务
**修改文件**:
- `Entities/ApprovalDefinition.cs`, `ApprovalInstance.cs`, `ApprovalTask.cs` — 追加 `Guid? TenantId`，实现 `IMultiTenant`
- `H.Approval.EntityFrameworkCore.csproj` — 追加 `Volo.Abp.MultiTenancy` 包
- ApprovalDbContext 已继承 `AbpDbContext<T>`，添加 IMultiTenant 后自动过滤生效

### 8.3 AutoTest 服务
**修改**: AutoTest 使用 JSON 文件存储，修改文件路径加入租户目录层级
- 所有 AppService 中注入 `ICurrentTenant`
- 数据路径改为: `Path.Combine(_dataPath, tenantId?.ToString() ?? "host", "projects.json")`

---

## Task 9: DbMigrator 工具

**新建**: `src/Tools/H.Enterprise.DbMigrator/`
- `H.Enterprise.DbMigrator.csproj`
- `EnterpriseDbContextFactory.cs` (IDesignTimeDbContextFactory)
- `Program.cs` (迁移入口)
- `appsettings.json` (连接字符串)

执行首次迁移: `dotnet ef migrations add Initial --project H.Enterprise.DbMigrator`

---

## 实施顺序

```
Task 1 (基础设施) → Task 2 (EF Core) → Task 3 (Contracts) → Task 4 (Application)
→ Task 5 (Web) → Task 6 (Host) → Task 7 (登录改造) → Task 8 (数据隔离) → Task 9 (DbMigrator)
→ 验证: 编译通过 + 迁移执行 + 登录流程 + 企业选择 + 数据隔离
```

## 验证方案

1. `dotnet build` 整个解决方案编译通过
2. 运行 DbMigrator 创建 EnterpriseDb 数据库和表
3. 启动 Host.All，登录后验证企业选择流程
4. 创建企业 → 超管激活（共用数据库模式） → 验证数据隔离
5. 创建企业 → 超管激活（独立数据库模式） → 验证独立数据库创建和迁移
6. TopNavbar 显示当前企业 + 切换企业入口
7. Organization/Approval 数据按 EnterpriseId 隔离

## 风险与注意事项

- **OrganizationDbContext 基类变更**: 从 DbContext 改为 AbpDbContext 可能影响现有查询，需充分测试
- **Cookie 重签发**: SelectEnterpriseAsync 先 SignOut 再 SignIn，确保原子性
- **独立数据库创建**: 高风险操作，需事务包裹和失败回滚
- **存量数据**: 现有用户无企业关联，首次登录需引导创建
- **ABP Identity 多租户**: 启用后 IdentityUser 自动按 TenantId 过滤，登录时必须用 `ICurrentTenant.Change(null)` 绕过
