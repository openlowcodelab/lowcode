using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace H.LowCode.WorkflowDesigner.Models
{
    /// <summary>
    /// 节点基类
    /// </summary>
    public abstract class NodeModelBase
    {
        public NodeModelBase()
        {
            ChildNodes = new List<NodeModelBase>();
            ConditionNodes = new List<NodeModelBase>();
        }

        public string Id { get; set; }

        public string NodeName {  get; set; }

        public NodeTypeEnum NodeType {  get; set; }

        public bool IsInput { get; set; }

        public List<NodeModelBase> ChildNodes {  get; set; }

        public List<NodeModelBase> ConditionNodes { get; set; }
    }

    public enum NodeTypeEnum
    {
        Start = 0,
        Approve = 1,
        CarbonCopy = 2,
        Condition = 3,
        Branch = 4
    }
}
