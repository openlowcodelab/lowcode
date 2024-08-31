using H.LowCode.MetaSchema;

namespace H.LowCode.DesignEngine.Abstraction;

public interface IComponentProvider
{
    public string Title { get; set; }

    IEnumerable<ComponentSchema> LoadComponent();
}
