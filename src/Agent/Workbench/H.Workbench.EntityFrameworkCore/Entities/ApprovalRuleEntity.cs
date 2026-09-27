using Volo.Abp.Domain.Entities.Auditing;

namespace H.Workbench.EntityFrameworkCore;

/// <summary>
/// 审批规则：把"要不要人来批"从技能级布尔升级为「工具 + 参数模式」级判断。
/// 技能级 RequiresApproval 仍是兜底，规则只用于收窄或放宽具体场景，
/// 例如"推送到 main 必须批、推送到 feature/* 免批"。
/// </summary>
public class ApprovalRuleEntity : AuditedEntity<Guid>
{
    /// <summary>规则名（人读）</summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>归属员工；空=对所有员工生效</summary>
    public string? AgentType { get; set; }

    /// <summary>
    /// 工具名匹配：精确名或含 * 的通配（如 Git* / DbTool* 不适用，工具名即方法名 GitPushAsync）
    /// </summary>
    public string ToolPattern { get; set; } = string.Empty;

    /// <summary>
    /// 可选参数正则，对工具入参 JSON 原文匹配（如 "branch"\\s*:\\s*"(main|master)"）
    /// </summary>
    public string? ArgPattern { get; set; }

    /// <summary>Require=必须人工批 | Auto=直接放行 | Deny=直接拒绝不执行</summary>
    public string Effect { get; set; } = "Require";

    /// <summary>数值大者优先匹配</summary>
    public int Priority { get; set; }

    public bool IsEnabled { get; set; } = true;
}
