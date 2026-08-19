using H.LowCode.Entity;
using H.LowCode.RenderEngine.Domain.Repositories;
using System.ComponentModel.DataAnnotations;
using System.Reflection;
using System.Reflection.Emit;
using Volo.Abp.Domain.Entities;

namespace H.LowCode.RenderEngine.EntityFrameworkCore;

public class EntityTypeManager
{
    private static AssemblyBuilder _dynamicAssembly;
    private static ModuleBuilder _dynamicModule;
    private const string _dynamicAssemblyName = "H.LowCode.DynamicEntity";
    private const string _dynamicModuleName = "DynamicModule";

    private static Dictionary<string, List<DynamicEntityInfo>> _dynamicEntitiesDic = [];

    private IDataSourceRepository _dataSourceRepository;

    public EntityTypeManager(IDataSourceRepository dataSourceRepository)
    {
        _dataSourceRepository = dataSourceRepository;
    }

    public IReadOnlyList<DynamicEntityInfo> LoadDynamicEntities(string? appId)
    {
        if (string.IsNullOrEmpty(appId))
        {
            throw new EntityNotFoundException($"{nameof(appId)} is empty");
        }

        InitDynamicAssembly();

        if (_dynamicEntitiesDic.ContainsKey(appId))
            return _dynamicEntitiesDic[appId];

        var dynamicEntities = new List<DynamicEntityInfo>();

        var entities = _dataSourceRepository.GetAllEntities(appId);
        foreach (var entity in entities)
        {
            var fields = entity.TableFields.Select(f =>
            {
                var field = new DynamicEntityField()
                {
                    Name = f.Name,
                    ClrType = FieldTypeMapping.GetFieldType(f.Type, f.IsNullable),
                    IsNullable = f.IsNullable
                };

                // 解析类型中的长度声明（如 varchar(2000)），text 类型默认长文本
                field.MaxLength = ResolveFieldMaxLength(f.Type);
                return field;
            });

            var primaryField = entity.TableFields.FirstOrDefault(t => t.IsPrimaryKey);
            if (primaryField == null)
                throw new ValidationException("primary is required");

            //创建实体类
            var entityType = EntityFactory.CreateEntityType(_dynamicModule, entity.Name, fields);

            var dynamicEntity = new DynamicEntityInfo()
            {
                EntityName = entity.Name,
                EntityType = entityType,
                PrimaryKey = primaryField.Name,
                EnableSoftDelete = entity.EnableSoftDelete,
                Fields = fields
            };
            dynamicEntities.Add(dynamicEntity);
        }

        _dynamicEntitiesDic[appId] = dynamicEntities;

        return dynamicEntities;
    }

    /// <summary>
    /// 解析字段类型的长度声明（如 varchar(2000) 返回 2000；text 类型返回长文本默认长度）
    /// </summary>
    private static int? ResolveFieldMaxLength(string? fieldType)
    {
        if (string.IsNullOrWhiteSpace(fieldType))
            return null;

        var raw = fieldType.Trim().ToLowerInvariant();

        int parenIndex = raw.IndexOf('(');
        if (parenIndex > 0)
        {
            var closeIndex = raw.IndexOf(')', parenIndex);
            if (closeIndex > parenIndex)
            {
                var lengthText = raw.Substring(parenIndex + 1, closeIndex - parenIndex - 1).Split(',')[0].Trim();
                if (int.TryParse(lengthText, out var length))
                    return length;
            }
        }

        var baseType = parenIndex >= 0 ? raw[..parenIndex].Trim() : raw;
        if (baseType == "text")
            return 4000;

        return null;
    }

    private static void InitDynamicAssembly()
    {
        if (_dynamicAssembly == null)
        {
            AssemblyName dynamicAssemblyName = new AssemblyName(_dynamicAssemblyName);
            //dynamicAssemblyName.Version = new Version(1, 0, 0, 1);

            _dynamicAssembly = AssemblyBuilder.DefineDynamicAssembly(dynamicAssemblyName, AssemblyBuilderAccess.Run);
            _dynamicModule = _dynamicAssembly.DefineDynamicModule(_dynamicModuleName);
        }
    }
}
