using System;

namespace H.LowCode.MetaSchema.DesignEngine;

public abstract class PartsMetaSchemaBase
{
    public string CreatedUser {  get; set; }

    public DateTime CreatedTime { get; set; }

    public string ModifiedUser { get; set; }

    public DateTime ModifiedTime { get; set; }
}
