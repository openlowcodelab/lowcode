using H.Extensions.System;
using H.LowCode.Configuration;
using H.LowCode.MetaSchema;
using H.LowCode.Workbench.Application.Contracts;
using Microsoft.Extensions.Options;
using System.Text;

namespace H.LowCode.Workbench.Application;

public class WorkbenchAppService : IWorkbenchAppService
{
    private static string metaBaseDir;

    public WorkbenchAppService(IOptions<MetaOption> metaOption)
    {
        metaBaseDir = metaOption.Value.AppsFilePath;
    }
}
