using H.LowCode.MetaSchema;
using System.Text.Json.Serialization;

namespace H.LowCode.DesignEngine.Application.Contracts;

public class DataSourceInput //: PagedResultRequestDto
{
    [JsonRequired]
    public ComponentDataSourceTypeEnum DataSourceType { get; set; }
}
