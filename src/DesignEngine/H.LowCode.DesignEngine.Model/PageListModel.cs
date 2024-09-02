using H.LowCode.MetaSchema;
using System;

namespace H.LowCode.DesignEngine.Model;

public class PageListModel
{
    public string PageId { get; set; }

    public string PageName { get; set; }

    public int Order { get; set; }

    public PageTypeEnum PageType { get; set; }

    public int PublishState { get; set; }

    public DateTime ModifiedTime { get; set; }
}
