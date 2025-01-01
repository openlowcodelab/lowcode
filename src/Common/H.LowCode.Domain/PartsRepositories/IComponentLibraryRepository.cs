using H.LowCode.DesignEngine.Model;
using H.LowCode.PartsMetaSchema;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace H.LowCode.Domain.Repositories;

public interface IComponentLibraryRepository
{
    Task<List<ComponentLibrarySchema>> GetListAsync();

    Task<ComponentLibrarySchema> GetByIdAsync(string libraryId);

    Task<bool> SaveAsync(ComponentLibrarySchema componentLibrary);

    Task<bool> DeleteAsync(string libraryId);
}
