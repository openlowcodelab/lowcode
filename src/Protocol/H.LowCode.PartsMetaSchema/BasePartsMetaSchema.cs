using System;

namespace H.LowCode.PartsMetaSchema;

public abstract class BasePartsMetaSchema
{
    public string CreatedUser {  get; set; }

    public DateTime CreatedTime { get; set; }

    public string ModifiedUser { get; set; }

    public DateTime ModifiedTime { get; set; }
}
