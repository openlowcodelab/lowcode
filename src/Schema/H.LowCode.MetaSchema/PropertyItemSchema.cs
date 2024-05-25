using System;

namespace H.LowCode.MetaSchema
{
    public class PropertyItemSchema
    {
        public string Label { get; set; }

        public string StringValue { get; set; }

        public bool BoolValue { get; set; }

        public int IntValue { get; set; }

        public string Description { get; set; }

        public string Category { get; set; }

        /// <summary>
        /// 设置项类型
        /// </summary>
        public PropertyItemTypeEnum SettingItemType { get; set; }
    }

    public enum PropertyItemTypeEnum
    {
        Text,
        Text_Int,
        Radio,
        Checkbox,
        Select,
        Switch,
        Date,
        Textarea,
        Options
    }
}
