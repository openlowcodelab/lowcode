using H.LowCode.JsonSchemaExtensions;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Schema;
using System;
using System.Text;

namespace H.LowCode.Designer.HttpApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class DesignerController : ControllerBase
    {
        [HttpGet]
        public string Get()
        {
            return "ok";
        }

        [HttpPost]
        [Route("SaveMetadata")]
        public void SaveMetadata(JSchema jsonSchema)
        {
            string filePath = @"D:\temp.json";
            System.IO.File.WriteAllText(filePath, jsonSchema.ToJson(), Encoding.UTF8);
        }
    }
}
