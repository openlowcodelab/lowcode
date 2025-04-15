using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace H.LowCode.MetaSchema.DesignEngine;

public class ComponentPartsDataSourceSchema : ComponentDataSourceSchemaBase
{
    /// <summary>
    /// DataSource 渲染的 Fragment
    /// </summary>
    [JsonPropertyName("dsfrag")]
    public ComponentPartsFragmentSchema DataSourceFragment { get; set; }
}
