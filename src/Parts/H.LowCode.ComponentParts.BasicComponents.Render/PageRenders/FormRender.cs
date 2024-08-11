using Microsoft.AspNetCore.Components;
using H.LowCode.MetaSchema;
using H.LowCode.RenderEngine;

namespace H.LowCode.ComponentParts.BasicComponents.Render
{
    internal class FormRender : AntBlazorRender
    {
        private static bool _isInitElementRenders = false;
        private static List<ComponentRenderBase> _elementRenders = [];

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
            foreach (var component in jsonSchema.Components)
            {
                foreach (var elementRender in _elementRenders)
                {
                    if (!elementRender.CanRender(component.ComponentProperty))
                        continue;

                    builder.OpenElement(0, "div");
                    builder.AddAttribute(1, "class", "field");
                    elementRender.Render(builder, component.ComponentProperty.Name, component.ComponentProperty, CreateDynamicComponent);
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
