using H.Extensions.System;
using H.LowCode.Domain;
using H.LowCode.MetaSchema;
using H.LowCode.RenderEngine.Application.Contracts;
using Microsoft.Extensions.DependencyInjection;
using System.Text;
using System.Xml.Linq;
using Volo.Abp;
using Volo.Abp.Application.Services;

namespace H.LowCode.RenderEngine.Application;

[RemoteService]
public class MetaAppService : ApplicationService, IMetaAppService
{
    private IMenuDomainService _menuDomainService => LazyServiceProvider.GetRequiredService<IMenuDomainService>();
    private IPageDomainService _pageDomainService => LazyServiceProvider.GetRequiredService<IPageDomainService>();

    public async Task<IList<MenuSchema>> GetMenusAsync(string appId)
    {
        return await _menuDomainService.GetListAsync(appId);
    }

    public async Task<PageSchema> GetPageAsync(string appId, string pageId)
    {
        return await _pageDomainService.GetAsync(appId, pageId);
    }
}
