using H.LowCode.Entity;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Volo.Abp.Domain.Entities;
using Volo.Abp.Domain.Services;

namespace H.LowCode.RenderEngine.Domain;

public class FormDataDomainService : DomainService, IFormDataDomainService
{
    private IFormDataRepository _formDataRepository => LazyServiceProvider.GetRequiredService<IFormDataRepository>();
    private IPageDomainService _pageDomainService => LazyServiceProvider.GetRequiredService<IPageDomainService>();

    public async Task<FormEntity> GetAsync(string appId, string pageId, string id)
    {
        var formPageSchema = await _pageDomainService.GetAsync(appId, pageId);
        if (formPageSchema == null)
            throw new KeyNotFoundException($"page not found: appId={appId}, pageId={pageId}");

        string entityName = formPageSchema.DataSource.DataSourceValue;

        if (string.IsNullOrEmpty(id))
        {
            var defaultEntity = new FormEntity()
            {
                Name = entityName,
                Fields = formPageSchema.Components.Where(t => t.IsContainer == false)
                    .Select(t => new FormFieldEntity()
                    {
                        Name = t.Name,
                        TypeName = t.Fragment.ValueType,
                        Value = t.Fragment.GetDefaultValue()
                    }).ToList()
            };
            return defaultEntity;
        }

        var entity = await _formDataRepository.GetAsync(entityName, id);
        if (entity == null)
            throw new EntityNotFoundException($"Entity {entityName} Not Found: {id}");

        return entity;
    }

    public async Task<bool> SaveAsync(FormEntity entity)
    {
        // 验证实体
        if (entity == null)
            throw new ArgumentNullException(nameof(entity));

        // 设置默认表名
        entity.Name = entity.Name ?? "tb_test1";

        // 验证字段
        if (entity.Fields == null || !entity.Fields.Any())
            throw new ArgumentException("表单字段不能为空", nameof(entity));

        // 验证字段值类型
        foreach (var field in entity.Fields)
        {
            if (string.IsNullOrEmpty(field.Name))
                throw new ArgumentException("字段名称不能为空");

            if (string.IsNullOrEmpty(field.TypeName))
                throw new ArgumentException($"字段 {field.Name} 的类型不能为空");
        }

        // 应用字段验证和默认值
        await ApplyFieldValidationAndDefaults(entity);

        try
        {
            // 检查是否为新增记录（通过主键字段判断）
            string primaryKeyName = "f_id";
            var primaryKeyField = entity.Fields.FirstOrDefault(f => f.Name == primaryKeyName);
            
            bool isNewRecord = primaryKeyField == null || 
                              string.IsNullOrEmpty(primaryKeyField.Value?.ToString());

            if (isNewRecord)
            {
                // 新增记录 - 设置主键字段值
                if (primaryKeyField == null)
                {
                    // 如果主键字段不存在，创建一个
                    primaryKeyField = new FormFieldEntity
                    {
                        Name = primaryKeyName,
                        TypeName = typeof(string).FullName,
                        Value = Guid.NewGuid().ToString()
                    };
                    entity.Fields.Add(primaryKeyField);
                }
                else
                {
                    // 如果主键字段存在但值为空，设置新值
                    primaryKeyField.Value = Guid.NewGuid().ToString();
                }
                
                await _formDataRepository.AddAsync(entity);
            }
            else
            {
                // 更新记录
                await _formDataRepository.UpdateAsync(entity);
            }
            return true;
        }
        catch (Exception ex)
        {
            // 记录错误日志
            string entityId = "";
            try { entityId = entity.Id; } catch { }
            Logger.LogError(ex, "保存表单数据失败: EntityName={EntityName}, EntityId={EntityId}", entity.Name, entityId);
            throw;
        }
    }

    /// <summary>
    /// 应用字段验证和默认值设置
    /// </summary>
    private async Task ApplyFieldValidationAndDefaults(FormEntity entity)
    {
        // 获取页面配置以获取字段默认值定义
        var pageConfig = await GetPageConfigForEntity(entity.Name);
        
        foreach (var field in entity.Fields)
        {
            // 跳过主键字段的验证和默认值处理
            if (field.Name == "f_id")
                continue;

            // 获取字段类型
            var fieldType = Type.GetType(field.TypeName);
            if (fieldType == null)
                continue;

            // 检查字段是否有有效值
            bool hasValidValue = HasValidValue(field.Value, fieldType);

            if (!hasValidValue)
            {
                // 首先尝试从页面配置获取默认值
                var configDefaultValue = GetFieldDefaultValueFromConfig(field.Name, fieldType, pageConfig);
                if (configDefaultValue != null)
                {
                    field.Value = configDefaultValue;
                    Logger.LogInformation("Applied config default value for field '{FieldName}': {DefaultValue}", 
                        field.Name, configDefaultValue);
                }
                else
                {
                    // 如果没有配置默认值，使用类型默认值
                    field.Value = GetTypeDefaultValue(fieldType);
                    Logger.LogInformation("Applied type default value for field '{FieldName}': {DefaultValue}", 
                        field.Name, field.Value);
                }
            }

            // 验证字段值的有效性
            ValidateFieldValue(field, fieldType);
        }
    }

