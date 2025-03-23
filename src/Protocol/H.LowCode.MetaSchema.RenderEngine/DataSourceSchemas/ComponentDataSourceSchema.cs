using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;

namespace H.LowCode.MetaSchema.RenderEngine;

public class ComponentDataSourceSchema : ComponentDataSourceSchemaBase
{
    /// <summary>
    /// DataSource 渲染的 Fragment
    /// </summary>
    [JsonPropertyName("dsfrag")]
    public ComponentFragmentSchema DataSourceFragment { get; set; }
}
