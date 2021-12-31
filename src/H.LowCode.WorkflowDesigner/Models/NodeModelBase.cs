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
            ChildrenNodes = new List<NodeModelBase>();
        }

        public string Id { get; set; }

        public string Name {  get; set; }

        public NodeTypeEnum NodeType {  get; set; }

        public List<NodeModelBase> ChildrenNodes {  get; set; }
    }

    public enum NodeTypeEnum
    {
        ApproveNodeType,
        CarbonCopyNodeType,
        ConditionNodeType
    }
}
