using System;

namespace H.LowCode.Metadata
{
    public interface IComponentProvider
    {
        public string Title { get; set; }

        IEnumerable<ComponentSchema> LoadComponent();
    }
}
