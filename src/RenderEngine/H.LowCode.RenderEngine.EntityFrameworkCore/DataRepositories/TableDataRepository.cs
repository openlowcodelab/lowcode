using H.LowCode.RenderEngine.Application.Contracts;
using H.LowCode.RenderEngine.Domain;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace H.LowCode.RenderEngine.EntityFrameworkCore;

public class TableDataRepository : ITableDataRepository
{
    private readonly IDbContextFactory<RenderEngineDbContext> _dbContextFactory;
    private readonly IDataSourceDomainService _dataSourceDomainService;
    public bool? IsChangeTrackingEnabled => true;

    public TableDataRepository(IDbContextFactory<RenderEngineDbContext> dbContextFactory, IDataSourceDomainService dataSourceDomainService)
    {
        _dbContextFactory = dbContextFactory;
        _dataSourceDomainService = dataSourceDomainService;
    }

    /// <summary>
    /// 获取表格数据列表
    /// </summary>
    /// <param name="input">查询参数</param>
    /// <returns>分页数据结果</returns>
    public async Task<TableGetListOutput> GetListAsync(TableGetListInput input)
    {
        try
        {
            // 根据数据源ID获取数据源信息
            var dataSource = await _dataSourceDomainService.GetAsync(input.AppId, input.DataSourceId);
            if (dataSource == null)
            {
                return new TableGetListOutput
                {
                    Items = new List<Dictionary<string, object>>(),
                    TotalCount = 0,
                    PageIndex = input.PageIndex,
                    PageSize = input.PageSize
                };
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
                return new TableGetListOutput
                {
                    Items = new List<Dictionary<string, object>>(),
                    TotalCount = 0,
                    PageIndex = input.PageIndex,
                    PageSize = input.PageSize
                };
            }

            // 应用筛选条件
            if (input.Filters != null && input.Filters.Any())
            {
                // 这里可以根据需要实现更复杂的筛选逻辑
                // 暂时跳过筛选实现
            }

            // 应用排序
            if (!string.IsNullOrEmpty(input.SortField))
            {
                var parameter = System.Linq.Expressions.Expression.Parameter(entityType, "x");
                var property = System.Linq.Expressions.Expression.Property(parameter, input.SortField);
                var lambda = System.Linq.Expressions.Expression.Lambda(property, parameter);
                
                var orderByMethod = input.SortOrder?.ToLower() == "desc" ? "OrderByDescending" : "OrderBy";
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
                .Skip((input.PageIndex - 1) * input.PageSize)
                .Take(input.PageSize)
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

            // 计算分页信息
            var totalPages = (int)Math.Ceiling((double)totalCount / input.PageSize);
            var hasNextPage = input.PageIndex < totalPages;
            var hasPreviousPage = input.PageIndex > 1;

            return new TableGetListOutput
            {
                Items = result,
                TotalCount = totalCount,
                PageIndex = input.PageIndex,
                PageSize = input.PageSize
            };
        }
        catch (Exception ex)
        {
            // 记录详细错误信息，便于调试
            Console.WriteLine($"获取表格数据失败: {ex.Message}");
            Console.WriteLine($"堆栈跟踪: {ex.StackTrace}");
            
            // 返回空结果，避免异常中断
            return new TableGetListOutput
            {
                Items = new List<Dictionary<string, object>>(),
                TotalCount = 0,
                PageIndex = input.PageIndex,
                PageSize = input.PageSize
            };
        }
    }
}
