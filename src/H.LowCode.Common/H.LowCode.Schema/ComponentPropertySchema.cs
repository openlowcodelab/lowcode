using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace H.LowCode.Schema
{
    public class ComponentPropertySchema
    {
        public bool IsRequired { get; set; }

        public string DefaultValue { get; set; }

        /// <summary>
        /// 组件宽度
        /// </summary>
        public double ItemWidth { get; set; } = 12;

        /// <summary>
        /// 标签宽度
        /// </summary>
        public double LabelWidth { get; set; } = 180;

        /// <summary>
        /// 自定义样式
        /// </summary>
        public string CustomStyle { get; set; }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="propertyName"></param>
        /// <returns></returns>
        public virtual bool IsShowProperty(string propertyName)
        {
            return false;
        }
    }
}
