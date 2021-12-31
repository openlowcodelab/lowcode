using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace H.LowCode.DesignerEngine.PropertyPanel
{
    public class PagePropertyBaseModel
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
