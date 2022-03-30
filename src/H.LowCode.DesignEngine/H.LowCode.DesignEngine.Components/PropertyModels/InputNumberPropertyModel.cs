using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace H.LowCode.DesignEngine.Components.PropertyModels
{
    internal class InputNumberPropertyModel : ComponentPropertyModelBase
    {
        private static readonly List<string> _properties = new() { "Maximum", "Minimum" };

        public override bool IsShowProperty(string propertyName)
        {
            if (_properties.Contains(propertyName))
                return true;
            return false;
        }
    }
}
