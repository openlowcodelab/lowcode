using H.Extensions.System;
using H.Util.Ids;
using System.ComponentModel;
using System.Text.Json.Serialization;

namespace H.LowCode.MetaSchema.RenderEngine;

public class ComponentSchema : ComponentSchemaBase
{
    /// <summary>
    /// 组件渲染 Fragment
    /// </summary>
    [JsonPropertyName("frag")]
    public ComponentFragmentSchema Fragment { get; set; }

    [JsonPropertyName("ds")]
    public ComponentDataSourceSchema DataSource { get; set; } = new();

    /// <summary>
    /// Attribute定义分组
    /// </summary>
    /// <remarks>此数据通过 ConvertAttributeDefineToFragmentAttribute
    /// 转换为 Fragment 中的 Attribute 用于组件渲染</remarks>
    [JsonPropertyName("attrdefgroups")]
    public ComponentAttributeDefineGroupSchema[] AttributeDefineGroups { get; set; }

    /// <summary>
    /// 将组件自定义属性合并到 Fragment 中
    /// </summary>
    [JsonPropertyName("childs")]
    public ComponentSchema[] Childrens { get; set; }

    public void MergeAttributeDefineToFragment()
    {
        if (this?.AttributeDefineGroups == null
                    || this.AttributeDefineGroups.Length == 0)
            return;

        var attrList = new List<ComponentAttributeFragmentSchema>();
        if (this.Fragment?.Attributes != null)
        {
            attrList = this.Fragment.Attributes.ToList();
        }
        bool isMerge = false;

        foreach (var attrDefineGroup in this.AttributeDefineGroups)
        {
            if (attrDefineGroup?.AttributeDefines == null
                || attrDefineGroup.AttributeDefines.Length == 0)
                continue;

            foreach (var attrDefine in attrDefineGroup.AttributeDefines)
            {
                if (attrDefine == null)
                    continue;

                var attr = new ComponentAttributeFragmentSchema()
                {
                    AttributeName = attrDefine.AttributeName,
                    AttributeClrType = attrDefine.AttributeClrType,
                    AttributeValue = attrDefine.AttributeValue
                };
                attrList.Add(attr);

                isMerge = true;
            }
        }

        if (isMerge)
            this.Fragment.Attributes = attrList.ToArray();
    }
}