using H.Portal.Application.Contracts;
using Volo.Abp.Modularity;

namespace H.Portal.Web;

[DependsOn(
    typeof(PortalApplicationContractsModule)
)]
public class PortalWebModule : AbpModule
{
}
