using Microsoft.AspNetCore.Components;
using Newtonsoft.Json.Schema;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace H.LowCode.RenderEngine.AntBlazor.PageRender
{
    internal class FormRender : AntBlazorRender
    {
        private static bool _isInitElementRenders = false;
        private static List<ComponentRenderBase> _elementRenders = new List<ComponentRenderBase>();

        public FormRender()
        {
            InitElementRenders();
        }

        public RenderFragment Render(JSchema jsonSchema)
        {
            return CreateDynamicComponent(jsonSchema);
        }

        private RenderFragment CreateDynamicComponent(JSchema jsonSchema) => builder =>
        {
            foreach (var kv in jsonSchema.Properties)
            {
                //bool isCanRender = false;
                foreach (var elementRender in _elementRenders)
                {
                    if (!elementRender.CanRender(kv.Value))
                        continue;

                    builder.OpenElement(0, "div");
                    builder.AddAttribute(1, "class", "field");
                    elementRender.Render(builder, kv.Key, kv.Value, CreateDynamicComponent);
                    builder.CloseElement();

                    //isCanRender = true;
                    break;  //可渲染的组件只有一个，渲染后结束遍历, 其他 ElementRender 不再判断是否可渲染
                }
                //if (!isCanRender)
                //    throw new ArgumentOutOfRangeException($"参数不合法");
            }
        };

        private void InitElementRenders()
        {
            if (_isInitElementRenders)
                return;

            var types = typeof(AntBlazorRender).Assembly.GetTypes().Where(t => typeof(ComponentRenderBase).IsAssignableFrom(t));
            foreach (var elementType in types)
            {
                _elementRenders.Add((ComponentRenderBase)Activator.CreateInstance(elementType));
            }

            _isInitElementRenders = true;
        }
    }
}
