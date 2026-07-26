using System;
using System.Text.Json.Serialization;

namespace H.LowCode.DesignEngine.Model;

public class AppTemplateListModel
{
    public string TemplateId { get; set; }

    public string Name { get; set; }

    public string Description { get; set; }

    public string Icon { get; set; }

    public int PageCount { get; set; }

    public int Order { get; set; }

    [JsonPropertyName("pub")]
    public int PublishStatus { get; set; }

    public DateTime ModifiedTime { get; set; }
}
