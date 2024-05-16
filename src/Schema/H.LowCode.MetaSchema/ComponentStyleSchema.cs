using System;
using System.Text.Json.Serialization;

namespace H.LowCode.MetaSchema
{
    public class ComponentStyleSchema
    {
        /// <summary>
        /// 组件宽度（默认12：12/24*100 = 50%）
        /// </summary>
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
        public double ItemWidth { get; set; } = 12;

        /// <summary>
        /// 组件高度（默认 85 px）
        /// </summary>
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
        public double ItemHeight { get; set; } = 85;

        /// <summary>
        /// 标签宽度（默认 180 px）
        /// </summary>
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
        public double LabelWidth { get; set; } = 180;

        /// <summary>
        /// 默认样式
        /// </summary>
        public string DefaultStyle { get; set; }

        private string _display = "inline";
        public string Display
        {
            get { return $"display:{_display}"; }
            set { _display = value; }
        }

        private string _position = "static";
        public string Position
        {
            get { return $"position:{_position}"; }
            set { _position = value; }
        }

        /// <summary>
        /// 自定义样式
        /// </summary>
        public string CustomStyle { get; set; }
    }
}
