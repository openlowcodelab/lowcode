using System;

namespace H.LowCode.MetaSchema
{
    public interface IComponentProvider
    {
        public string Title { get; set; }

        IEnumerable<ComponentSchema> LoadComponent();
    }
}
