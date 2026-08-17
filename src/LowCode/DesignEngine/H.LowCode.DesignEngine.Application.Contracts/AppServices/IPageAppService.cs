using H.Abp.Application.Contracts;
using H.LowCode.DesignEngine.Model;
using H.LowCode.MetaSchema.DesignEngine;
using H.Util.Base;

namespace H.LowCode.DesignEngine.Application.Contracts;

public interface IPageAppService : IAppService
{
    Task<BaseOutput<List<PageListModel>>> GetListAsync(string appId);

    Task<BaseOutput<PagePartsSchema>> GetByIdAsync(string appId, string pageId);

    Task<BaseOutput<PagePartsSchema>> GetByIdWithDefineAsync(string appId, string pageId);

    Task<BaseOutput<bool>> SaveAsync(PagePartsSchema pageSchema);

    Task<BaseOutput<bool>> DeleteAsync(string appId, string pageId);

    Task<BaseOutput<ComponentPartsSchema>> GetPageComponentAsync(string appId, string pageId, string componentId);
}