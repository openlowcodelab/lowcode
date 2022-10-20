using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace H.LowCode.Metadata.Components
{
    public interface IComponentProvider
    {
        public string Title { get; set; }

        IEnumerable<ComponentSchema> LoadComponent();
    }
}
