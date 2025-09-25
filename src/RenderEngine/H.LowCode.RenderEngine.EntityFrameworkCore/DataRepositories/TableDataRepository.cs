using H.LowCode.Application.Contracts;
using H.LowCode.RenderEngine.Domain;
using H.LowCode.RenderEngine.Domain.Repositories;
using Microsoft.EntityFrameworkCore;
using Volo.Abp.Application.Dtos;

namespace H.LowCode.RenderEngine.EntityFrameworkCore;

public class TableDataRepository : ITableDataRepository
{
    private readonly IDbContextFactory<RenderEngineDbContext> _dbContextFactory;
    private readonly IDataSourceRepository _dataSourceRepository;
    public bool? IsChangeTrackingEnabled => true;

    public TableDataRepository(IDbContextFactory<RenderEngineDbContext> dbContextFactory, IDataSourceRepository dataSourceRepository)
    {
        _dbContextFactory = dbContextFactory;
        _dataSourceRepository = dataSourceRepository;
    }

    /// <summary>
    /// 获取表格数据列表
    /// </summary>
    /// <param name="input">查询参数</param>
    /// <returns>分页数据结果</returns>
    public async Task<PagedResultDto<Dictionary<string, object>>> GetListAsync(TableDataInput input)
    {
        var dataSource = await _dataSourceRepository.GetAsync(input.AppId, input.DataSourceId);
        if (dataSource == null)
        {
            return new();
        }

        // 使用 DbContextFactory 创建新的 DbContext 实例，确保线程安全
        using var dbContext = await _dbContextFactory.CreateDbContextAsync();

        // 获取实体类型（使用数据源名称）
        var entityType = dbContext.GetEntityType(dataSource.Name);

        // 获取DbSet
        var dbSetProperty = dbContext.GetType().GetMethod("Set", Type.EmptyTypes)?.MakeGenericMethod(entityType);
        var dbSet = dbSetProperty?.Invoke(dbContext, null) as IQueryable<object>;

        if (dbSet == null)
        {
            return new();
        }

        // 应用筛选条件
        if (input.Filters != null && input.Filters.Any())
        {
            // 这里可以根据需要实现更复杂的筛选逻辑
            // 暂时跳过筛选实现
        }

        // 应用排序
        if (!string.IsNullOrEmpty(input.Sorting))
        {
            var parameter = System.Linq.Expressions.Expression.Parameter(entityType, "x");
            var property = System.Linq.Expressions.Expression.Property(parameter, input.Sorting);
            var lambda = System.Linq.Expressions.Expression.Lambda(property, parameter);

            var orderByMethod = input.Sorting?.ToLower() == "desc" ? "OrderByDescending" : "OrderBy";
            var orderByExpression = System.Linq.Expressions.Expression.Call(
                typeof(Queryable),
                orderByMethod,
                new Type[] { entityType, property.Type },
                dbSet.Expression,
                System.Linq.Expressions.Expression.Quote(lambda));

            dbSet = dbSet.Provider.CreateQuery<object>(orderByExpression);
        }

        // 获取总数
        var totalCount = await dbSet.CountAsync();

        // 应用分页
        var pagedData = await dbSet
            .Skip(input.SkipCount)
            .Take(input.MaxResultCount)
            .ToListAsync();

        // 转换为字典格式
        var result = new List<Dictionary<string, object>>();
        foreach (var item in pagedData)
        {
            var dict = new Dictionary<string, object>();
            var properties = entityType.GetProperties();
            foreach (var prop in properties)
            {
                var value = prop.GetValue(item);
                dict[prop.Name] = value;
            }
            result.Add(dict);
        }

        return new()
        {
            Items = result,
            TotalCount = totalCount
        };
    }
}
