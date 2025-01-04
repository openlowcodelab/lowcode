using H.LowCode.MetaSchema;
using H.Util.Ids;
using System.ComponentModel;
using System.Text.Json.Serialization;

namespace H.LowCode.PartsMetaSchema;

public class ComponentPartsSchema : ComponentSchema
{
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
    public new ComponentPartsFragmentSchema Fragment { get; set; }

    /// <summary>
    /// Property 分组
    /// </summary>
    [JsonPropertyName("pgroups")]
    public new IList<ComponentPartsPropertyGroupSchema> PropertyGroups { get; set; } = [];

    /// <summary>
    /// 
    /// </summary>
    [JsonPropertyName("childs")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
    public new IList<ComponentPartsSchema> Childrens { get; set; } = [];

    /// <summary>
    /// 是否支持数据源属性
    /// </summary>
    [JsonPropertyName("sptds")]
    public bool IsSupportDataSource { get; set; }

    [JsonPropertyName("order")]
    public int Order { get; set; }

    /// <summary>
    /// 发布状态
    /// </summary>
    [JsonPropertyName("pub")]
    public int PublishStatus { get; set; }

    [JsonPropertyName("mt")]
    public DateTime ModifiedTime { get; set; }

    /// <summary>
    /// 设计过程中的组件状态 (无需持久化)
    /// </summary>
    [JsonIgnore]
    public ComponentDesignStateSchema DesignState { get; set; } = new();

    [JsonIgnore]
    public Action Refresh { get; set; }

    public ComponentPartsSchema CopyNew()
    {
        ComponentPartsSchema newComponent = this.DeepClone();

        //Copy全新对象, Id 重新生成
        newComponent.Id = ShortIdGenerator.Generate();
        newComponent.ParentId = string.Empty;
        newComponent.Name = $"{newComponent.ComponentName}_{Random.Shared.Next(100, 999)}";
        newComponent.DesignState.IsSelected = false;

        //手动赋值无法序列化属性
        newComponent.Refresh = Refresh;

        //1.子节点 ParentId 重新赋值; 2.重新赋值序列化过程中丢失的 RenderFragment、Refresh 值
        CopyNewRecursive(newComponent, this);

        return newComponent;
    }

    public ComponentSchema ConvertToComponentSchema()
    {
        string json = this.ToJson();
        return json.FromJson<ComponentSchema>();
    }

    public void RefreshState()
    {
        Refresh?.Invoke();
    }

    #region private
    private static void CopyNewRecursive(ComponentPartsSchema newComponent, ComponentPartsSchema oldComponent)
    {
        for (var i = 0; i < newComponent.Childrens.Count; i++)
        {
            var child = newComponent.Childrens[i];
            child.Id = ShortIdGenerator.Generate();
            child.ParentId = newComponent.Id;

            child.Refresh = oldComponent.Childrens[i].Refresh;

            CopyNewRecursive(child, oldComponent.Childrens[i]);
        }
    }
    #endregion
}