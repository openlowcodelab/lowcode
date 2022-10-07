using H.Extensions.Json.Schema;
using Newtonsoft.Json.Schema;
using System;

namespace H.LowCode.Metadata
{
    public class FormJsonSchema : JSchema
    {
        public bool WriteToFile(string filePath)
        {
            return this.WriteToJsonFile(filePath);
        }
    }
}
