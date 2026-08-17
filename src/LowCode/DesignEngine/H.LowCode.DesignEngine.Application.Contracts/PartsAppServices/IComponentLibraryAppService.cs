using H.Abp.Application.Contracts;
using H.LowCode.MetaSchema.DesignEngine;
using H.Util.Base;

namespace H.LowCode.DesignEngine.Application.Contracts;

public interface IComponentLibraryAppService : IAppService
{
    Task<BaseOutput<List<ComponentLibrarySchema>>> GetListAsync();

    Task<BaseOutput<ComponentLibrarySchema>> GetByIdAsync(string libraryId);

    Task<BaseOutput<bool>> SaveAsync(ComponentLibrarySchema componentLibrary);

    Task<BaseOutput<bool>> DeleteAsync(string libraryId);
}
