using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace H.LowCode.Model;

public class ComponentPartsListModel
{
    [JsonPropertyName("id")]
    public string Id { get; set; }

    [JsonPropertyName("n")]
    public string Name { get; set; }

    [JsonPropertyName("order")]
    public int Order { get; set; }

    [JsonPropertyName("pubstatus")]
    public bool PublishStatus { get; set; }

    public DateTime ModifiedTime { get; set; }
}
