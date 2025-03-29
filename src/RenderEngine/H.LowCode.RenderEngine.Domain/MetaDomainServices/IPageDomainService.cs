using H.LowCode.MetaSchema.RenderEngine;
using Volo.Abp.Domain.Services;

namespace H.LowCode.RenderEngine.Domain;

public interface IPageDomainService : IDomainService
{
    Task<PageSchema> GetAsync(string appId, string pageId);
}