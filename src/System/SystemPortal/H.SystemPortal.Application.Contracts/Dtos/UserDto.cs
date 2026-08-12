namespace H.SystemPortal.Application.Contracts;

/// <summary>
/// 用户 DTO
/// </summary>
public class UserDto
{
    public Guid Id { get; set; }
    public string UserName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string PhoneNumber { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty; // 注意：实际应用中不应直接暴露密码字段
    public UserType UserType { get; set; }
    public List<string> RoleNames { get; set; } = new();
    public string? LoginMode { get; set; }
    public bool IsActive { get; set; }
    public bool EmailConfirmed { get; set; }
    public bool PhoneNumberConfirmed { get; set; }
    public bool IsLocked => LockoutEnd > DateTime.UtcNow;
    public DateTime? LockoutEnd { get; set; }
    public int AccessFailedCount { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public DateTime? LastLoginAt { get; set; }
    public Guid? CreatedBy { get; set; }
    public Guid? UpdatedBy { get; set; }
    public string? Remark { get; set; }

    public List<ExternalAccountDto> ExternalAccounts { get; set; } = new();
}

/// <summary>
/// 分配角色 DTO
/// </summary>
public class AssignRolesDto
{
    public List<string> RoleNames { get; set; } = new();
}

/// <summary>
/// 用户列表 DTO
/// </summary>
public class UserListDto : UserDto
{
    public string? CreatedByName { get; set; }
    public string? UpdatedByName { get; set; }
}

/// <summary>
/// 创建用户请求 DTO
/// </summary>
public class CreateUserDto
{
    public string UserName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string ConfirmPassword { get; set; } = string.Empty;
    public string? PhoneNumber { get; set; }
    public UserType UserType { get; set; } = UserType.Normal;
    public List<string> RoleNames { get; set; } = new();
    public bool IsActive { get; set; } = true;
    public bool EmailConfirmed { get; set; }
    public bool PhoneNumberConfirmed { get; set; }
    public string? Remark { get; set; }
}

/// <summary>
/// 更新用户请求 DTO
/// </summary>
public class UpdateUserDto
{
    public string UserName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? PhoneNumber { get; set; }
    public UserType UserType { get; set; }
    public List<string> RoleNames { get; set; } = new();
    public bool IsActive { get; set; } = true;
    public bool EmailConfirmed { get; set; }
    public bool PhoneNumberConfirmed { get; set; }
    public string? Remark { get; set; }
}

/// <summary>
/// 更新用户状态 DTO
/// </summary>
public class UpdateUserStatusDto
{
    public bool IsActive { get; set; }
    public string? Remark { get; set; }
}

/// <summary>
/// 重置密码 DTO
/// </summary>
public class ResetPasswordDto
{
    public string NewPassword { get; set; } = string.Empty;
    public string ConfirmPassword { get; set; } = string.Empty;
}

/// <summary>
/// 用户查询参数
/// </summary>
public class UserQueryParams
{
    public string? Keyword { get; set; }
    public UserType? UserType { get; set; }
    public bool? IsActive { get; set; }
    public int PageIndex { get; set; } = 1;
    public int PageSize { get; set; } = 10;
}

/// <summary>
/// 分页结果
/// </summary>
public class PagedResult<T>
{
    public List<T> Items { get; set; } = new();
    public int Total { get; set; }
    public int PageIndex { get; set; }
    public int PageSize { get; set; }
    public int TotalPages => (int)Math.Ceiling(Total / (double)PageSize);
}

public class ExternalAccountDto
{
    public string Provider { get; set; } = string.Empty;
    public string ProviderKey { get; set; } = string.Empty;
    public string? DisplayName { get; set; }
    public DateTime BoundAt { get; set; }
}

/// <summary>
/// 登录请求 DTO
/// </summary>
public class LoginRequestDto
{
    public string Account { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public bool RememberMe { get; set; }
}

/// <summary>
/// 登录类型
/// </summary>
public enum LoginType
{
    Account = 0,
    Email = 1,
    PhoneNumber = 2
}

/// <summary>
/// 注册请求 DTO
/// </summary>
public class RegisterRequestDto
{
    public string? UserName { get; set; }
    public string? Email { get; set; }
    public string? PhoneNumber { get; set; }
    public string Password { get; set; } = string.Empty;
    public string ConfirmPassword { get; set; } = string.Empty;
    public string? EmailCode { get; set; }
    public string? PhoneCode { get; set; }
    public RegisterType RegisterType { get; set; }
}

/// <summary>
/// 注册类型
/// </summary>
public enum RegisterType
{
    UserName = 0,
    Email = 1,
    PhoneNumber = 2
}

/// <summary>
/// 认证响应 DTO
/// </summary>
public class AuthResponseDto
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public UserDto? User { get; set; }
}
