using Microsoft.AspNetCore.Components;
using H.LowCode.MetaSchema;
using H.LowCode.RenderEngine.AntBlazor.Renders;

namespace H.LowCode.RenderEngine.AntBlazor.PageRenders
{
    internal class ListRender : AntBlazorRender
    {
        private static bool _isInitElementRenders = false;
        private static List<ComponentRenderBase> _elementRenders = new List<ComponentRenderBase>();

        public ListRender()
        {
            InitElementRenders();
        }

        public RenderFragment Render(PageSchema jsonSchema)
        {
            return CreateDynamicComponent(jsonSchema);
        }

        private RenderFragment CreateDynamicComponent(PageSchema jsonSchema) => builder =>
        {
            foreach (var component in jsonSchema.Components)
            {
                //bool isCanRender = false;
                foreach (var elementRender in _elementRenders)
                {
                    if (!elementRender.CanRender(component.ComponentPropertySchema))
                        continue;

                    builder.OpenElement(0, "div");
                    builder.AddAttribute(1, "class", "field");
                    elementRender.Render(builder, component.ComponentPropertySchema.Name, component.ComponentPropertySchema, CreateDynamicComponent);
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
