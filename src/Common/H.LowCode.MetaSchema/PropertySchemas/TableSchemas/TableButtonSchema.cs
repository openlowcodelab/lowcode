using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace H.LowCode.MetaSchema;

public class TableButtonSchema
{
    [JsonPropertyName("id")]
    public string Id { get; set; }

    [JsonPropertyName("n")]
    public string Name { get; set; }

    [JsonPropertyName("t")]
    public string Title { get; set; }

    [JsonPropertyName("btnType")]
    public string ButtonType { get; set; }

    [JsonPropertyName("disabled")]
    public bool Disabled { get; set; }

    /// <summary>
    /// 列顺序
    /// </summary>
    [JsonPropertyName("order")]
    public int Order { get; set; }

    [JsonPropertyName("actType")]
    public ActionTypeEnum ActionType { get; set; } = new();

    /// <summary>
    /// 目标对象Id
    /// </summary>
    /// <remarks>如打开页面时的PageId</remarks>
    [JsonPropertyName("tgId")]
    public string TargetId { get; set; }
}
