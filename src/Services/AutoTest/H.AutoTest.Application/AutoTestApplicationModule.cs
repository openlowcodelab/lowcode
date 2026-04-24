using H.AutoTest.Application.Contracts;
using Volo.Abp.Modularity;

namespace H.AutoTest.Application;

/// <summary>
/// AutoTest 应用模块
/// </summary>
[DependsOn(typeof(AutoTestApplicationContractsModule))]
public class AutoTestApplicationModule : AbpModule
{
}
