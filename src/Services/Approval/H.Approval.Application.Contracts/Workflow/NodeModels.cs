using System.Collections.Generic;

namespace H.Approval.Application.Contracts;

/// <summary>
/// 条件规则
/// </summary>
public class ConditionRule
{
    /// <summary>变量字段名</summary>
    public string Field { get; set; } = string.Empty;

    /// <summary>操作符: ==, !=, >, &lt;, >=, &lt;=, contains</summary>
    public string Operator { get; set; } = "==";

    /// <summary>比较值</summary>
    public string Value { get; set; } = string.Empty;
}

/// <summary>
/// 发起人节点
/// </summary>
public class StartNodeModel : NodeModelBase
{
    public StartNodeModel()
    {
        NodeType = NodeTypeEnum.Start;
        StartType = StartTypeEnum.All;
        SpecifiedUsers = new List<string>();
        SpecifiedUserNames = new List<string>();
        SpecifiedRoles = new List<string>();
        SpecifiedRoleNames = new List<string>();
    }

    /// <summary>发起人类型</summary>
    public StartTypeEnum StartType { get; set; }

    /// <summary>指定成员ID列表</summary>
    public List<string> SpecifiedUsers { get; set; }

    /// <summary>指定成员姓名列表(与 SpecifiedUsers 一一对应)</summary>
    public List<string> SpecifiedUserNames { get; set; }

    /// <summary>指定角色ID列表</summary>
    public List<string> SpecifiedRoles { get; set; }

    /// <summary>指定角色名称列表</summary>
    public List<string> SpecifiedRoleNames { get; set; }
}

/// <summary>
/// 审核人节点
/// </summary>
public class ApproveModel : NodeModelBase
{
    public ApproveModel()
    {
        NodeType = NodeTypeEnum.Approve;
        ApproverType = ApproverTypeEnum.Specified;
        SpecifiedUsers = new List<string>();
        SpecifiedUserNames = new List<string>();
        SpecifiedRoles = new List<string>();
        SpecifiedRoleNames = new List<string>();
        ApproverMode = ApproverModeEnum.Sequential;
    }

    /// <summary>审批人类型</summary>
    public ApproverTypeEnum ApproverType { get; set; }

    /// <summary>指定审批人ID列表</summary>
    public List<string> SpecifiedUsers { get; set; }

    /// <summary>指定审批人姓名列表(与 SpecifiedUsers 一一对应)</summary>
    public List<string> SpecifiedUserNames { get; set; }

    /// <summary>指定角色ID列表</summary>
    public List<string> SpecifiedRoles { get; set; }

    /// <summary>指定角色名称列表</summary>
    public List<string> SpecifiedRoleNames { get; set; }

    /// <summary>多人审批方式</summary>
    public ApproverModeEnum ApproverMode { get; set; }
}

/// <summary>
/// 抄送节点
/// </summary>
public class CarbonCopyModel : NodeModelBase
{
    public CarbonCopyModel()
    {
        NodeType = NodeTypeEnum.CarbonCopy;
        CarbonCopyType = CarbonCopyTypeEnum.Specified;
        SpecifiedUsers = new List<string>();
        SpecifiedUserNames = new List<string>();
        SpecifiedRoles = new List<string>();
        SpecifiedRoleNames = new List<string>();
    }

    /// <summary>抄送人类型</summary>
    public CarbonCopyTypeEnum CarbonCopyType { get; set; }

    /// <summary>指定成员ID列表</summary>
    public List<string> SpecifiedUsers { get; set; }

    /// <summary>指定成员姓名列表</summary>
    public List<string> SpecifiedUserNames { get; set; }

    /// <summary>指定角色ID列表</summary>
    public List<string> SpecifiedRoles { get; set; }

    /// <summary>指定角色名称列表</summary>
    public List<string> SpecifiedRoleNames { get; set; }
}

/// <summary>
/// 条件节点(分支中的某一条件路径)
/// </summary>
public class ConditionModel : NodeModelBase
{
    public ConditionModel()
    {
        NodeType = NodeTypeEnum.Condition;
        Rules = new List<ConditionRule>();
    }

    /// <summary>条件规则列表(多规则为 AND 关系)</summary>
    public List<ConditionRule> Rules { get; set; }

    /// <summary>是否为默认分支(所有条件都不满足时走此分支)</summary>
    public bool IsDefault { get; set; }
}

/// <summary>
/// 条件分支节点
/// </summary>
public class BranchModel : NodeModelBase
{
    public BranchModel()
    {
        NodeType = NodeTypeEnum.Branch;
    }
}

/// <summary>
/// 结束节点
/// </summary>
public class EndNodeModel : NodeModelBase
{
}

/// <summary>
/// 多人审批方式枚举
/// </summary>
public enum ApproverModeEnum
{
    /// <summary>依次审批(按顺序)</summary>
    Sequential = 0,
    /// <summary>会签(需要所有人都同意)</summary>
    CounterSign = 1,
    /// <summary>或签(其中一人同意即可)</summary>
    OrSign = 2
}

/// <summary>
/// 审批人类型枚举
/// </summary>
public enum ApproverTypeEnum
{
    /// <summary>指定成员</summary>
    Specified = 0,
    /// <summary>发起人自选</summary>
    StarterSelect = 1,
    /// <summary>发起人自己</summary>
    StarterSelf = 2,
    /// <summary>指定角色</summary>
    Role = 3,
    /// <summary>部门主管</summary>
    DepartmentManager = 4
}

/// <summary>
/// 抄送人类型枚举
/// </summary>
public enum CarbonCopyTypeEnum
{
    /// <summary>指定成员</summary>
    Specified = 0,
    /// <summary>发起人自选</summary>
    StarterSelect = 1,
    /// <summary>指定角色</summary>
    Role = 2
}

/// <summary>
/// 发起人类型枚举
/// </summary>
public enum StartTypeEnum
{
    /// <summary>全员可发起</summary>
    All = 0,
    /// <summary>指定成员可发起</summary>
    Specified = 1,
    /// <summary>指定角色可发起</summary>
    Role = 2
}
