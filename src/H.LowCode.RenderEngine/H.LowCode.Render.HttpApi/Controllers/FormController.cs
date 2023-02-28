using H.Ddd.HttpApi;
using H.LowCode.Metadata;
using Microsoft.AspNetCore.Mvc;
using System;

namespace H.LowCode.Render.HttpApi.Controllers
{
    public class FormController : ControllerApiBase
    {
        public FormController()
        {
        }

        [HttpGet]
        public object GetFormData(string id)
        {
            return new PageSchema();
        }

        [HttpPost]
        public void Save(string jsonData)
        {
            //PageSchema pageSchema = new PageSchema();
            //JsonSchemaHelper.ValidateJsonSchema(jsonSchema, jsonData);
        }
    }
}
