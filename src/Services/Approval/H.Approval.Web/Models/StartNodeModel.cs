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
    }
}
