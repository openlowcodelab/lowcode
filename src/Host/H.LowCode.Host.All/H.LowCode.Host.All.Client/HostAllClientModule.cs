using Volo.Abp.Autofac.WebAssembly;
using Volo.Abp.Modularity;

namespace H.LowCode.Host.All.Client;

[DependsOn(
    typeof(AbpAutofacWebAssemblyModule)
)]
public class HostAllClientModule : AbpModule
{
}
