using Volo.Abp.Application.Dtos;

namespace H.Notification.Application.Contracts;

/// <summary>
/// 联系人DTO
/// </summary>
public class ContactDto : FullAuditedEntityDto<Guid>
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public bool IsEnabled { get; set; }

    /// <summary>
    /// 站内信目标用户标识
    /// </summary>
    public string? InAppUserId { get; set; }

    public string? Email { get; set; }
    public string? Phone { get; set; }
    public string? WebhookUrl { get; set; }

    /// <summary>
    /// 所属分组ID集合
    /// </summary>
    public List<long> GroupIds { get; set; } = new();
}

/// <summary>
/// 创建联系人DTO
/// </summary>
public class CreateContactDto
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public bool IsEnabled { get; set; } = true;
    public string? InAppUserId { get; set; }
    public string? Email { get; set; }
    public string? Phone { get; set; }
    public string? WebhookUrl { get; set; }
}

/// <summary>
/// 更新联系人DTO
/// </summary>
public class UpdateContactDto
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public bool IsEnabled { get; set; }
    public string? InAppUserId { get; set; }
    public string? Email { get; set; }
    public string? Phone { get; set; }
    public string? WebhookUrl { get; set; }
}

/// <summary>
/// 联系人查询参数
/// </summary>
public class ContactQueryDto : PagedResultRequestDto
{
    public string? Filter { get; set; }
    public long? GroupId { get; set; }
}

/// <summary>
/// 联系人分组DTO
/// </summary>
public class ContactGroupDto : FullAuditedEntityDto<long>
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public bool IsEnabled { get; set; }

    /// <summary>
    /// 分组内联系人数量
    /// </summary>
    public int ContactCount { get; set; }

    /// <summary>
    /// 分组内联系人ID集合（GetAsync 时填充）
    /// </summary>
    public List<Guid> ContactIds { get; set; } = new();
}

/// <summary>
/// 创建联系人分组DTO
/// </summary>
public class CreateContactGroupDto
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public bool IsEnabled { get; set; } = true;

    /// <summary>
    /// 分组内联系人ID集合
    /// </summary>
    public List<Guid> ContactIds { get; set; } = new();
}

/// <summary>
/// 更新联系人分组DTO
/// </summary>
public class UpdateContactGroupDto
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public bool IsEnabled { get; set; }

    /// <summary>
    /// 分组内联系人ID集合
    /// </summary>
    public List<Guid> ContactIds { get; set; } = new();
}

/// <summary>
/// 联系人分组查询参数
/// </summary>
public class ContactGroupQueryDto : PagedResultRequestDto
{
    public string? Filter { get; set; }
}
