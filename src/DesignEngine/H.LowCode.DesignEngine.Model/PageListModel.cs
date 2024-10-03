using H.LowCode.MetaSchema;
using System;
using System.Text.Json.Serialization;

namespace H.LowCode.DesignEngine.Model;

public class PageListModel
{
    public string PageId { get; set; }

    public string PageName { get; set; }

    public int Order { get; set; }

    public PageTypeEnum PageType { get; set; }

    [JsonPropertyName("pubstatus")]
    public int PublishStatus { get; set; }

    public DateTime ModifiedTime { get; set; }
}
