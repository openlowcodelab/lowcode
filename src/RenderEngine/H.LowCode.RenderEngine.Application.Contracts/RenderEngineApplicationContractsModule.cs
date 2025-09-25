using H.LowCode.Application.Contracts;
using Volo.Abp.Modularity;

namespace H.LowCode.RenderEngine.Application.Contracts;

[DependsOn(
    typeof(LowCodeApplicationContractsModule)
    )]
public class RenderEngineApplicationContractsModule : AbpModule
{

}
