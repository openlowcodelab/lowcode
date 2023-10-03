using System;

namespace H.LowCode.Metadata
{
    public class PageSettingSchema
    {
        /// <summary>
        /// 页面布局（一列、二列、三列）
        /// </summary>
        public PageLayoutEnum PageLayout { get; set; }

        /// <summary>
        /// 标题宽度
        /// </summary>
        public string TitleWidth { get; set; }
    }

    public enum PageLayoutEnum
    {
        OneColumn = 0,
        TwoColumn = 1,
        ThreeColumn = 2
    }
}
