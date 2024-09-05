using H.LowCode.MetaSchema;
using System;
using System.Text.Json.Serialization;

namespace H.LowCode.MetaSchema;

public class PageModel
{
    public string AppId { get; set; }

    public string PageId { get; set; }

    public string PageName { get; set; }

    /// <summary>
    /// 页面布局（1:一列、2:二列、3:三列、4:四列）
    /// </summary>
    [JsonPropertyName("playout")]
    public int PageLayout { get; set; } = 2;
}
