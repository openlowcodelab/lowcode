using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace H.LowCode.DesignerEngine.PropertyPanel
{
    public class ComponentPropertyBaseModel
    {
        public string ID { get; set; }

        public string Title {  get; set; }

        /// <summary>
        /// 字段描述
        /// </summary>
        public string Description {  get; set; }

        public string DefaultValue { get; set; }

        public bool IsRequired {  get; set; }

        /// <summary>
        /// 元素宽度
        /// </summary>
        public double ItemWidth { get; set; } = 180;

        /// <summary>
        /// 标签宽度
        /// </summary>
        public double LabelWidth { get; set; } = 180;


    }
}
