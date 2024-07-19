using System;

namespace H.LowCode.MetaSchema
{
    public abstract class BaseMetaSchema
    {
        public string CreatedName {  get; set; }

        public DateTime CreatedTime { get; set; }

        public string ModifiedName { get; set; }

        public DateTime ModifiedTime { get; set; }
    }
}
