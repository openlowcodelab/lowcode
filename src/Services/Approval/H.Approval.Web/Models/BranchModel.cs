using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace H.Approval.Web;

/// <summary>
/// ����˽ڵ�
/// </summary>
public class BranchModel : NodeModelBase
{
    public BranchModel()
    {
        NodeType = NodeTypeEnum.Branch;
    }
}
