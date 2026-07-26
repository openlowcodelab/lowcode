using H.Abstractions;

namespace H.Order.Application.Contracts;

/// <summary>
/// 路由规则 DTO
/// </summary>
public class RouteRuleDto : FullAuditedEntityDto<Guid>
{
    /// <summary>规则名称</summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>命中后下发的供应商编码</summary>
    public string SupplierCode { get; set; } = string.Empty;

    /// <summary>规则类型</summary>
    public RouteRuleTypeEnum RuleType { get; set; }

    /// <summary>优先级（数字越小越优先）</summary>
    public int Priority { get; set; }

    /// <summary>是否启用</summary>
    public bool IsEnabled { get; set; }

    /// <summary>条件集合（JSON），见 <see cref="RuleCondition"/></summary>
    public string? ConditionsJson { get; set; }

    /// <summary>是否为兜底规则（无条件匹配）</summary>
    public bool Fallback { get; set; }

    /// <summary>备注</summary>
    public string? Remark { get; set; }
}

/// <summary>
/// 创建路由规则 DTO
/// </summary>
public class CreateRouteRuleDto
{
    /// <summary>规则名称</summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>命中后下发的供应商编码</summary>
    public string SupplierCode { get; set; } = string.Empty;

    /// <summary>规则类型</summary>
    public RouteRuleTypeEnum RuleType { get; set; }

    /// <summary>优先级</summary>
    public int Priority { get; set; } = 100;

    /// <summary>是否启用</summary>
    public bool IsEnabled { get; set; } = true;

    /// <summary>条件集合（JSON）</summary>
    public string? ConditionsJson { get; set; }

    /// <summary>是否为兜底规则</summary>
    public bool Fallback { get; set; }

    /// <summary>备注</summary>
    public string? Remark { get; set; }
}

/// <summary>
/// 更新路由规则 DTO
/// </summary>
public class UpdateRouteRuleDto
{
    /// <summary>规则名称</summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>命中后下发的供应商编码</summary>
    public string SupplierCode { get; set; } = string.Empty;

    /// <summary>规则类型</summary>
    public RouteRuleTypeEnum RuleType { get; set; }

    /// <summary>优先级</summary>
    public int Priority { get; set; }

    /// <summary>是否启用</summary>
    public bool IsEnabled { get; set; }

    /// <summary>条件集合（JSON）</summary>
    public string? ConditionsJson { get; set; }

    /// <summary>是否为兜底规则</summary>
    public bool Fallback { get; set; }

    /// <summary>备注</summary>
    public string? Remark { get; set; }
}

/// <summary>
/// 路由规则查询参数
/// </summary>
public class RouteRuleQueryDto : PagedResultRequestDto
{
    /// <summary>关键词</summary>
    public string? Filter { get; set; }

    /// <summary>是否启用</summary>
    public bool? IsEnabled { get; set; }

    /// <summary>供应商编码</summary>
    public string? SupplierCode { get; set; }
}