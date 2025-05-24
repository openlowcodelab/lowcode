using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Volo.Abp.Modularity;
using Volo.Abp.Validation;

namespace H.LowCode.DesignEngine.Application.Contracts;

public class DesignEngineApplicationContractsModule : AbpModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        //TODO: 临时解决异常: Property accessor 'IsSecurityCritical' on object 'System.Reflection.RuntimeMethodInfo' threw the following exception:'The method or operation is not implemented.'
        //      System.Reflection.TargetInvocationException: Property accessor 'IsSecurityCritical' on object 'System.Reflection.RuntimeMethodInfo' threw the following exception:'The method or operation is not implemented.'
        Configure<AbpValidationOptions>(options =>
        {
            options.IgnoredTypes.Add(typeof(MethodBase));
            options.IgnoredTypes.Add(typeof(MethodInfo));
        });
    }
}
