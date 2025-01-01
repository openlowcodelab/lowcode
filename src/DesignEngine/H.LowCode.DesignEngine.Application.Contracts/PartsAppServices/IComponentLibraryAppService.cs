using H.LowCode.DesignEngine.Model;
using H.LowCode.PartsMetaSchema;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Volo.Abp.Application.Services;

namespace H.LowCode.DesignEngine.Application.Contracts;

public interface IComponentLibraryAppService : IApplicationService
{
    Task<List<ComponentLibrarySchema>> GetListAsync();

    Task<ComponentLibrarySchema> GetByIdAsync(string libraryId);

    Task<bool> SaveAsync(ComponentLibrarySchema componentLibrary);

    Task<bool> DeleteAsync(string libraryId);
}
