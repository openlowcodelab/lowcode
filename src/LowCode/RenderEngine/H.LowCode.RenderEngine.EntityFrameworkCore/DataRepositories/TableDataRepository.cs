using H.Abp.Application.Contracts;
using H.LowCode.Application.Contracts;
using H.LowCode.RenderEngine.Domain;
using H.LowCode.RenderEngine.Domain.Repositories;
using Microsoft.EntityFrameworkCore;

namespace H.LowCode.RenderEngine.EntityFrameworkCore;

public class TableDataRepository : ITableDataRepository
{
    private readonly IDbContextFactory<RenderEngineDbContext> _dbContextFactory;
    private readonly IDataSourceRepository _dataSourceRepository;
    public bool? IsChangeTrackingEnabled => true;

    public string? EntityName { get; set; }

    public string ProviderName => throw new NotImplementedException();

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
        if (string.IsNullOrEmpty(input.DataSourceId))
        {
            return new();
        }

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

        // 应用筛选条件（等值过滤）
        if (input.Filters != null && input.Filters.Any())
        {
            foreach (var filter in input.Filters)
            {
                var property = entityType.GetProperty(filter.Key);
                if (property == null || filter.Value == null)
                    continue;

                var filterValue = ConvertValue(filter.Value, property.PropertyType);
                if (filterValue == null)
                    continue;

                var parameter = System.Linq.Expressions.Expression.Parameter(entityType, "x");
                var member = System.Linq.Expressions.Expression.Property(parameter, property);
                var equal = System.Linq.Expressions.Expression.Equal(
                    member, System.Linq.Expressions.Expression.Constant(filterValue, property.PropertyType));
                var lambda = System.Linq.Expressions.Expression.Lambda(equal, parameter);

                var whereExpression = System.Linq.Expressions.Expression.Call(
                    typeof(Queryable), "Where",
                    new Type[] { entityType },
                    dbSet.Expression,
                    System.Linq.Expressions.Expression.Quote(lambda));

                dbSet = dbSet.Provider.CreateQuery<object>(whereExpression);
            }
        }

