using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using H.Approval.Application.Contracts;
using Microsoft.Extensions.Logging;

namespace H.Approval.Application;

/// <summary>
/// 审批工作流引擎(自包含,直接解释设计器产出的节点树JSON)
/// <para>支持: 发起 → 审批(依次/会签/或签) → 抄送(跳过) → 条件分支(规则求值) → 结束</para>
/// </summary>
public class ApprovalWorkflowEngine
{
    private readonly ILogger<ApprovalWorkflowEngine> _logger;

    public ApprovalWorkflowEngine(ILogger<ApprovalWorkflowEngine> logger)
    {
        _logger = logger;
    }

    /// <summary>
    /// 解析节点树
    /// </summary>
    public NodeModelBase? ParseDefinition(string definitionJson)
    {
        if (string.IsNullOrWhiteSpace(definitionJson) || definitionJson == "{}")
            return null;
        return NodeSerializer.Deserialize(definitionJson);
    }

    /// <summary>
    /// 解析审批变量JSON为字典
    /// </summary>
    public Dictionary<string, object?> ParseVariables(string? variablesJson)
    {
        if (string.IsNullOrWhiteSpace(variablesJson) || variablesJson == "{}")
            return new Dictionary<string, object?>();
        try
        {
            var doc = JsonDocument.Parse(variablesJson);
            var result = new Dictionary<string, object?>();
            foreach (var prop in doc.RootElement.EnumerateObject())
            {
                result[prop.Name] = prop.Value.ValueKind switch
                {
                    JsonValueKind.String => prop.Value.GetString(),
                    JsonValueKind.Number => prop.Value.GetRawText(),
                    JsonValueKind.True => "true",
                    JsonValueKind.False => "false",
                    _ => prop.Value.GetRawText()
                };
            }
            return result;
        }
        catch
        {
            return new Dictionary<string, object?>();
        }
    }

    /// <summary>
    /// 按实际路径获取所有审批节点(经过条件分支求值)
    /// </summary>
    public List<ApproveModel> GetApproveNodesOnPath(NodeModelBase root, Dictionary<string, object?> variables)
    {
        var nodes = new List<ApproveModel>();
        WalkOnPath(root, nodes, variables);
        return nodes;
    }

    private void WalkOnPath(NodeModelBase? node, List<ApproveModel> nodes, Dictionary<string, object?> variables)
    {
        if (node == null) return;

        switch (node.NodeType)
        {
            case NodeTypeEnum.Start:
                foreach (var child in node.ChildNodes) WalkOnPath(child, nodes, variables);
                break;
            case NodeTypeEnum.Approve:
                if (node is ApproveModel approve) nodes.Add(approve);
                foreach (var child in node.ChildNodes) WalkOnPath(child, nodes, variables);
                break;
            case NodeTypeEnum.CarbonCopy:
                // 抄送节点不产生审批任务,直接继续
                foreach (var child in node.ChildNodes) WalkOnPath(child, nodes, variables);
                break;
            case NodeTypeEnum.Condition:
                // 条件节点内的子节点(分支路径上的节点)
                foreach (var child in node.ChildNodes) WalkOnPath(child, nodes, variables);
                break;
            case NodeTypeEnum.Branch:
                // 条件分支: 求值选择一条路径
                var selected = EvaluateBranch(node, variables);
                if (selected != null)
                {
                    WalkOnPath(selected, nodes, variables);
                }
                // 分支汇合后的后续节点
                foreach (var child in node.ChildNodes) WalkOnPath(child, nodes, variables);
                break;
        }
    }

    /// <summary>
    /// 条件分支求值: 遍历所有条件,返回第一个满足条件的;都不满足则返回默认分支或第一个
    /// </summary>
    private NodeModelBase? EvaluateBranch(NodeModelBase branch, Dictionary<string, object?> variables)
    {
        NodeModelBase? defaultNode = null;

        foreach (var cond in branch.ConditionNodes)
        {
            if (cond is ConditionModel cm)
            {
                if (cm.IsDefault)
                {
                    defaultNode = cond;
                    continue;
                }
                if (EvaluateRules(cm.Rules, variables))
                {
                    _logger.LogInformation("条件分支命中: {ConditionName}", cm.NodeName);
                    return cond;
                }
            }
        }

        // 都不满足,走默认分支
        if (defaultNode != null)
        {
            _logger.LogInformation("条件分支走默认路径");
            return defaultNode;
        }

        // 没有默认分支,走第一个
        return branch.ConditionNodes.FirstOrDefault();
    }

