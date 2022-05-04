using H.LowCode.Designer.Domain.DomainService;
using H.LowCode.Designer.Repository;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Volo.Abp;
using Volo.Abp.Modularity;

namespace H.LowCode.Designer.Domain
{
    public class DomainServiceModule : AbpModule
    {
        public override void ConfigureServices(ServiceConfigurationContext context)
        {
            context.Services.AddScoped<IDesignerDomainService, DesignerDomainService>();
        }
    }
}
