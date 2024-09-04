using H.LowCode.MetaSchema;
using System;

namespace H.LowCode.DesignEngine.Model;

public class PageModel
{
    public string AppId { get; set; }

    public string PageId { get; set; }

    public string PageName { get; set; }

    public PagePropertySchema PageProperty { get; set; } = new();
}
