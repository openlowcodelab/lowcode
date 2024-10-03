using H.LowCode.MetaSchema;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace H.LowCode.DesignEngine.Application.Contracts;

public class DataSourceInput //: PagedResultRequestDto
{
    [JsonRequired]
    public DataSourceTypeEnum DataSourceType { get; set; }
}
