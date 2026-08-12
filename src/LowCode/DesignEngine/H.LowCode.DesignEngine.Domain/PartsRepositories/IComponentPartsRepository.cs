using H.LowCode.DesignEngine.Model;
using H.LowCode.MetaSchema.DesignEngine;

namespace H.LowCode.DesignEngine.Domain.Repositories;

public interface IComponentPartsRepository
{
    Task<List<ComponentPartsListModel>> GetListAsync(string libraryId);

    Task<List<ComponentPartsSchema>> GetAllComponentsAsync(string libraryId);

    Task<ComponentPartsSchema> GetByIdAsync(string libraryId, string partsId);

    Task<bool> SaveAsync(ComponentPartsSchema componentParts);

    Task<bool> DeleteAsync(string libraryId, string partsId);
}
