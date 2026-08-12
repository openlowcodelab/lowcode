using H.LowCode.MetaSchema;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;
using Microsoft.Extensions.Logging;
using System.Reflection;

namespace H.LowCode.ComponentBase;

public abstract class LowCodeDynamicComponentBase : LowCodeComponentBase
{
    /// <summary>
    /// 旧版 AntDesign 组件类型名 -> 现行 Hc 组件类型名 映射。
    /// 早期页面数据保存的是 AntDesign 原生类型（如 "AntDesign.Input`1[System.String], AntDesign"），
    /// 项目已迁移到自研 Hc 组件且不再引用 AntDesign 程序集，故在类型解析时做兼容映射。
    /// Key 为 AntDesign 短类名（去除泛型与程序集），Value 为 Hc 组件完整类型名。
    /// </summary>
    private static readonly Dictionary<string, string> _legacyAntDesignTypeMap = new(StringComparer.OrdinalIgnoreCase)
    {
        ["Input"] = "H.LowCode.Components.Defaults.HcInput, H.LowCode.Components.Defaults",
        ["InputNumber"] = "H.LowCode.Components.Defaults.HcInputNumber, H.LowCode.Components.Defaults",
        ["TextArea"] = "H.LowCode.Components.Defaults.HcTextarea, H.LowCode.Components.Defaults",
        ["Select"] = "H.LowCode.Components.Defaults.HcSelect, H.LowCode.Components.Defaults",
        ["RadioGroup"] = "H.LowCode.Components.Defaults.HcRadio, H.LowCode.Components.Defaults",
        ["Radio"] = "H.LowCode.Components.Defaults.HcRadioOption, H.LowCode.Components.Defaults",
        ["CheckboxGroup"] = "H.LowCode.Components.Defaults.HcCheckbox, H.LowCode.Components.Defaults",
        ["Checkbox"] = "H.LowCode.Components.Defaults.HcCheckboxOption, H.LowCode.Components.Defaults",
        ["Switch"] = "H.LowCode.Components.Defaults.HcSwitch, H.LowCode.Components.Defaults",
        ["DatePicker"] = "H.LowCode.Components.Defaults.HcDatePicker, H.LowCode.Components.Defaults",
        ["TimePicker"] = "H.LowCode.Components.Defaults.HcTimePicker, H.LowCode.Components.Defaults",
        ["AutoComplete"] = "H.LowCode.Components.Defaults.HcAutoComplete, H.LowCode.Components.Defaults",
        ["Cascader"] = "H.LowCode.Components.Defaults.HcCascader, H.LowCode.Components.Defaults",
        ["TreeSelect"] = "H.LowCode.Components.Defaults.HcTreeSelect, H.LowCode.Components.Defaults",
        ["Tree"] = "H.LowCode.Components.Defaults.HcTree, H.LowCode.Components.Defaults",
        ["Tabs"] = "H.LowCode.Components.Defaults.HcTabs, H.LowCode.Components.Defaults",
        ["TabPane"] = "H.LowCode.Components.Defaults.HcPlaceholder, H.LowCode.Components.Defaults",
        ["Card"] = "H.LowCode.Components.Defaults.HcCard, H.LowCode.Components.Defaults",
        ["Flex"] = "H.LowCode.Components.Defaults.HcFlex, H.LowCode.Components.Defaults",
        ["Row"] = "H.LowCode.Components.Defaults.HcRow, H.LowCode.Components.Defaults",
        ["Col"] = "H.LowCode.Components.Defaults.HcCol, H.LowCode.Components.Defaults",
        ["Layout"] = "H.LowCode.Components.Defaults.HcLayout, H.LowCode.Components.Defaults",
        ["Sider"] = "H.LowCode.Components.Defaults.HcSider, H.LowCode.Components.Defaults",
        ["Content"] = "H.LowCode.Components.Defaults.HcContent, H.LowCode.Components.Defaults",
        ["Button"] = "H.LowCode.Components.Defaults.HcButton, H.LowCode.Components.Defaults",
        ["Image"] = "H.LowCode.Components.Defaults.HcImage, H.LowCode.Components.Defaults",
        ["List"] = "H.LowCode.Components.Defaults.HcList, H.LowCode.Components.Defaults",
        ["Upload"] = "H.LowCode.Components.Defaults.HcUpload, H.LowCode.Components.Defaults",
    };

