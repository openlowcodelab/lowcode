namespace H.Account.Domain;

public class UserEntity
{
    public Guid Id { get; set; }
    
    /// <summary>
    /// 用户名
    /// </summary>
    public string UserName { get; set; } = string.Empty;
    
    /// <summary>
    /// 邮箱
    /// </summary>
    public string Email { get; set; } = string.Empty;
    
    /// <summary>
    /// 密码哈希
    /// </summary>
    public string PasswordHash { get; set; } = string.Empty;
    
    /// <summary>
    /// 电话号码
    /// </summary>
    public string PhoneNumber { get; set; } = string.Empty;
    
    /// <summary>
    /// 用户类型
    /// </summary>
    public int UserType { get; set; }
    
    /// <summary>
    /// 角色（逗号分隔）
    /// </summary>
    public string? Roles { get; set; }
    
    /// <summary>
    /// 账户是否激活
    /// </summary>
    public bool IsActive { get; set; } = true;
    
    /// <summary>
    /// 邮箱是否已验证
    /// </summary>
    public bool EmailConfirmed { get; set; }
    
    /// <summary>
    /// 手机号是否已验证
    /// </summary>
    public bool PhoneNumberConfirmed { get; set; }
    
    /// <summary>
    /// 账户锁定结束时间（null 表示未锁定）
    /// </summary>
    public DateTime? LockoutEnd { get; set; }
    
    /// <summary>
    /// 登录失败次数
    /// </summary>
    public int AccessFailedCount { get; set; }
    
    /// <summary>
    /// 创建时间
    /// </summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    /// <summary>
    /// 更新时间
    /// </summary>
    public DateTime? UpdatedAt { get; set; }
    
    /// <summary>
    /// 最后登录时间
    /// </summary>
    public DateTime? LastLoginAt { get; set; }
    
    /// <summary>
    /// 创建者 ID
    /// </summary>
    public Guid? CreatedBy { get; set; }
    
    /// <summary>
    /// 更新者 ID
    /// </summary>
    public Guid? UpdatedBy { get; set; }
    
    /// <summary>
    /// 备注
    /// </summary>
    public string? Remark { get; set; }
}