using Microsoft.AspNetCore.Components;
using H.LowCode.Metadata;

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

        public RenderFragment Render(PageSchema jsonSchema)
        {
            return CreateDynamicComponent(jsonSchema);
        }

        private RenderFragment CreateDynamicComponent(PageSchema jsonSchema) => builder =>
        {
            foreach (var componentSchema in jsonSchema.ComponentSchemas)
            {
                foreach (var elementRender in _elementRenders)
                {
                    if (!elementRender.CanRender(componentSchema))
                        continue;

                    builder.OpenElement(0, "div");
                    builder.AddAttribute(1, "class", "field");
                    elementRender.Render(builder, componentSchema.Name, componentSchema, CreateDynamicComponent);
                    builder.CloseElement();

                    break;
                }
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
