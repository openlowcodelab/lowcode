using H.LowCode.MetaSchema;
using H.Util.Ids;
using System.ComponentModel;
using System.Text.Json.Serialization;

namespace H.LowCode.MetaSchema.DesignEngine;

public class ComponentPartsSchema : ComponentSchemaBase
{
    /// <summary>
    /// 组件Id
    /// </summary>
    /// <remarks>一类组件唯一Id</remarks>
    [JsonPropertyName("compid")]
    public string ComponentId { get; set; } = ShortIdGenerator.Generate();

    /// <summary>
    /// 组件库Id
    /// </summary>
    [JsonPropertyName("libid")]
    public string LibraryId { get; set; }

    /// <summary>
    /// 组件 Name
    /// </summary>
    [JsonPropertyName("cn")]
    public string ComponentName { get; set; }

    /// <summary>
    /// 组件类型：1-原子组件  2-组合组件
    /// </summary>
    [JsonPropertyName("ct")]
    public int ComponentType { get; set; }

    /// <summary>
    /// 组件渲染 Fragment
    /// </summary>
    [JsonPropertyName("frag")]
    public ComponentPartsFragmentSchema Fragment { get; set; }

    [JsonPropertyName("ds")]
    public ComponentPartsDataSourceSchema DataSource { get; set; } = new();

    /// <summary>
    /// Attribute定义分组
    /// </summary>
    [JsonPropertyName("attrdefgroups")]
    public IList<ComponentPartsAttributeDefineGroupSchema> AttributeDefineGroups { get; set; } = [];

    /// <summary>
    /// 
    /// </summary>
    [JsonPropertyName("childs")]
    public IList<ComponentPartsSchema> Childrens { get; set; } = [];

    [JsonPropertyName("order")]
    public int Order { get; set; }

    /// <summary>
    /// 发布状态
    /// </summary>
    [JsonPropertyName("pub")]
    public int PublishStatus { get; set; }

    [JsonPropertyName("mt")]
    public DateTime ModifiedTime { get; set; }

    #region 仅设计过程使用
    /// <summary>
    /// 设计过程中的组件状态 (无需持久化)
    /// </summary>
    [JsonIgnore]
    public ComponentDesignStateSchema DesignState { get; set; } = new();

    [JsonIgnore]
    public Action Refresh { get; set; }

    public void RefreshState()
    {
        Refresh?.Invoke();
    }
    #endregion

    #region DeepClone
    public ComponentPartsSchema DeepClone()
    {
        ComponentPartsSchema newComponent = ObjectExtension.DeepClone(this);

        //Copy全新对象, Id 重新生成
        newComponent.Id = ShortIdGenerator.Generate();
        newComponent.ParentId = string.Empty;
        newComponent.Name = $"{newComponent.ComponentName}_{Random.Shared.Next(100, 999)}";
        newComponent.DesignState.IsSelected = false;

        //手动赋值无法序列化属性
        newComponent.Refresh = Refresh;

        //1.子节点 ParentId 重新赋值; 2.重新赋值序列化过程中丢失的 RenderFragment、Refresh 值
        DeepCloneRecursive(newComponent, this);

        return newComponent;
    }

    private static void DeepCloneRecursive(ComponentPartsSchema newComponent, ComponentPartsSchema oldComponent)
    {
        for (var i = 0; i < newComponent.Childrens.Count; i++)
        {
            var child = newComponent.Childrens[i];
            child.Id = ShortIdGenerator.Generate();
            child.ParentId = newComponent.Id;

            child.Refresh = oldComponent.Childrens[i].Refresh;

            DeepCloneRecursive(child, oldComponent.Childrens[i]);
        }
    }
    #endregion

    public ComponentPartsSchema ConvertToComponentSchema()
    {
        string json = this.ToJson();
        return json.FromJson<ComponentPartsSchema>();
    }

    /// <summary>
    /// 将组件物料定义合并到组件实例
    /// </summary>
    /// <param name="componentPartsDefine"></param>
    /// <returns></returns>
    public void MergeComponentPartsDefine(ComponentPartsSchema componentPartsDefine)
    {
        if (componentPartsDefine == null)
        {
            return;
        }

        //TODO
        //this.Fragment = componentPartsDefine.Fragment;
        //this.Style = componentPartsDefine.Style;
        this.AttributeDefineGroups = componentPartsDefine.AttributeDefineGroups;
        this.IsSupportDataSource = componentPartsDefine.IsSupportDataSource;
        
        foreach (var child in this.Childrens)
        {
            child.MergeComponentPartsDefine(componentPartsDefine);
        }
    }
}