using System;

namespace H.LowCode.Schema
{
    public interface IComponentProvider
    {
        public string Title { get; set; }

        IEnumerable<ComponentSchema> LoadComponent();
    }
}
