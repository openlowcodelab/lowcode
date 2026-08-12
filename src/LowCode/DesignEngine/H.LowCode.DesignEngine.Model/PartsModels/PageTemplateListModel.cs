using H.LowCode.MetaSchema;
using H.LowCode.MetaSchema.DesignEngine;
using System.Text.Json.Serialization;

namespace H.LowCode.DesignEngine.Model;

public class PageTemplateListModel
{
    public string TemplateId { get; set; }

    public string Name { get; set; }

    public PageTemplateCategoryEnum Category { get; set; }

    public PageTypeEnum PageType { get; set; }

    public string Description { get; set; }

    public int Order { get; set; }

    [JsonPropertyName("pub")]
    public int PublishStatus { get; set; }

    public DateTime ModifiedTime { get; set; }
}
