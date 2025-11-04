using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;
using H.LowCode.Entity;
using H.LowCode.RenderEngine.Domain;
using H.LowCode.RenderEngine.Domain.Repositories;
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

    public IReadOnlyList<DynamicEntityInfo> LoadDynamicEntities(string appId)
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
            var fields = entity.TableFields.Select(f => new DynamicEntityField()
            {
                Name = f.Name,
                ClrType = FieldTypeMapping.GetFieldType(f.Type, f.IsNullable),
                IsNullable = f.IsNullable
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
