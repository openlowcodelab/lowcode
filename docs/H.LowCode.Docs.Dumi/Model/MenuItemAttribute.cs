using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace H.LowCode.Docs.Dumi.Model
{
    public class MenuItemAttribute : Attribute
    {
        public MenuItemAttribute(int Order)
        {
            this.Order = Order;
        }

        public int Order { get; set; }
    }
}
