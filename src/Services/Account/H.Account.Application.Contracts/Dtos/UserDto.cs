using System;
using System.ComponentModel.DataAnnotations;

namespace H.Account.Application.Contracts;

/// <summary>
/// 用户 DTO
/// </summary>
public class UserDto
{
    public Guid Id { get; set; }
    public string UserName { get; set; }
    public string Email { get; set; }
    public string PhoneNumber { get; set; }
    public string Password { get; set; } // 注意：实际应用中不应直接暴露密码字段
    public UserType UserType { get; set; }
    public string? Roles { get; set; }
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
    public string UserName { get; set; }
    public string Email { get; set; }
    public string Password { get; set; }
    public string ConfirmPassword { get; set; }
    public string? PhoneNumber { get; set; }
    public UserType UserType { get; set; } = UserType.Normal;
    public string? Roles { get; set; }
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
    public string UserName { get; set; }
    public string Email { get; set; }
    public string? PhoneNumber { get; set; }
    public UserType UserType { get; set; }
    public string? Roles { get; set; }
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
    public string NewPassword { get; set; }
    public string ConfirmPassword { get; set; }
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

public class LoginRequestDto
{
    [Required(ErrorMessage = "用户名不能为空")]
    public string UserName { get; set; }
    
    [Required(ErrorMessage = "密码不能为空")]
    public string Password { get; set; }
}

public class RegisterRequestDto
{
    [Required(ErrorMessage = "用户名不能为空")]
    public string UserName { get; set; }
    
    [Required(ErrorMessage = "邮箱不能为空")]
    [EmailAddress(ErrorMessage = "邮箱格式不正确")]
    public string Email { get; set; }
    
    [Required(ErrorMessage = "密码不能为空")]
    [MinLength(6, ErrorMessage = "密码长度至少6位")]
    public string Password { get; set; }
    
    [Required(ErrorMessage = "确认密码不能为空")]
    [Compare(nameof(Password), ErrorMessage = "两次密码输入不一致")]
    public string ConfirmPassword { get; set; }
    
    public string PhoneNumber { get; set; }
}

public class AuthResponseDto
{
    public bool Success { get; set; }
    public string Message { get; set; }
    public string Token { get; set; }
    public UserDto? User { get; set; }
}