    /// <summary>
    /// 检查字段是否有有效值
    /// </summary>
    private bool HasValidValue(object value, Type fieldType)
    {
        if (value == null)
            return false;

        // 字符串类型：空字符串视为无效值
        if (fieldType == typeof(string))
            return !string.IsNullOrWhiteSpace(value.ToString());

        // 数值类型：0值可能是有效值，所以只检查null
        if (fieldType == typeof(int) || fieldType == typeof(int?) ||
            fieldType == typeof(long) || fieldType == typeof(long?) ||
            fieldType == typeof(decimal) || fieldType == typeof(decimal?))
            return true;

        // 布尔类型：false也是有效值
        if (fieldType == typeof(bool) || fieldType == typeof(bool?))
            return true;

        // DateTime类型：检查是否为默认值
        if (fieldType == typeof(DateTime) || fieldType == typeof(DateTime?))
        {
            if (value is DateTime dateValue)
                return dateValue != default(DateTime);
            return true;
        }

        // Guid类型：检查是否为空Guid
        if (fieldType == typeof(Guid) || fieldType == typeof(Guid?))
        {
            if (value is Guid guidValue)
                return guidValue != Guid.Empty;
            return true;
        }

        return true;
    }

    /// <summary>
    /// 获取类型的默认值
    /// </summary>
    private object GetTypeDefaultValue(Type type)
    {
        if (type == null)
            return null;

        // 处理可空类型
        if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Nullable<>))
        {
            return null;
        }

        // 处理字符串类型
        if (type == typeof(string))
            return string.Empty;

        // 处理基本数值类型
        if (type == typeof(int))
            return 0;
        if (type == typeof(long))
            return 0L;
        if (type == typeof(decimal))
            return 0m;
        if (type == typeof(double))
            return 0.0;
        if (type == typeof(float))
            return 0.0f;
        if (type == typeof(short))
            return (short)0;
        if (type == typeof(byte))
            return (byte)0;

        // 处理布尔类型
        if (type == typeof(bool))
            return false;

        // 处理日期时间类型
        if (type == typeof(DateTime))
            return DateTime.Now;
        if (type == typeof(DateTimeOffset))
            return DateTimeOffset.Now;

        // 处理Guid类型
        if (type == typeof(Guid))
            return Guid.NewGuid();

        // 处理集合类型
        if (type.IsArray)
        {
            return Array.CreateInstance(type.GetElementType(), 0);
        }

        if (type.IsGenericType)
        {
            var genericTypeDef = type.GetGenericTypeDefinition();
            if (genericTypeDef == typeof(List<>) || 
                genericTypeDef == typeof(IList<>) || 
                genericTypeDef == typeof(ICollection<>) ||
                genericTypeDef == typeof(IEnumerable<>))
            {
                return Activator.CreateInstance(typeof(List<>).MakeGenericType(type.GetGenericArguments()));
            }
        }

        // 对于其他值类型，返回类型的默认值
        if (type.IsValueType)
            return Activator.CreateInstance(type);

        // 其他引用类型返回null
        return null;
    }

    /// <summary>
    /// 验证字段值的有效性
    /// </summary>
    private void ValidateFieldValue(FormFieldEntity field, Type fieldType)
    {
        try
        {
            // 尝试转换值以验证类型兼容性
            if (field.Value != null)
            {
                Convert.ChangeType(field.Value, fieldType);
            }
        }
        catch (Exception ex)
        {
            throw new ArgumentException($"Field '{field.Name}' value '{field.Value}' is not compatible with type '{fieldType.Name}'", ex);
        }
    }

    /// <summary>
    /// 获取页面配置
    /// </summary>
    private async Task<object> GetPageConfigForEntity(string entityName)
    {
        try
        {
            // 通过实体名称查找对应的页面配置
            // 这里简化处理，实际应该通过数据源配置来查找页面
            // TODO: 实现更精确的页面配置查找逻辑
            return null;
        }
        catch (Exception ex)
        {
            Logger.LogWarning(ex, "Failed to get page config for entity '{EntityName}'", entityName);
            return null;
        }
    }

    /// <summary>
    /// 从页面配置中获取字段默认值
    /// </summary>
    private object GetFieldDefaultValueFromConfig(string fieldName, Type fieldType, object pageConfig)
    {
        if (pageConfig == null)
            return null;

        try
        {
            // TODO: 实现从页面配置JSON中解析字段默认值的逻辑
            // 这里需要解析页面配置中的组件定义，找到对应字段的默认值
            // 由于页面配置结构复杂，暂时返回null
            return null;
        }
        catch (Exception ex)
        {
            Logger.LogWarning(ex, "Failed to get default value from config for field '{FieldName}'", fieldName);
            return null;
        }
    }

    public async Task<bool> DeleteAsync(string appId, string pageId, string id)
    {
        var formPageSchema = await _pageDomainService.GetAsync(appId, pageId);

        return await _formDataRepository.DeleteAsync(formPageSchema.DataSource.DataSourceValue, id);
    }
}
