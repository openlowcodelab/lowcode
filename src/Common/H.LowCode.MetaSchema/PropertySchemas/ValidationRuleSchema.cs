using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace H.LowCode.MetaSchema;

public class ValidationRuleSchema
{
    /// <summary>
    /// 校验规则ID
    /// </summary>
    [JsonPropertyName("id")]
    public string Id { get; set; }

    /// <summary>
    /// 关联的组件ID
    /// </summary>
    [JsonPropertyName("cid")]
    public string ComponentId { get; set; }

    /// <summary>
    /// 是否启用
    /// </summary>
    [JsonPropertyName("enabled")]
    public bool IsEnabled { get; set; }

    /// <summary>
    /// 校验规则类型
    /// </summary>
    [JsonPropertyName("type")]
    public ValidationRuleTypeEnum RuleType { get; set; }

    /// <summary>
    /// 是否必填
    /// </summary>
    [JsonPropertyName("required")]
    public bool IsRequired { get; set; }

    /// <summary>
    /// 最小长度
    /// </summary>
    [JsonPropertyName("minlen")]
    public int? MinLength { get; set; }

    /// <summary>
    /// 最大长度
    /// </summary>
    [JsonPropertyName("maxlen")]
    public int? MaxLength { get; set; }

    /// <summary>
    /// 最小值
    /// </summary>
    [JsonPropertyName("minval")]
    public decimal? MinValue { get; set; }

    /// <summary>
    /// 最大值
    /// </summary>
    [JsonPropertyName("maxval")]
    public decimal? MaxValue { get; set; }

    /// <summary>
    /// 正则表达式
    /// </summary>
    [JsonPropertyName("pattern")]
    public string? Pattern { get; set; }

    /// <summary>
    /// 自定义表达式
    /// </summary>
    [JsonPropertyName("expr")]
    public string? Expression { get; set; }

    /// <summary>
    /// 错误消息
    /// </summary>
    [JsonPropertyName("errmsg")]
    public string? ErrorMessage { get; set; }

    /// <summary>
    /// 校验触发时机
    /// </summary>
    [JsonPropertyName("trigger")]
    public ValidationTriggerEnum Trigger { get; set; } = ValidationTriggerEnum.Blur;

    /// <summary>
    /// 排序
    /// </summary>
    [JsonPropertyName("order")]
    public int Order { get; set; }
}

/// <summary>
/// 校验规则类型枚举
/// </summary>
public enum ValidationRuleTypeEnum
{
    /// <summary>
    /// 必填校验
    /// </summary>
    Required = 1,

    /// <summary>
    /// 最小长度校验
    /// </summary>
    MinLength = 2,

    /// <summary>
    /// 最大长度校验
    /// </summary>
    MaxLength = 3,

    /// <summary>
    /// 最小值校验
    /// </summary>
    MinValue = 4,

    /// <summary>
    /// 最大值校验
    /// </summary>
    MaxValue = 5,

    /// <summary>
    /// 正则表达式校验
    /// </summary>
    Pattern = 6,

    /// <summary>
    /// 邮箱格式校验
    /// </summary>
    Email = 7,

    /// <summary>
    /// 手机号格式校验
    /// </summary>
    Phone = 8,

    /// <summary>
    /// URL格式校验
    /// </summary>
    Url = 9,

    /// <summary>
    /// 身份证号格式校验
    /// </summary>
    IdCard = 10,

    /// <summary>
    /// 自定义表达式校验
    /// </summary>
    Custom = 99
}

/// <summary>
/// 校验触发时机枚举
/// </summary>
public enum ValidationTriggerEnum
{
    /// <summary>
    /// 失去焦点时校验
    /// </summary>
    Blur = 1,

    /// <summary>
    /// 值改变时校验
    /// </summary>
    Change = 2,

    /// <summary>
    /// 提交时校验
    /// </summary>
    Submit = 3
}
