using AntDesign;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;
using H.LowCode.MetaSchema;

namespace H.LowCode.RenderEngine.AntBlazor.DataEntryComponent
{
    internal class DatePickerRender : ComponentRenderBase
    {
        public override bool CanRender(ComponentPropertySchema jsonSchema)
        {
            if (jsonSchema.ComponentValueType != ComponentValueType.String)
                return false;

            if (string.Equals(jsonSchema.Format, "date", StringComparison.OrdinalIgnoreCase))
                return true;

            return false;
        }

        public override void Render(RenderTreeBuilder builder, string key, ComponentPropertySchema jsonSchema, Func<PageSchema, RenderFragment> func)
        {
            builder.OpenElement(0, "div");
            builder.AddAttribute(1, "class", "");
            builder.AddContent(3, $"{jsonSchema.Title}：");
            builder.CloseElement();

            builder.OpenComponent(0, typeof(DatePicker<DateTime?>));
            builder.AddAttribute(1, "Picker", DatePickerType.Date);

            builder.CloseComponent();
        }
    }
}
