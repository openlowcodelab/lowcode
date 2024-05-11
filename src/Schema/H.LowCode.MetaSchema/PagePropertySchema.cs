using System;

namespace H.LowCode.MetaSchema
{
    public class PagePropertySchema
    {
        /// <summary>
        /// 页面布局（1:一列、2:二列、3:三列、4:四列）
        /// </summary>
        public int PageLayout { get; set; } = 2;

        /// <summary>
        /// 标题宽度
        /// </summary>
        public string TitleWidth { get; set; }
    }
}
