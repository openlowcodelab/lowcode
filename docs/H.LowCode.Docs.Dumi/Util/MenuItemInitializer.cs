using H.LowCode.Docs.Dumi.Model;
using Microsoft.AspNetCore.Components;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace H.LowCode.Docs.Dumi.Util
{
    internal static class MenuItemInitializer
    {
        internal static IList<MenuItem> menuItems = new List<MenuItem>();

        internal static void InitMenuItems(Assembly assembly)
        {
            var pageComponentTypes = assembly.ExportedTypes
                .Where(t => t.Namespace != null && (t.IsSubclassOf(typeof(ComponentBase))
                                                    && t.Namespace.Contains(".Pages")));

            foreach (var pageType in pageComponentTypes)
            {
                if (pageType.FullName == null)
                    continue;

                var routeAttributes = pageType.GetCustomAttributes<RouteAttribute>(inherit: false);
                if (!routeAttributes.Any())
                    continue;

                var menuItemAttribute = pageType.GetCustomAttribute<MenuItemAttribute>(inherit: false);
                if (menuItemAttribute == null)
                    continue;

                MenuItem menuItem = new MenuItem()
                {
                    Path = routeAttributes.First().Template,
                    Title = menuItemAttribute.Title,
                    Order = menuItemAttribute.Order
                };
                menuItems.Add(menuItem);
            }

            menuItems = menuItems.OrderBy(t => t.Order).ToList();
        }
    }
}
