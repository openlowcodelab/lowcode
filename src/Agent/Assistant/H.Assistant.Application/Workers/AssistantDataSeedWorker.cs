using H.Assistant.Data;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Volo.Abp.Data;
using Volo.Abp.Uow;

namespace H.Assistant.Application.Workers;

/// <summary>
/// 宿主启动时执行 Assistant 种子数据（技能/连接器/员工模板），各 Seeder 内部保证幂等
/// </summary>
public class AssistantDataSeedWorker : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;

    public AssistantDataSeedWorker(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        using var scope = _serviceProvider.CreateScope();
        var uowManager = scope.ServiceProvider.GetRequiredService<IUnitOfWorkManager>();
        using var uow = uowManager.Begin(requiresNew: true);
        var seeder = scope.ServiceProvider.GetRequiredService<AgentSkillDataSeeder>();
        await seeder.SeedAsync(new DataSeedContext());
        await uow.CompleteAsync();
    }
}
