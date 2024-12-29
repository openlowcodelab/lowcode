using H.LowCode.MetaSchema;
using H.LowCode.PartsMetaSchema;

namespace H.LowCode.DesignEngine.Abstraction;

public interface IPageTemplateProvider
{
    public string Label { get; set; }

    IEnumerable<ComponentPartsSchema> LoadPageTemplate();

    IEnumerable<ComponentPartsSchema> LoadPageTemplates();
}
