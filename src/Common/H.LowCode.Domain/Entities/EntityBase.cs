using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace H.LowCode.Domain;

public abstract class EntityBase
{
    protected EntityBase()
    {
    }

    public string Id { get; set; }

    ///// <summary>
    ///// 创建一个自定实体。
    ///// </summary>
    ///// <param name="entityName">实体名称。</param>
    ///// <returns>返回创建的自定义实体。</returns>
    //internal static EntityBase Create(string entityName)
    //{
    //    EntityBase entity = EntityFactory.CreateInstance<EntityBase>();
    //    entity.SetEntityName(entityName);
    //    return entity;
    //}
}
