using H.Abp.Application.Contracts;
using System.ComponentModel.DataAnnotations;

namespace H.Workbench.Application.Contracts;

/// <summary>
/// 插件 DTO（能力包：绑定给员工后整体授予其技能）
/// </summary>
public class WorkbenchPluginDto : AuditedEntityDto<Guid>
{
    public string PluginKey { get; set; } = string.Empty;
    public string PluginName { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;

    /// <summary>来源：Market(插件市场)/Custom(自定义)</summary>
    public string Source { get; set; } = "Custom";

    public string Icon { get; set; } = string.Empty;
    public bool IsEnabled { get; set; }

    /// <summary>该插件授予的技能名列表</summary>
    public List<string> SkillKeys { get; set; } = new();

    /// <summary>是否来自插件市场</summary>
    public bool IsMarket => Source == "Market";
}

public class CreateWorkbenchPluginDto
{
    [Required(ErrorMessage = "插件名称不能为空")]
    [StringLength(200, ErrorMessage = "插件名称不能超过200个字符")]
    public string PluginName { get; set; } = string.Empty;

    [StringLength(1000, ErrorMessage = "插件描述不能超过1000个字符")]
    public string Description { get; set; } = string.Empty;

    [StringLength(100)]
    public string Icon { get; set; } = string.Empty;

    /// <summary>授予的技能名列表，空列表的插件不会授予任何工具</summary>
    public List<string> SkillKeys { get; set; } = new();
}

public class UpdateWorkbenchPluginDto
{
    [Required(ErrorMessage = "插件名称不能为空")]
    [StringLength(200, ErrorMessage = "插件名称不能超过200个字符")]
    public string PluginName { get; set; } = string.Empty;

    [StringLength(1000, ErrorMessage = "插件描述不能超过1000个字符")]
    public string Description { get; set; } = string.Empty;

    [StringLength(100)]
    public string Icon { get; set; } = string.Empty;

    public List<string> SkillKeys { get; set; } = new();
}

public class WorkbenchPluginQueryDto : PagedResultRequestDto
{
    public string? Filter { get; set; }

    /// <summary>按来源过滤：Market/Custom</summary>
    public string? Source { get; set; }
}
