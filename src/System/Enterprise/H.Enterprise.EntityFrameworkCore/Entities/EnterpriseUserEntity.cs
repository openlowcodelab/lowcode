namespace H.Enterprise.EntityFrameworkCore.Entities;

/// <summary>
/// 企业用户关联实体（多对多关系）
/// </summary>
public class EnterpriseUserEntity
{
    /// <summary>
    /// 关联 ID
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// 企业 ID
    /// </summary>
    public Guid EnterpriseId { get; set; }

    /// <summary>
    /// 用户 ID（关联 Account 服务 IdentityUser）
    /// </summary>
    public Guid UserId { get; set; }

    /// <summary>
    /// 用户名（冗余字段，便于显示）
    /// </summary>
    public string UserName { get; set; } = string.Empty;

    /// <summary>
    /// 角色（Owner/Admin/Member 等）
    /// </summary>
    public string Role { get; set; } = "Member";

    /// <summary>
    /// 是否为默认企业（用户登录时自动选择）
    /// </summary>
    public bool IsDefault { get; set; }

    /// <summary>
    /// 加入时间
    /// </summary>
    public DateTime JoinedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// 创建时间
    /// </summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// 创建者 ID
    /// </summary>
    public Guid? CreatedBy { get; set; }

    /// <summary>
    /// 关联企业
    /// </summary>
    public virtual EnterpriseEntity? Enterprise { get; set; }

}
