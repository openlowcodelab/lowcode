using H.Abp.Application.Contracts;
using H.Util.Base;
using System.ComponentModel.DataAnnotations;

namespace H.Workbench.Application.Contracts;

/// <summary>
/// 审批规则 DTO：工具（可选参数模式）→ Require/Auto/Deny
/// </summary>
public class ApprovalRuleDto : AuditedEntityDto<Guid>
{
    public string Name { get; set; } = string.Empty;
    public string? AgentType { get; set; }
    public string ToolPattern { get; set; } = string.Empty;
    public string? ArgPattern { get; set; }

    /// <summary>Require | Auto | Deny</summary>
    public string Effect { get; set; } = "Require";
    public int Priority { get; set; }
    public bool IsEnabled { get; set; }
}

public class CreateApprovalRuleDto
{
    [Required(ErrorMessage = "规则名不能为空")]
    [StringLength(100)]
    public string Name { get; set; } = string.Empty;

    /// <summary>留空=对所有员工生效</summary>
    [StringLength(100)]
    public string? AgentType { get; set; }

    [Required(ErrorMessage = "工具匹配不能为空")]
    [StringLength(100)]
    public string ToolPattern { get; set; } = string.Empty;

    [StringLength(500)]
    public string? ArgPattern { get; set; }

    [Required]
    public string Effect { get; set; } = "Require";

    public int Priority { get; set; }
    public bool IsEnabled { get; set; } = true;
}

public class UpdateApprovalRuleDto : CreateApprovalRuleDto
{
}

/// <summary>
/// 审批规则管理服务
/// </summary>
public interface IApprovalRuleAppService : IAppService
{
    Task<BaseOutput<List<ApprovalRuleDto>>> GetListAsync(string? agentType = null);
    Task<BaseOutput<ApprovalRuleDto>> CreateAsync(CreateApprovalRuleDto input);
    Task<BaseOutput<ApprovalRuleDto>> UpdateAsync(Guid id, UpdateApprovalRuleDto input);
    Task<BaseOutput> DeleteAsync(Guid id);

    /// <summary>
    /// 供运行时（AgentFactory）取生效规则：按优先级降序，含全局规则
    /// </summary>
    Task<BaseOutput<List<ApprovalRuleDto>>> GetEffectiveRulesAsync(string? agentType);
}
