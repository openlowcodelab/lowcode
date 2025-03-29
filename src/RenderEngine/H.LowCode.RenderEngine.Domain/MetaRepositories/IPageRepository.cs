using H.LowCode.MetaSchema.RenderEngine;

namespace H.LowCode.RenderEngine.Domain.Repositories;

public interface IPageRepository
{
    Task<PageSchema> GetAsync(string appId, string pageId);
}