using H.LowCode.MetaSchema.DesignEngine;

namespace H.LowCode.DesignEngine.Domain.Repositories;

public interface IComponentLibraryRepository
{
    Task<List<ComponentLibrarySchema>> GetListAsync();

    Task<ComponentLibrarySchema> GetByIdAsync(string libraryId);

    Task<bool> SaveAsync(ComponentLibrarySchema componentLibrary);

    Task<bool> DeleteAsync(string libraryId);
}
