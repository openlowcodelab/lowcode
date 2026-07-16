using System.Collections.Generic;
using System.Text.Json;

namespace H.Approval.Application.Contracts;

/// <summary>
/// 表单字段类型
/// </summary>
public static class FormFieldTypes
{
    public const string Input = "input";           // 单行输入框
    public const string Textarea = "textarea";     // 多行输入框
    public const string Number = "number";         // 数字输入框
    public const string Amount = "amount";         // 金额
    public const string Date = "date";             // 日期
    public const string Radio = "radio";           // 单选框
    public const string Checkbox = "checkbox";     // 多选框
    public const string Description = "description";// 说明文字
}

/// <summary>
/// 表单字段定义
/// </summary>
public class FormFieldModel
{
    /// <summary>字段唯一标识</summary>
    public string Id { get; set; } = string.Empty;

    /// <summary>字段类型(见 FormFieldTypes)</summary>
    public string Type { get; set; } = FormFieldTypes.Input;

    /// <summary>标题</summary>
    public string Label { get; set; } = string.Empty;

    /// <summary>提示文字</summary>
    public string? Placeholder { get; set; }

    /// <summary>是否必填</summary>
    public bool Required { get; set; }

    /// <summary>选项(单选/多选使用)</summary>
    public List<string> Options { get; set; } = new();
}

/// <summary>
/// 表单设计 Schema
/// </summary>
public class FormSchema
{
    public List<FormFieldModel> Fields { get; set; } = new();

    private static readonly JsonSerializerOptions Options = new()
    {
        PropertyNameCaseInsensitive = true
    };

    public static FormSchema Parse(string? json)
    {
        if (string.IsNullOrWhiteSpace(json))
        {
            return new FormSchema();
        }
        try
        {
            return JsonSerializer.Deserialize<FormSchema>(json, Options) ?? new FormSchema();
        }
        catch
        {
            return new FormSchema();
        }
    }

    public string Serialize()
    {
        return JsonSerializer.Serialize(this, Options);
    }
}
