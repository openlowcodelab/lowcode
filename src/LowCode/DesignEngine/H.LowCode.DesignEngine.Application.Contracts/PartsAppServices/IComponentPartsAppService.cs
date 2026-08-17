using H.Abp.Application.Contracts;
using H.LowCode.DesignEngine.Model;
using H.LowCode.MetaSchema.DesignEngine;
using H.Util.Base;

namespace H.LowCode.DesignEngine.Application.Contracts;

public interface IComponentPartsAppService : IAppService
{
    Task<BaseOutput<List<ComponentPartsListModel>>> GetListAsync(string libraryId);

    Task<BaseOutput<List<ComponentPartsSchema>>> GetAllComponentsAsync(string libraryId);

    Task<BaseOutput<ComponentPartsSchema>> GetByIdAsync(string libraryId, string partsId);

    Task<BaseOutput<bool>> SaveAsync(ComponentPartsSchema componentParts);

    Task<BaseOutput<bool>> DeleteAsync(string libraryId, string partsId);
}
