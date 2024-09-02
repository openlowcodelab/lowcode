using H.LowCode.MetaSchema;
using System;

namespace H.LowCode.Workbench.Application.Contracts;

public interface IWorkbenchAppService
{
    Task<IList<AppSchema>> GetAppsAsync();

    Task SaveAppAsync(AppSchema app);

}