using H.LowCode.ComponentBase;
using H.LowCode.MetaSchema;
using H.LowCode.RenderEngine.Application.Contracts;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Configuration;
using System;
using System.Linq;
using System.Text;

namespace H.LowCode.RenderEngine.Abstraction;

public abstract class ThemePartLayoutBase : LowCodeLayoutComponentBase
{
    [Inject]
    private IMetaAppService MetaAppService { get; set; }

    [Inject]
    private NavigationManager NavigationManager { get; set; }

    [Inject]
    private IConfiguration Configuration { get; set; }

    protected async Task<IList<MenuSchema>> GetMenusAsync(string appId)
    {
        var menus = await GetMenuListAsync(appId);

        string IndexUrl = "index";
        if (menus.Any(t => string.Equals(t.MenuUrl, IndexUrl, StringComparison.OrdinalIgnoreCase)) == false)
        {
            menus.Insert(0, new MenuSchema
            {
                MenuUrl = IndexUrl,
                Title = "首页",
                Id = IndexUrl
            });
        }
        return menus;
    }

    private async Task<IList<MenuSchema>> GetMenuListAsync(string appId)
    {
        return await MetaAppService.GetMenusAsync(appId);
    }
}
