using H.LowCode.Metadata;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;

namespace H.LowCode.RenderEngine
{
    public abstract class ComponentRenderBase
    {
        public abstract void Render(RenderTreeBuilder builder, string key, ComponentPropertySchema jsonSchema, Func<PageSchema, RenderFragment> func);

        public abstract bool CanRender(ComponentPropertySchema jsonSchema);
    }
}
