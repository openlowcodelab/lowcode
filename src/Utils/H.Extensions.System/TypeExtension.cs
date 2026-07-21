using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace System;

public static class TypeExtension
{
    public static object GetDefaultValue(this Type type)
    {
        ArgumentNullException.ThrowIfNull(type);

        //array 
        if (type.IsArray)
            return Array.CreateInstance(type.GetElementType(), 0);

        //List<T>
        if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(List<>))
            return Activator.CreateInstance(type);

        //Dictionary<TKey, TValue>
        if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Dictionary<,>))
            return Activator.CreateInstance(type);

        //其他集合接口 
        if (typeof(System.Collections.IList).IsAssignableFrom(type))
            return Array.Empty<object>();

        if (type.IsValueType)
        {
            return Activator.CreateInstance(type);
        }

        return null;
    }

    /// <summary>
    /// 旧版 AntDesign 组件短类名 -> 现行 Hc 组件完整类型名。
    /// 项目已从 AntDesign 迁移到自研 Hc 组件并移除 AntDesign 程序集，
    /// 早期页面数据仍保存 AntDesign 类型，故在类型解析时回退映射。
    /// </summary>
    private static readonly Dictionary<string, string> _legacyAntDesignTypeMap = new(StringComparer.OrdinalIgnoreCase)
    {
        ["Input"] = "H.LowCode.Components.Defaults.HcInput",
        ["InputNumber"] = "H.LowCode.Components.Defaults.HcInputNumber",
        ["TextArea"] = "H.LowCode.Components.Defaults.HcTextarea",
        ["Select"] = "H.LowCode.Components.Defaults.HcSelect",
        ["RadioGroup"] = "H.LowCode.Components.Defaults.HcRadio",
        ["Radio"] = "H.LowCode.Components.Defaults.HcRadioOption",
        ["CheckboxGroup"] = "H.LowCode.Components.Defaults.HcCheckbox",
        ["Checkbox"] = "H.LowCode.Components.Defaults.HcCheckboxOption",
        ["Switch"] = "H.LowCode.Components.Defaults.HcSwitch",
        ["DatePicker"] = "H.LowCode.Components.Defaults.HcDatePicker",
        ["TimePicker"] = "H.LowCode.Components.Defaults.HcTimePicker",
        ["AutoComplete"] = "H.LowCode.Components.Defaults.HcAutoComplete",
        ["Cascader"] = "H.LowCode.Components.Defaults.HcCascader",
        ["TreeSelect"] = "H.LowCode.Components.Defaults.HcTreeSelect",
        ["Tree"] = "H.LowCode.Components.Defaults.HcTree",
        ["Tabs"] = "H.LowCode.Components.Defaults.HcTabs",
        ["TabPane"] = "H.LowCode.Components.Defaults.HcPlaceholder",
        ["Card"] = "H.LowCode.Components.Defaults.HcCard",
        ["Flex"] = "H.LowCode.Components.Defaults.HcFlex",
        ["Row"] = "H.LowCode.Components.Defaults.HcRow",
        ["Col"] = "H.LowCode.Components.Defaults.HcCol",
        ["Layout"] = "H.LowCode.Components.Defaults.HcLayout",
        ["Sider"] = "H.LowCode.Components.Defaults.HcSider",
        ["Content"] = "H.LowCode.Components.Defaults.HcContent",
        ["Button"] = "H.LowCode.Components.Defaults.HcButton",
        ["Image"] = "H.LowCode.Components.Defaults.HcImage",
        ["List"] = "H.LowCode.Components.Defaults.HcList",
        ["Upload"] = "H.LowCode.Components.Defaults.HcUpload",
    };

    public static Type ResolveType(this string typeName)
    {
        if (string.IsNullOrWhiteSpace(typeName))
            return null;

        var type = Type.GetType(typeName, throwOnError: false, ignoreCase: true);
        if (type != null)
            return type;

        var (fullName, asmName) = SplitTypeName(typeName);
        if (string.IsNullOrEmpty(fullName))
            return null;

        foreach (var asm in AppDomain.CurrentDomain.GetAssemblies())
        {
            if (!string.IsNullOrEmpty(asmName))
            {
                var name = asm.GetName().Name;
                if (!string.Equals(name, asmName, StringComparison.OrdinalIgnoreCase))
                    continue;
            }
            var t = asm.GetType(fullName, throwOnError: false, ignoreCase: true);
            if (t != null)
                return t;
        }

        foreach (var asm in AppDomain.CurrentDomain.GetAssemblies())
        {
            var t = asm.GetType(fullName, throwOnError: false, ignoreCase: true);
            if (t != null)
                return t;
        }

        // 旧版 AntDesign 类型回退到 Hc 组件映射
        var mapped = MapLegacyAntDesignType(fullName);
        if (!string.IsNullOrEmpty(mapped))
        {
            foreach (var asm in AppDomain.CurrentDomain.GetAssemblies())
            {
                var t = asm.GetType(mapped, throwOnError: false, ignoreCase: true);
                if (t != null)
                    return t;
            }
        }

        return null;
    }

    private static string MapLegacyAntDesignType(string fullName)
    {
        const string antDesignPrefix = "AntDesign.";
        if (string.IsNullOrEmpty(fullName) || !fullName.StartsWith(antDesignPrefix, StringComparison.OrdinalIgnoreCase))
            return null;

        var afterNs = fullName.Substring(antDesignPrefix.Length);
        var cut = afterNs.IndexOfAny(new[] { '`', '[', ' ' });
        var shortName = cut >= 0 ? afterNs.Substring(0, cut) : afterNs;
        return _legacyAntDesignTypeMap.TryGetValue(shortName, out var mapped) ? mapped : null;
    }

    private static (string fullName, string asmName) SplitTypeName(string typeName)
    {
        var idx = typeName.IndexOf(',');
        if (idx < 0)
            return (typeName.Trim(), null);
        var fullName = typeName.Substring(0, idx).Trim();
        var asmName = typeName.Substring(idx + 1).Trim();
        return (fullName, asmName);
    }
}
