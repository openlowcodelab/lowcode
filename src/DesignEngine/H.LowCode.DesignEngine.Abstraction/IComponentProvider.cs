using H.LowCode.MetaSchema;
using H.LowCode.PartsMetaSchema;

namespace H.LowCode.DesignEngine.Abstraction;

public interface IComponentProvider
{
    public string Name { get; }

    public string Label { get; }

    IEnumerable<ComponentPartsSchema> LoadComponents();
}
