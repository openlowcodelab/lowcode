using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace H.LowCode.Docs.Dumi.Model
{
    public class MenuItemAttribute : Attribute
    {
        public MenuItemAttribute(string title, int order)
        {
            if(string.IsNullOrEmpty(title))
            {
                throw new ArgumentNullException(nameof(title));
            }
            this.Title = title;
            this.Order = order;
        }

        public string Title { get; set; }

        public int Order { get; set; }
    }
}
