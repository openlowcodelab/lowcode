using H.LowCode.Designer.Repository.Abstraction;
using H.LowCode.Designer.Repository.JsonFile.Repositories;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Volo.Abp.Modularity;

namespace H.LowCode.Designer.Repository.JsonFile
{
    public class JsonFileRepositoryModule : AbpModule
    {
        public override void ConfigureServices(ServiceConfigurationContext context)
        {
            context.Services.AddScoped<IDesignerRepository, DesignerJsonFileRepository>();
        }
    }
}
