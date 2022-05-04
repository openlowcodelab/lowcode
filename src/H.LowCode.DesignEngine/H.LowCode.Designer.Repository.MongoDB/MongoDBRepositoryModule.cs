using H.LowCode.Designer.Repository.Abstraction;
using H.LowCode.Designer.Repository.MongoDB.Repositories;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Volo.Abp.Modularity;

namespace H.LowCode.Designer.Repository.MongoDB
{
    public class MongoDBRepositoryModule : AbpModule
    {
        public override void ConfigureServices(ServiceConfigurationContext context)
        {
            context.Services.AddScoped<IDesignerRepository, DesignerMongoDBRepository>();
        }
    }
}
