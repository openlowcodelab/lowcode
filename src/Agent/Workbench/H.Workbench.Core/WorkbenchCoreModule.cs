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
        context.Services.AddSingleton<Tools.WorkspaceShellTool>();
        context.Services.AddSingleton<Tools.TestRunnerTool>();
        context.Services.AddSingleton<Tools.WorkspaceBuildTool>();
        context.Services.AddSingleton<Tools.OfficeDocumentTool>();
        context.Services.AddSingleton<Tools.SpreadsheetTool>();
        context.Services.AddSingleton<Tools.NotifyTool>();
        // 注册本身跨平台安全（构造里没有 Win32 调用），方法内部按操作系统拒绝，
        // 这样非 Windows 宿主仍能加载模块，只是该工具不可用
#pragma warning disable CA1416
        context.Services.AddSingleton<Tools.ComputerControlTool>();
#pragma warning restore CA1416
        // 浏览器会话必须活过工具实例的重建（ToolRegistry 每次注册都新建工具实例），
        // 所以它挂在池上，而池是单例
        context.Services.AddSingleton<Tools.Internal.BrowserSessionPool>();
        context.Services.AddSingleton<Tools.Internal.GitWorkspaceLocks>();
        context.Services.AddSingleton<Agents.ApprovalGateway>();
    }
}
