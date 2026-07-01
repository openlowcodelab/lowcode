using H.Enterprise.EntityFrameworkCore.Entities;
using Microsoft.EntityFrameworkCore;
using Volo.Abp.Data;
using Volo.Abp.MultiTenancy;

namespace H.Enterprise.EntityFrameworkCore;

/// <summary>
/// 自定义 ITenantStore 实现，从 EnterpriseDbContext 读取租户配置
/// ABP 多租户框架通过此接口解析当前租户的连接字符串等信息
/// </summary>
public class EnterpriseTenantStore : ITenantStore
{
    private readonly EnterpriseDbContext _context;

    public EnterpriseTenantStore(EnterpriseDbContext context)
    {
        _context = context;
    }

    public TenantConfiguration? Find(Guid id)
    {
        var entity = _context.Enterprises
            .AsNoTracking()
            .FirstOrDefault(e => e.Id == id && e.IsActivated && e.Status == EnterpriseStatus.Active);

        return MapToTenantConfiguration(entity);
    }

    public TenantConfiguration? Find(string name)
    {
        var entity = _context.Enterprises
            .AsNoTracking()
            .FirstOrDefault(e => e.Name == name && e.IsActivated && e.Status == EnterpriseStatus.Active);

        return MapToTenantConfiguration(entity);
    }

    public async Task<TenantConfiguration?> FindAsync(Guid id)
    {
        var entity = await _context.Enterprises
            .AsNoTracking()
            .FirstOrDefaultAsync(e => e.Id == id && e.IsActivated && e.Status == EnterpriseStatus.Active);

        return MapToTenantConfiguration(entity);
    }

    public async Task<TenantConfiguration?> FindAsync(string name)
    {
        var entity = await _context.Enterprises
            .AsNoTracking()
            .FirstOrDefaultAsync(e => e.Name == name && e.IsActivated && e.Status == EnterpriseStatus.Active);

        return MapToTenantConfiguration(entity);
    }

    public async Task<IReadOnlyList<TenantConfiguration>> GetListAsync(bool includeDetails = false)
    {
        var entities = await _context.Enterprises
            .AsNoTracking()
            .Where(e => e.IsActivated && e.Status == EnterpriseStatus.Active)
            .ToListAsync();

        return entities.Select(e => MapToTenantConfiguration(e)!).Where(c => c != null).ToList();
    }

    private static TenantConfiguration? MapToTenantConfiguration(EnterpriseEntity? entity)
    {
        if (entity == null)
            return null;

        var config = new TenantConfiguration(entity.Id, entity.Name);

        // 独立数据库模式：设置自定义连接字符串
        // ABP 框架会优先使用租户级别的连接字符串
        if (entity.DatabaseMode == DatabaseMode.Independent && !string.IsNullOrEmpty(entity.ConnectionString))
        {
            config.ConnectionStrings = new ConnectionStrings();
            config.ConnectionStrings["Default"] = entity.ConnectionString;
            config.ConnectionStrings["OrganizationDb"] = entity.ConnectionString;
            config.ConnectionStrings["ApprovalDb"] = entity.ConnectionString;
        }

        return config;
    }
}
