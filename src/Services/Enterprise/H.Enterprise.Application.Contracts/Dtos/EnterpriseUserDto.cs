using System.ComponentModel.DataAnnotations;

namespace H.Enterprise.Application.Contracts.Dtos;

/// <summary>
/// 企业用户 DTO（输出）
/// </summary>
public class EnterpriseUserDto
{
    public Guid Id { get; set; }
    public Guid EnterpriseId { get; set; }
    public string EnterpriseName { get; set; } = string.Empty;
    public Guid UserId { get; set; }
    public string UserName { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
    public bool IsDefault { get; set; }
    public DateTime JoinedAt { get; set; }
}

/// <summary>
/// 添加企业用户 DTO
/// </summary>
public class AddEnterpriseUserDto
{
    [Required]
    public Guid EnterpriseId { get; set; }

    [Required]
    public Guid UserId { get; set; }

    /// <summary>
    /// 角色（Owner/Admin/Member 等）
    /// </summary>
    [Required]
    public string Role { get; set; } = "Member";
}
