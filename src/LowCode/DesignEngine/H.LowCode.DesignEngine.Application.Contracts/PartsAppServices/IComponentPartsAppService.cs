using H.Abp.Application.Contracts;
using H.LowCode.DesignEngine.Model;
using H.LowCode.MetaSchema.DesignEngine;

namespace H.LowCode.DesignEngine.Application.Contracts;

public interface IComponentPartsAppService : IAppService
{
    Task<List<ComponentPartsListModel>> GetListAsync(string libraryId);

    Task<List<ComponentPartsSchema>> GetAllComponentsAsync(string libraryId);

    Task<ComponentPartsSchema> GetByIdAsync(string libraryId, string partsId);

    Task<bool> SaveAsync(ComponentPartsSchema componentParts);

    Task<bool> DeleteAsync(string libraryId, string partsId);
}
