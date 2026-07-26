using H.Util.Ids;
using System.Text.Json.Serialization;
using System.Linq;

namespace H.LowCode.MetaSchema.DesignEngine;

public class ComponentPartsSchema : ComponentSchemaBase
{
    /// <summary>
    /// 组件库Id
    /// </summary>
    [JsonPropertyName("libid")]
    public string? LibraryId { get; set; }

    /// <summary>
    /// 组件物料Id
    /// </summary>
    /// <remarks>一类组件唯一Id</remarks>
    [JsonPropertyName("partsId")]
    public required string PartsId { get; set; }

    /// <summary>
    /// 组件渲染 Fragment
    /// </summary>
    [JsonPropertyName("frag")]
    public ComponentPartsFragmentSchema? Fragment { get; set; }

    [JsonPropertyName("ds")]
    public ComponentPartsDataSourceSchema? DataSource { get; set; } = new();

    /// <summary>
    /// Attribute定义分组
    /// </summary>
    [JsonPropertyName("attrdefgroups")]
    public IEnumerable<ComponentPartsAttributeDefineGroupSchema> AttributeDefineGroups { get; set; } = [];

    /// <summary>
    /// 
    /// </summary>
    [JsonPropertyName("childs")]
    public IList<ComponentPartsSchema> Childrens { get; set; } = [];

    /// <summary>
    /// 组件支持的事件
    /// </summary>
    [JsonPropertyName("sptevs")]
    public string[]? SupportEvents { get; set; }

    /// <summary>
    /// 事件定义
    /// </summary>
    [JsonPropertyName("evdefs")]
    public List<ComponentPartsEventDefineSchema> EventDefines { get; set; } = [];

    /// <summary>
    /// 样式定义
    /// </summary>
    [JsonPropertyName("stydefs")]
    public List<ComponentPartsStyleDefineSchema> StyleDefines { get; set; } = [];

    /// <summary>
    /// 条件分支配置（用于条件渲染组件）
    /// Key: 条件值（字符串形式）
    /// Value: 对应的子组件配置
    /// </summary>
    [JsonPropertyName("cases")]
    public Dictionary<string, ComponentPartsSchema>? Cases { get; set; }

    /// <summary>
    /// 默认分支（当没有匹配的条件时渲染）
    /// </summary>
    [JsonPropertyName("default")]
    public ComponentPartsSchema? DefaultCase { get; set; }

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
    public Action? Refresh { get; set; }

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
        newComponent.Name = $"{newComponent.PartsId}_{Random.Shared.Next(100, 999)}";
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

        //基础属性合并
        //this.Fragment = componentPartsDefine.Fragment;
        //this.Style = componentPartsDefine.Style;
        this.IsHiddenLabel = componentPartsDefine.IsHiddenLabel;
        this.SupportEvents = componentPartsDefine.SupportEvents;

        //属性合并
        MergeAttributeDefineGroups(componentPartsDefine.AttributeDefineGroups);

        //数据源合并
        this.IsSupportDataSource = componentPartsDefine.IsSupportDataSource;
        if (componentPartsDefine?.DataSource?.DataSourceFragment != null)
            this.DataSource.DataSourceFragment = componentPartsDefine.DataSource.DataSourceFragment;
    }

    // 抽取的私有方法：合并 AttributeDefineGroups，存在则更新，不存在则新增
    private void MergeAttributeDefineGroups(IEnumerable<ComponentPartsAttributeDefineGroupSchema>? srcGroups)
    {
        if (srcGroups == null)
            return;

        var currentGroups = this.AttributeDefineGroups?.ToList() ?? new List<ComponentPartsAttributeDefineGroupSchema>();

        foreach (var srcGroup in srcGroups)
        {
            if (srcGroup == null || srcGroup.AttributeDefines == null)
                continue;

            var targetGroup = currentGroups.FirstOrDefault(g => g.GroupName == srcGroup.GroupName);

            if (targetGroup == null)
            {
                // 新增整个分组（复制数组以避免引用同一实例）
                var newGroup = new ComponentPartsAttributeDefineGroupSchema
                {
                    GroupName = srcGroup.GroupName,
                    AttributeDefines = srcGroup.AttributeDefines.ToArray()
                };

                currentGroups.Add(newGroup);
            }
            else
            {
                var targetAttrs = targetGroup.AttributeDefines?.ToList() ?? new List<ComponentPartsAttributeDefineSchema>();

                foreach (var srcAttr in srcGroup.AttributeDefines)
                {
                    if (srcAttr == null)
                        continue;

                    var existingAttr = targetAttrs.FirstOrDefault(a => a.AttributeName == srcAttr.AttributeName);
                    if (existingAttr != null)
                    {
                        existingAttr.DisplayName = srcAttr.DisplayName;
                        existingAttr.AttributeItemType = srcAttr.AttributeItemType;
                        //existingAttr.IsRequired = srcAttr.IsRequired;
                        existingAttr.Description = srcAttr.Description;
                        existingAttr.DefaultValue = srcAttr.DefaultValue;
                        existingAttr.Options = srcAttr.Options;
                        //existingAttr.IsValidationEnabled = srcAttr.IsValidationEnabled;
                        //existingAttr.ValidationRules = srcAttr.ValidationRules;
                    }
                    else
                    {
                        targetAttrs.Add(srcAttr);
                    }
                }

                targetGroup.AttributeDefines = targetAttrs.ToArray();
            }
        }

        this.AttributeDefineGroups = currentGroups;
    }
}