    /// <summary>
    /// 解析组件类型名为 Type。优先直接解析；无法解析且为 AntDesign 旧类型时，回退到 Hc 组件映射。
    /// 解析失败返回 null（调用方应跳过该节点渲染，避免整页崩溃）。
    /// </summary>
    protected static Type ResolveComponentType(string typeName)
    {
        if (string.IsNullOrEmpty(typeName))
            return null;

        var type = Type.GetType(typeName, throwOnError: false);
        if (type != null)
            return type;

        const string antDesignPrefix = "AntDesign.";
        if (typeName.StartsWith(antDesignPrefix, StringComparison.OrdinalIgnoreCase))
        {
            var afterNs = typeName.Substring(antDesignPrefix.Length);
            var cut = afterNs.IndexOfAny(new[] { '`', '[', ',', ' ' });
            var shortName = cut >= 0 ? afterNs.Substring(0, cut) : afterNs;
            if (_legacyAntDesignTypeMap.TryGetValue(shortName, out var mappedTypeName))
                return Type.GetType(mappedTypeName, throwOnError: false);
        }

        return null;
    }

    /// <summary>
    /// 组件属性渲染
    /// </summary>
    /// <param name="builder"></param>
    /// <param name="index"></param>
    /// <param name="componentId"></param>
    /// <param name="componentType"></param>
    /// <param name="attributes"></param>
    protected virtual void RenderComponentAttributes(RenderTreeBuilder builder, int index,
        string componentId, Type componentType,
        ComponentAttributeFragmentSchema[] attributes)
    {
        if (attributes == null || attributes.Length == 0)
            return;

        foreach (var attr in attributes)
        {
            RenderComponentAttribute(builder, index, componentId, componentType, attr);
        }
    }

    private void RenderComponentAttribute(RenderTreeBuilder builder, int index,
        string componentId, Type componentType,
        ComponentAttributeFragmentSchema attr)
    {
        ArgumentNullException.ThrowIfNull(attr);

        if (string.IsNullOrEmpty(attr.AttributeName))
            throw new NullReferenceException($"{nameof(attr.AttributeName)} is empty");

        var propertyInfo = componentType.GetProperty(attr.AttributeName);
        if (propertyInfo == null)
            return;

        if (attr.AttributeValue == null)
        {
            Logger.LogWarning($"componentId={componentId}, {nameof(attr.AttributeValue)} is null");
        }

        if (propertyInfo.PropertyType == typeof(RenderFragment))
        {
            //渲染 RenderFragment 属性（如 ChildContent）
            builder.AddAttribute(index++, attr.AttributeName, (RenderFragment)(childBuilder =>
            {
                childBuilder.AddContent(index++, attr.AttributeValue?.ToString());
            }));
        }
        else if (propertyInfo.PropertyType == typeof(EventCallback))
        {
            //渲染事件回调属性
            var method = GetType().GetMethod(attr.AttributeValue?.ToString(), BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
            if (method != null)
            {
                var delegateType = typeof(EventCallback);
                var eventCallback = Delegate.CreateDelegate(delegateType, this, method);
                builder.AddAttribute(index++, attr.AttributeName, eventCallback);
            }
        }
        else
        {
            //渲染简单属性
            RenderComponentSimpleAttribute(builder, index, componentId, componentType, attr);
        }
    }

    /// <summary>
    /// 渲染简单属性
    /// </summary>
    /// <param name="builder"></param>
    /// <param name="index"></param>
    /// <param name="componentId"></param>
    /// <param name="componentType"></param>
    /// <param name="attr"></param>
    /// <exception cref="NullReferenceException"></exception>
    private void RenderComponentSimpleAttribute(RenderTreeBuilder builder, int index,
        string componentId, Type componentType,
        ComponentAttributeFragmentSchema attr)
    {
        //if (prop.Name == "Value" && SupportsValueBinding(componentType))
        //{
        //    builder.AddAttribute(index++, "Value", prop.Value);
        //    builder.AddAttribute(index++, "ValueChanged", EventCallback.Factory.Create(this, (object newValue) =>
        //    {
        //        prop.Value = newValue;
        //        ValueChanged.InvokeAsync(newValue);
        //    }));
        //}

        if (string.IsNullOrEmpty(attr.AttributeClrType))
            return;

        var attrType = Type.GetType(attr.AttributeClrType, throwOnError: false);
        if (attrType == null)
            return;

        var realValue = attr.AttributeValue.ConvertToRealType(attrType);
        builder.AddAttribute(index++, attr.AttributeName, realValue);
    }

    private bool SupportsValueBinding(Type componentType)
    {
        // Check if the component has a "Value" parameter and a "ValueChanged" parameter
        var valueProperty = componentType.GetProperty("Value");
        var valueChangedProperty = componentType.GetProperty("ValueChanged");

        return valueProperty != null && valueChangedProperty != null;
    }
}
