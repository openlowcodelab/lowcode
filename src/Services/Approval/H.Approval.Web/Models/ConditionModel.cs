using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace H.Approval.Web;

/// <summary>
/// �����ڵ�
/// </summary>
public class ConditionModel : NodeModelBase
{
    public ConditionModel()
    {
        NodeType = NodeTypeEnum.Condition;
    }
}
