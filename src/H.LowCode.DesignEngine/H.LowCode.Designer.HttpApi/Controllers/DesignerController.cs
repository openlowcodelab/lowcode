using H.Extensions.Json.Schema;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Schema;
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
        public void SaveMetadata([FromForm] string jsonSchema)
        {
            string filePath = @"D:\temp.json";
            System.IO.File.WriteAllText(filePath, jsonSchema, Encoding.UTF8);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="jsonSchema"></param>
        [HttpPost]
        [Route("SaveMetadata2")]
        public void SaveMetadata2([FromBody]JSchema jsonSchema)
        {
            string filePath = @"D:\temp.json";
            System.IO.File.WriteAllText(filePath, jsonSchema.ToJson(), Encoding.UTF8);
        }
    }
}
