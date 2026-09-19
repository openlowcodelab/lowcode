using H.Workbench.Core.Tools;
using Microsoft.Extensions.DependencyInjection;
using Volo.Abp.Modularity;

namespace H.Workbench.Core;

public class WorkbenchCoreModule : AbpModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        var configuration = context.Services.GetConfiguration();
        var section = configuration.GetSection(WorkbenchToolOptions.SectionName);
        Configure<WorkbenchToolOptions>(options =>
        {
            Microsoft.Extensions.Configuration.ConfigurationBinder.Bind(section, options);
        });

        // 工具实例被单例 ToolRegistry 捕获，只能是单例（红线：不得依赖 scoped 服务）
        context.Services.AddSingleton<GitTool>();
        context.Services.AddSingleton<WorkspaceFileTool>();
        context.Services.AddSingleton<Tools.Internal.GitWorkspaceLocks>();
    }
}
