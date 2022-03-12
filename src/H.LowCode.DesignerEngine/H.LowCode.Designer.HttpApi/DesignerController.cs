using H.LowCode.Metadata;
using Microsoft.AspNetCore.Mvc;
using System;

namespace H.LowCode.Designer.HttpApi
{
    public class DesignerController : ControllerBase
    {
        public void SaveMetadata(FormJsonSchema formJsonSchema)
        {
            string filePath = string.Empty;
            formJsonSchema.WriteToFile(filePath);
        }
    }
}
