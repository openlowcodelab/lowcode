在 src\\Services\\Account 目录创建一个账号注册与认证服务，包含用户的注册、登录、JWT 认证、用户管理等功能。同时需提供 sdk 供业务系统快速接入。

技术要求：
1、基于 ASP.NET Core Minimal API、Blazor 等技术
2、非必要不引入第三方组件，也不要使用第三方框架
3、关于认证，如果容易实现则自行实现，如果需要可考虑引入 OpenIddict

项目分层：
H.Account.HttpApi：api 接口项目，提供给 web 调用
H.Account.Application：业务逻辑
H.Account.Application.Contracts：对象、接口定义
H.Account.Domain：实体定义
H.Account.EntityFrameworkCore
H.Account.Web：页面，如注册、登录、用户管理等
H.Account.Client：提供给外部应用对接的 SDK

## 功能模块

### 1. 认证功能
- 用户注册
- 用户登录（JWT 令牌）
- 令牌验证

### 2. 用户管理
- 用户列表（分页、搜索、筛选）
- 创建用户
- 编辑用户
- 删除用户
- 禁用/启用用户
- 重置密码
- 用户类型管理（普通用户、管理员、超级管理员）

### 3. 用户实体字段
- 基本信息：用户名、邮箱、手机号、密码
- 用户类型：UserType（Normal、Admin、SuperAdmin）
- 角色：Roles（逗号分隔）
- 状态：IsActive（是否激活）
- 验证状态：EmailConfirmed、PhoneNumberConfirmed
- 锁定信息：LockoutEnd、AccessFailedCount
- 审计信息：CreatedAt、UpdatedAt、CreatedBy、UpdatedBy、LastLoginAt
- 备注：Remark

### 4. API 接口

#### 认证接口
- POST /api/auth/register - 用户注册
- POST /api/auth/login - 用户登录
- GET /api/auth/validate - 验证令牌

#### 用户管理接口（需要 Admin 权限）
- GET /api/users - 获取用户列表（分页）
- GET /api/users/{id} - 获取用户详情
- POST /api/users - 创建用户
- PUT /api/users/{id} - 更新用户
- PATCH /api/users/{id}/status - 更新用户状态
- POST /api/users/{id}/reset-password - 重置密码
- DELETE /api/users/{id} - 删除用户
- GET /api/users/check-username - 检查用户名是否存在
- GET /api/users/check-email - 检查邮箱是否存在

### 5. 权限控制
- Admin 策略：需要 UserType 为 Admin 或 SuperAdmin
- SuperAdmin 策略：需要 UserType 为 SuperAdmin

## 服务注册

### 在 HttpApi 项目中注册服务

```csharp
using H.Account.Application;
using H.Account.EntityFrameworkCore;
using H.Account.HttpApi;

var builder = WebApplication.CreateBuilder(args);

// 配置 Account 模块
builder.Services.AddAccountDbContext(builder.Configuration);
builder.Services.AddAccountApplication();
builder.Services.AddAccountHttpApi(builder.Configuration);

var app = builder.Build();

app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.Run();
```

### 在 Client 项目中注册 SDK

```csharp
using H.Account.Client;

// 注册 Account SDK
services.AddAccountClient("https://localhost:5179");

// 或使用命名 HttpClient
services.AddAccountClient("AccountApi", "https://localhost:5179");
```