        // 应用排序（格式：字段名 或 "字段名 desc"）
        if (!string.IsNullOrEmpty(input.Sorting))
        {
            var sortingParts = input.Sorting.Trim().Split(' ', StringSplitOptions.RemoveEmptyEntries);
            var sortField = sortingParts[0];
            var sortDesc = sortingParts.Length > 1 && sortingParts[1].Equals("desc", StringComparison.OrdinalIgnoreCase);

            var parameter = System.Linq.Expressions.Expression.Parameter(entityType, "x");
            var property = System.Linq.Expressions.Expression.Property(parameter, sortField);
            var lambda = System.Linq.Expressions.Expression.Lambda(property, parameter);

            var orderByMethod = sortDesc ? "OrderByDescending" : "OrderBy";
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

    /// <summary>
    /// 删除数据
    /// </summary>
    /// <param name="request">删除请求参数</param>
    public async Task DeleteAsync(TableDataDeleteInput request)
    {
        var dataSource = await _dataSourceRepository.GetAsync(request.AppId, request.DataSourceId);
        if (dataSource == null)
        {
            throw new ArgumentException($"数据源不存在: {request.DataSourceId}");
        }

        // 使用 DbContextFactory 创建新的 DbContext 实例，确保线程安全
        using var dbContext = await _dbContextFactory.CreateDbContextAsync();

        // 获取实体类型（使用数据源名称）
        var entityType = dbContext.GetEntityType(dataSource.Name);

        // 根据ID查找要删除的实体
        var entity = await dbContext.FindAsync(entityType, request.Id);
        if (entity == null)
        {
            throw new ArgumentException($"要删除的记录不存在: {request.Id}");
        }

        // 删除实体
        dbContext.Remove(entity);
        await dbContext.SaveChangesAsync();
    }

    /// <summary>
    /// 更新数据
    /// </summary>
    /// <param name="request">更新请求参数</param>
    public async Task UpdateAsync(TableDataUpdateInput request)
    {
        var dataSource = await _dataSourceRepository.GetAsync(request.AppId, request.DataSourceId);
        if (dataSource == null)
        {
            throw new ArgumentException($"数据源不存在: {request.DataSourceId}");
        }

        // 使用 DbContextFactory 创建新的 DbContext 实例，确保线程安全
        using var dbContext = await _dbContextFactory.CreateDbContextAsync();

        // 获取实体类型（使用数据源名称）
        var entityType = dbContext.GetEntityType(dataSource.Name);

        // 根据ID查找要更新的实体
        var entity = await dbContext.FindAsync(entityType, request.Id);
        if (entity == null)
        {
            throw new ArgumentException($"要更新的记录不存在: {request.Id}");
        }

        // 更新实体属性
        ApplyRowDataToEntity(entity, entityType, request.UpdateData);

        // 保存更改
        await dbContext.SaveChangesAsync();
    }

    /// <summary>
    /// 保存行数据（按主键新增或更新），返回主键值
    /// </summary>
    public async Task<string> SaveAsync(TableDataSaveInput request)
    {
        var dataSource = await _dataSourceRepository.GetAsync(request.AppId, request.DataSourceId);
        if (dataSource == null)
        {
            throw new ArgumentException($"数据源不存在: {request.DataSourceId}");
        }

        var primaryKeyField = dataSource.TableFields?.FirstOrDefault(t => t.IsPrimaryKey);
        var primaryKeyName = primaryKeyField?.Name ?? "f_id";

        using var dbContext = await _dbContextFactory.CreateDbContextAsync();
        var entityType = dbContext.GetEntityType(dataSource.Name);

        // 解析主键值，主键为空或强制新增时自动生成
        string? primaryKey = null;
        if (!request.ForceInsert
            && request.RowData != null
            && request.RowData.TryGetValue(primaryKeyName, out var pkValue))
        {
            primaryKey = pkValue?.ToString();
        }

        if (string.IsNullOrEmpty(primaryKey))
        {
            primaryKey = Guid.NewGuid().ToString();
            request.RowData ??= new Dictionary<string, object>();
            request.RowData[primaryKeyName] = primaryKey;
        }

        // 主键存在则更新，否则新增
        var entity = await dbContext.FindAsync(entityType, primaryKey);
        if (entity == null)
        {
            entity = Activator.CreateInstance(entityType);
            ApplyRowDataToEntity(entity, entityType, request.RowData);
            dbContext.Add(entity);
        }
        else
        {
            ApplyRowDataToEntity(entity, entityType, request.RowData);
            dbContext.Update(entity);
        }

        await dbContext.SaveChangesAsync();
        return primaryKey;
    }

    /// <summary>
    /// 将行数据字典写入实体属性（按属性实际类型转换）
    /// </summary>
    private static void ApplyRowDataToEntity(object entity, Type entityType, Dictionary<string, object>? rowData)
    {
        if (rowData == null || rowData.Count == 0)
            return;

        var properties = entityType.GetProperties();
        foreach (var field in rowData)
        {
            var property = properties.FirstOrDefault(p =>
                string.Equals(p.Name, field.Key, StringComparison.OrdinalIgnoreCase));

            if (property == null || !property.CanWrite)
                continue;

            var value = ConvertValue(field.Value, property.PropertyType);
            if (value == null && property.PropertyType.IsValueType
                && Nullable.GetUnderlyingType(property.PropertyType) == null)
            {
                continue;
            }

            property.SetValue(entity, value);
        }
    }

    /// <summary>
    /// 值类型转换（兼容字符串形式的布尔/数值等）
    /// </summary>
    private static object? ConvertValue(object? value, Type targetType)
    {
        if (value == null)
            return null;

        var underlyingType = Nullable.GetUnderlyingType(targetType) ?? targetType;

        try
        {
            if (value.GetType() == underlyingType)
                return value;

            if (underlyingType == typeof(bool))
            {
                var str = value.ToString();
                if (str == "1" || str == "0")
                    return str == "1";
                return bool.Parse(str);
            }

            if (underlyingType.IsEnum)
                return Enum.Parse(underlyingType, value.ToString()!);

            return Convert.ChangeType(value, underlyingType);
        }
        catch
        {
            return null;
        }
    }
}
