using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace H.Approval.Web;

/// <summary>
/// 节点基类
/// </summary>
[JsonConverter(typeof(NodeModelBaseConverter))]
public abstract class NodeModelBase
{
    public NodeModelBase()
    {
        ChildNodes = new List<NodeModelBase>();
        ConditionNodes = new List<NodeModelBase>();
    }

    public string Id { get; set; }

    public string NodeName {  get; set; }

    public NodeTypeEnum NodeType {  get; set; }

    public bool IsInput { get; set; }

    public List<NodeModelBase> ChildNodes {  get; set; }

    public List<NodeModelBase> ConditionNodes { get; set; }
}

public enum NodeTypeEnum
{
    Start = 0,
    Approve = 1,
    CarbonCopy = 2,
    Condition = 3,
    Branch = 4
}

/// <summary>
/// 节点多态JSON转换器
/// </summary>
public class NodeModelBaseConverter : JsonConverter<NodeModelBase>
{
    private static readonly JsonSerializerOptions s_childOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = false
    };

    public override NodeModelBase? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        using var doc = JsonDocument.ParseValue(ref reader);
        var root = doc.RootElement;

        if (!root.TryGetProperty("nodeType", out var nodeTypeProp) &&
            !root.TryGetProperty("NodeType", out nodeTypeProp))
        {
            throw new JsonException("缺少 NodeType 属性");
        }

        var nodeType = (NodeTypeEnum)nodeTypeProp.GetInt32();
        var rawText = root.GetRawText();

        return nodeType switch
        {
            NodeTypeEnum.Start => JsonSerializer.Deserialize<StartNodeModel>(rawText, s_childOptions),
            NodeTypeEnum.Approve => JsonSerializer.Deserialize<ApproveModel>(rawText, s_childOptions),
            NodeTypeEnum.CarbonCopy => JsonSerializer.Deserialize<CarbonCopyModel>(rawText, s_childOptions),
            NodeTypeEnum.Condition => JsonSerializer.Deserialize<ConditionModel>(rawText, s_childOptions),
            NodeTypeEnum.Branch => JsonSerializer.Deserialize<BranchModel>(rawText, s_childOptions),
            _ => throw new JsonException($"未知的节点类型: {nodeType}")
        };
    }

    public override void Write(Utf8JsonWriter writer, NodeModelBase value, JsonSerializerOptions options)
    {
        JsonSerializer.Serialize(writer, value, value.GetType(), s_childOptions);
    }
}

/// <summary>
/// 节点序列化工具类
/// </summary>
public static class NodeSerializer
{
    private static readonly JsonSerializerOptions s_options = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = false
    };

    public static string Serialize(NodeModelBase node)
    {
        return JsonSerializer.Serialize(node, s_options);
    }

    public static NodeModelBase? Deserialize(string json)
    {
        if (string.IsNullOrWhiteSpace(json) || json == "{}")
            return null;
        return JsonSerializer.Deserialize<NodeModelBase>(json, s_options);
    }
}
