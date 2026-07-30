# 微信/钉钉扫码登录实现

## Context

当前登录页面已有微信/钉钉按钮和基础 OAuth 配置（`AccountHostModule.ConfigureExternalLogin`），但存在以下问题：
1. 内置 `AddOAuth` handler 不适用于微信开放平台扫码（需要 `#wechat_redirect` fragment）和钉钉（非标准 token 交换流程，需 HmacSHA256 签名）
2. `/api/account/external-login` 端点不存在（无对应 Controller）
3. `HostAllModule` 未配置外部登录
4. Account.Host 的 `Program.cs` 缺少 `app.MapControllers()`

**方案**: 用自定义 `ExternalLoginController` + `WeChatAuthService` / `DingTalkAuthService` 替代内置 OAuth handler，利用 ABP Identity 内置的 `IdentityUserLogin`（`AspNetUserLogins` 表）存储绑定关系。

---

## Task 1: 创建外部登录 DTO 和接口（Contracts 层）

**新建** `src/Services/Account/H.Account.Application.Contracts/Dtos/ExternalLoginDtos.cs`
- `ExternalLoginRequestDto`: Provider, ProviderKey(openid), DisplayName, AvatarUrl, UnionId
- `ExternalLoginResultDto`: Success, Message, IsNewUser, User

**新建** `src/Services/Account/H.Account.Application.Contracts/Services/IExternalLoginAppService.cs`
- `ExternalLoginAsync(ExternalLoginRequestDto)` - 核心：查找绑定 → 自动注册/直接登录 → 写 Cookie
- `BindExternalAccountAsync(Guid userId, ExternalLoginRequestDto)` - 绑定已有账号
- `UnbindExternalAccountAsync(Guid userId, string provider)` - 解绑
- `GetExternalAccountsAsync(Guid userId)` - 获取已绑定列表

---

## Task 2: 创建配置选项和第三方 API 封装（Application 层）

**新建** `src/Services/Account/H.Account.Application/ExternalLogin/ExternalLoginOptions.cs`
- `ExternalLoginOptions` 对应 appsettings 的 `"ExternalLogin"` 节
- 含 `WeChat` 和 `DingTalk` 两个 `ProviderOptions`（Enabled, ClientId, ClientSecret, CallbackPath）

**新建** `src/Services/Account/H.Account.Application/ExternalLogin/WeChatAuthService.cs`
- `BuildAuthorizationUrl(state, callbackUrl)` → 拼接 `https://open.weixin.qq.com/connect/qrconnect?...#wechat_redirect`
- `GetAccessTokenAsync(code)` → GET `https://api.weixin.qq.com/sns/oauth2/access_token`
- `GetUserInfoAsync(accessToken, openId)` → GET `https://api.weixin.qq.com/sns/userinfo`

**新建** `src/Services/Account/H.Account.Application/ExternalLogin/DingTalkAuthService.cs`
- `BuildAuthorizationUrl(state, callbackUrl)` → 拼接 `https://oapi.dingtalk.com/connect/oauth2/sns_authorize?...`
- `GetUserInfoByCodeAsync(tmpAuthCode)` → POST `https://oapi.dingtalk.com/sns/getuserinfo_bycode` (带 HmacSHA256 签名)

---

## Task 3: 创建外部登录应用服务（Application 层）

**新建** `src/Services/Account/H.Account.Application/Services/ExternalLoginAppService.cs`

核心逻辑 `ExternalLoginAsync`:
1. `FindByLoginAsync(provider, providerKey)` 查找已绑定用户
2. 未绑定 → 自动创建用户（生成随机密码满足 ABP 策略） + `AddLoginAsync` 绑定
3. 已绑定 → 检查 IsActive
4. 写入 Cookie 认证 (`SignInAsync`)
5. 返回结果（含 IsNewUser 标记）

**修改** `src/Services/Account/H.Account.Application/AccountApplicationModule.cs`
- 添加 `context.Services.AddHttpClient()` 注册

---

## Task 4: 创建外部登录 Controller（Web 层）

