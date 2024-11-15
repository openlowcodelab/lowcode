using H.Extensions.System;
using H.LowCode.Configuration;
using H.LowCode.Domain;
using H.LowCode.MetaSchema;
using H.LowCode.RenderEngine.Application.Contracts;
using Microsoft.Extensions.Options;
using System.Text;
using System.Xml.Linq;
using Volo.Abp;
using Volo.Abp.Application.Services;

namespace H.LowCode.RenderEngine.Application;

[RemoteService]
public class RenderEngineAppService : ApplicationService, IMetaAppService
{
    private IMenuDomainService _menuDomainService;
    private IPageDomainService _pageDomainService;

    public RenderEngineAppService(IMenuDomainService menuDomainService,
        IPageDomainService pageDomainService)
    {
        _menuDomainService = menuDomainService;
        _pageDomainService = pageDomainService;
    }

    public async Task<IList<MenuSchema>> GetMenusAsync(string appId)
    {
        return await _menuDomainService.GetListAsync(appId);
    }

    public async Task<PageSchema> GetPageAsync(string appId, string pageId)
    {
        return await _pageDomainService.GetAsync(appId, pageId);
    }
}
