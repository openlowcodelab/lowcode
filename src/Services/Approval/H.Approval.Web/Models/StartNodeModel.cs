using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace H.Approval.Web;

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
        SpecifiedRoles = new List<string>();
    }
    
    /// <summary>
    /// 发起人类型
    /// </summary>
    public StartTypeEnum StartType { get; set; }
    
    /// <summary>
    /// 指定成员ID列表
    /// </summary>
    public List<string> SpecifiedUsers { get; set; }
    
    /// <summary>
    /// 指定角色ID列表
    /// </summary>
    public List<string> SpecifiedRoles { get; set; }
}