    /// <summary>
    /// 评估条件规则(多规则 AND 关系)
    /// </summary>
    private bool EvaluateRules(List<ConditionRule> rules, Dictionary<string, object?> variables)
    {
        if (rules == null || rules.Count == 0) return true;

        foreach (var rule in rules)
        {
            if (!variables.TryGetValue(rule.Field, out var value))
                return false;
            if (!EvaluateRule(rule, value))
                return false;
        }
        return true;
    }

    private bool EvaluateRule(ConditionRule rule, object? variableValue)
    {
        var varStr = variableValue?.ToString() ?? "";
        var ruleStr = rule.Value ?? "";
        return rule.Operator switch
        {
            "==" or "=" => varStr == ruleStr,
            "!=" or "<>" => varStr != ruleStr,
            ">" => decimal.TryParse(varStr, out var v1) && decimal.TryParse(ruleStr, out var v2) && v1 > v2,
            "<" => decimal.TryParse(varStr, out var v1) && decimal.TryParse(ruleStr, out var v2) && v1 < v2,
            ">=" => decimal.TryParse(varStr, out var v1) && decimal.TryParse(ruleStr, out var v2) && v1 >= v2,
            "<=" => decimal.TryParse(varStr, out var v1) && decimal.TryParse(ruleStr, out var v2) && v1 <= v2,
            "contains" => varStr.Contains(ruleStr),
            _ => false
        };
    }

    /// <summary>
    /// 获取第一个审批节点及其所有审批人
    /// </summary>
    public (ApproveModel Node, List<(string Id, string Name)> Assignees)? GetFirstApprove(
        NodeModelBase root, string creatorId, string creatorName, Dictionary<string, object?> variables)
    {
        var nodes = GetApproveNodesOnPath(root, variables);
        if (nodes.Count == 0) return null;

        var assignees = ResolveAssignees(nodes[0], creatorId, creatorName);
        return (nodes[0], assignees);
    }

    /// <summary>
    /// 获取当前节点之后的下一个审批节点及其所有审批人
    /// </summary>
    public (ApproveModel Node, List<(string Id, string Name)> Assignees)? GetNextApprove(
        NodeModelBase root, string currentNodeId, string creatorId, string creatorName, Dictionary<string, object?> variables)
    {
        var nodes = GetApproveNodesOnPath(root, variables);
        var currentIndex = nodes.FindIndex(n => n.Id == currentNodeId);
        if (currentIndex < 0 || currentIndex >= nodes.Count - 1) return null;

        var next = nodes[currentIndex + 1];
        var assignees = ResolveAssignees(next, creatorId, creatorName);
        return (next, assignees);
    }

    /// <summary>
    /// 解析审批人列表
    /// </summary>
    public List<(string Id, string Name)> ResolveAssignees(ApproveModel node, string creatorId, string creatorName)
    {
        var result = new List<(string Id, string Name)>();

        switch (node.ApproverType)
        {
            case ApproverTypeEnum.Specified:
            case ApproverTypeEnum.StarterSelect:
                for (int i = 0; i < node.SpecifiedUsers.Count; i++)
                {
                    var uid = node.SpecifiedUsers[i];
                    var uname = i < node.SpecifiedUserNames.Count ? node.SpecifiedUserNames[i] : $"用户 {uid[..Math.Min(8, uid.Length)]}";
                    result.Add((uid, uname));
                }
                if (result.Count == 0)
                {
                    _logger.LogWarning("审批节点 {NodeName} 指定成员为空,回退到发起人", node.NodeName);
                    result.Add((creatorId, creatorName));
                }
                break;

            case ApproverTypeEnum.StarterSelf:
                result.Add((creatorId, creatorName));
                break;

            case ApproverTypeEnum.Role:
                // 角色类型: 使用 SpecifiedUsers 作为角色成员ID(由设计器选择)
                for (int i = 0; i < node.SpecifiedUsers.Count; i++)
                {
                    var uid = node.SpecifiedUsers[i];
                    var uname = i < node.SpecifiedUserNames.Count ? node.SpecifiedUserNames[i] : $"用户 {uid[..Math.Min(8, uid.Length)]}";
                    result.Add((uid, uname));
                }
                if (result.Count == 0)
                {
                    _logger.LogWarning("审批节点 {NodeName} 角色成员为空,回退到发起人", node.NodeName);
                    result.Add((creatorId, creatorName));
                }
                break;

            case ApproverTypeEnum.DepartmentManager:
                _logger.LogInformation("审批节点 {NodeName} 部门主管类型暂未集成,回退到发起人", node.NodeName);
                result.Add((creatorId, creatorName));
                break;

            default:
                result.Add((creatorId, creatorName));
                break;
        }

        return result;
    }
}
