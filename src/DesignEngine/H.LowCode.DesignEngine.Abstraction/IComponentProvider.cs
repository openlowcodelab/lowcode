using H.LowCode.MetaSchema;
using System;

namespace H.LowCode.DesignEngine.Abstraction
{
    public interface IComponentProvider
    {
        public string Title { get; set; }

        IEnumerable<ComponentSchema> LoadComponent();
    }
}
