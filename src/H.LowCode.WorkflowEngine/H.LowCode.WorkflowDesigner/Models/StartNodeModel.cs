using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace H.LowCode.WorkflowDesigner.Models
{
    /// <summary>
    /// 发起人节点
    /// </summary>
    internal class StartNodeModel : NodeModelBase
    {
        public StartNodeModel()
        {
            NodeType = NodeTypeEnum.Start;
        }
    }
}
