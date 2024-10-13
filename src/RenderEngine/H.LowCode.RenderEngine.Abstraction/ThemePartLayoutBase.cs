using H.LowCode.ComponentBase;
using H.LowCode.MetaSchema;
using Microsoft.AspNetCore.Components;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text;
using System.Threading.Tasks;

namespace H.LowCode.RenderEngine.Abstraction;

public abstract class ThemePartLayoutBase : LowCodeLayoutComponentBase
{
    [Inject]
    private IHttpClientFactory HttpClientFactory { get; set; }

    [Inject]
    private NavigationManager NavigationManager { get; set; }

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

    private async Task<List<MenuSchema>> GetMenuListAsync(string appId)
    {
        var httpClient = HttpClientFactory.CreateClient();
        httpClient.BaseAddress = base.GetBaseUri();

        var result = await httpClient.GetFromJsonAsync<List<MenuSchema>>($"api/renderengine/getmenus?appId={appId}");
        return result;
    }
}
