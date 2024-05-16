using System;

namespace H.LowCode.MetaSchema
{
    public class SettingItemSchema
    {
        public string Label { get; set; }

        public object Value { get; set; }

        public string Description { get; set; }

        public string Category { get; set; }

        /// <summary>
        /// 设置项类型
        /// </summary>
        public SettingItemTypeEnum SettingItemType { get; set; }
    }

    public enum SettingItemTypeEnum
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
