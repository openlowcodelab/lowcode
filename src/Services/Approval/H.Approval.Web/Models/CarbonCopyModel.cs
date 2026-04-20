using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace H.Approval.Web;

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
        SpecifiedRoles = new List<string>();
    }
    
    /// <summary>
    /// 抄送人类型
    /// </summary>
    public CarbonCopyTypeEnum CarbonCopyType { get; set; }
    
    /// <summary>
    /// 指定成员ID列表
    /// </summary>
    public List<string> SpecifiedUsers { get; set; }
    
    /// <summary>
    /// 指定角色ID列表
    /// </summary>
    public List<string> SpecifiedRoles { get; set; }
}
