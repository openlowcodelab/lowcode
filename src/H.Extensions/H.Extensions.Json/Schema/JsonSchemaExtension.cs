using Newtonsoft.Json.Schema;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace H.Extensions.Json.Schema
{
    public static class JsonSchemaExtension
    {
        public static bool WriteToJsonFile(this JSchema jSchema, string filePath)
        {
            if (jSchema is null)
            {
                throw new ArgumentNullException(nameof(jSchema));
            }

            if (filePath is null)
            {
                throw new ArgumentNullException(nameof(filePath));
            }

            return false;
        }
    }
}
