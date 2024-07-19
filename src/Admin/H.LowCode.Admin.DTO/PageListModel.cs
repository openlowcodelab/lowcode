using System.ComponentModel;

namespace H.LowCode.Admin.DTO
{
    public class PageListModel
    {
        [DisplayName("页面编码")]
        public string PageId { get; set; }

        [DisplayName("页面名称")]
        public string PageName { get; set; }

        [DisplayName("页面状态")]
        public string PageState {  get; set; }

        [DisplayName("修改时间")]
        public DateTime ModifiedTime { get; set; }
    }
}
