using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace H.Approval.Web;

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
        SpecifiedRoles = new List<string>();
        ApproverMode = ApproverModeEnum.Sequential;
    }
    
    /// <summary>
    /// 审批人类型
    /// </summary>
    public ApproverTypeEnum ApproverType { get; set; }
    
    /// <summary>
    /// 指定审批人ID列表
    /// </summary>
    public List<string> SpecifiedUsers { get; set; }
    
    /// <summary>
    /// 指定角色ID列表
    /// </summary>
    public List<string> SpecifiedRoles { get; set; }
    
    /// <summary>
    /// 多人审批方式
    /// </summary>
    public ApproverModeEnum ApproverMode { get; set; }
}