**新建** `src/Services/Account/H.Account.Web/Controllers/ExternalLoginController.cs`
- 路由: `[Route("api/external-login")]`, `[AllowAnonymous]`

**端点 `GET challenge`**: 
1. 生成 state + 写入临时 Cookie (`ext_login_state`, 5分钟有效, HttpOnly)
2. 构建 callbackUrl + 授权 URL
3. 302 Redirect 到微信/钉钉授权页

**端点 `GET callback`**:
1. 从 Cookie 读取 state 并验证一致性
2. 根据 provider 调用对应 AuthService 换取用户信息
3. 调用 `IExternalLoginAppService.ExternalLoginAsync` 处理登录
4. 302 Redirect 到 `/account/external-callback?success=true` 或 `/account/login?error=...`

---

## Task 5: 修改登录页面和回调页面

**修改** `src/Services/Account/H.Account.Web/Pages/Login.razor`
- `ExternalLogin` 方法路径改为 `/api/external-login/challenge`
- 添加 `forceLoad: true`（需要离开 Blazor SPA 跳转）

**修改** `src/Services/Account/H.Account.Web/Pages/ExternalLoginCallback.razor`
- 处理 `success=true` 参数 → `NavigateTo("/", forceLoad: true)` 重新加载以读取 Cookie
- 处理 `error` 参数 → 显示错误 + 返回登录按钮
- 处理 `isNewUser=true` → 可选：引导完善资料

---

## Task 6: 修改宿主模块配置

**修改** `src/Host/Account/H.Account.Host/AccountHostModule.cs`
- 删除 `ConfigureExternalLogin` 中的 `AddOAuth` 链式调用
- 替换为: `Configure<ExternalLoginOptions>` + 注册 `WeChatAuthService` / `DingTalkAuthService`

**修改** `src/Host/Account/H.Account.Host/Program.cs`
- 在 `app.UseAntiforgery()` 后添加 `app.MapControllers()`

**修改** `src/Host/H.LowCode.Host.All/H.LowCode.Host.All/HostAllModule.cs`
- 添加 `ConfigureExternalLogin` 方法（同 AccountHostModule）
- 在 `ConfigureServices` 中调用

**修改** `src/Host/H.LowCode.Host.All/H.LowCode.Host.All/appsettings.json`
- 添加 `ExternalLogin` 配置节（与 Account.Host 格式一致）

---

## Task 7: 编译验证

- 执行 `dotnet build` 验证所有项目编译通过
- 检查项目引用链：Host → Web(Controllers) → Application → Contracts

---

## 关键文件索引

**新建 (7 个)**:
- `src/Services/Account/H.Account.Application.Contracts/Dtos/ExternalLoginDtos.cs`
- `src/Services/Account/H.Account.Application.Contracts/Services/IExternalLoginAppService.cs`
- `src/Services/Account/H.Account.Application/ExternalLogin/ExternalLoginOptions.cs`
- `src/Services/Account/H.Account.Application/ExternalLogin/WeChatAuthService.cs`
- `src/Services/Account/H.Account.Application/ExternalLogin/DingTalkAuthService.cs`
- `src/Services/Account/H.Account.Application/Services/ExternalLoginAppService.cs`
- `src/Services/Account/H.Account.Web/Controllers/ExternalLoginController.cs`

**修改 (6 个)**:
- `src/Services/Account/H.Account.Web/Pages/Login.razor`
- `src/Services/Account/H.Account.Web/Pages/ExternalLoginCallback.razor`
- `src/Host/Account/H.Account.Host/AccountHostModule.cs`
- `src/Host/Account/H.Account.Host/Program.cs`
- `src/Host/H.LowCode.Host.All/H.LowCode.Host.All/HostAllModule.cs`
- `src/Host/H.LowCode.Host.All/H.LowCode.Host.All/appsettings.json`
- `src/Services/Account/H.Account.Application/AccountApplicationModule.cs`

## 验证方式

1. `dotnet build src/H.LowCode.slnx` 编译通过
2. 启动 Host.All 项目，访问 `/account/login`，点击微信/钉钉登录按钮应跳转到对应授权页面
3. 未配置 AppId 时应提示未启用
