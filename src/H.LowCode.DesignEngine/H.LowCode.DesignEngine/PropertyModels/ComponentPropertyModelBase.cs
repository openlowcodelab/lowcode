using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace H.LowCode.DesignEngine.PropertyModels
{
    public abstract class ComponentPropertyModelBase
    {
        public string ID { get; set; }

        public bool IsRequired {  get; set; }

        public string DefaultValue { get; set; }

        /// <summary>
        /// 元素宽度
        /// </summary>
        public double ItemWidth { get; set; } = 180;

        /// <summary>
        /// 标签宽度
        /// </summary>
        public double LabelWidth { get; set; } = 180;

        public abstract bool IsShowProperty(string propertyName);
    }
}
