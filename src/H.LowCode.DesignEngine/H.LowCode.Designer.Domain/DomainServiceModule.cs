using H.LowCode.Designer.Domain.DomainService;
using H.LowCode.Designer.Repository;
using H.LowCode.Designer.Repository.JsonFile;
using H.LowCode.Designer.Repository.MongoDB;
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
    [DependsOn(typeof(JsonFileRepositoryModule), typeof(MongoDBRepositoryModule))]
    public class DomainServiceModule : AbpModule
    {
        public override void ConfigureServices(ServiceConfigurationContext context)
        {
            context.Services.AddScoped<IDesignerDomainService, DesignerDomainService>();
        }
    }
}
