using H.Abp.Application.Contracts;

namespace H.Workbench.Application.Contracts;

/// <summary>
/// 会话查询 DTO
/// </summary>
public class SessionQueryDto : PagedResultRequestDto
{
    public string? Filter { get; set; }
}
