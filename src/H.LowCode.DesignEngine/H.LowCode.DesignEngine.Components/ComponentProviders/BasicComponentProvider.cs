using AntDesign;
using H.LowCode.DesignEngine.Components.PropertySchemas;
using H.LowCode.Metadata.Components;
using Newtonsoft.Json.Schema;

namespace H.LowCode.DesignEngine.Components.ComponentProviders
{
    public class BasicComponentProvider : IComponentProvider
    {
        public string Title { get; set; } = "基础组件";

        public IEnumerable<ComponentSchema> LoadComponent()
        {
            List<ComponentSchema> components = new List<ComponentSchema>()
            {
                new ComponentSchema(){
                    ComponentJsonSchema = new JSchema(){ Title = "输入框", Type = JSchemaType.String },
                    ComponentType = "input",
                    ComponentPropertySchema = new InputPropertyModel(),
                    ComponentRenderFragment = (builder) =>
                    {
                        builder.OpenComponent(1, typeof(Input<string>));
                        builder.CloseComponent();
                    }
                },
                new ComponentSchema(){
                    ComponentJsonSchema = new JSchema(){ Title = "大输入框", Type = JSchemaType.String },
                    ComponentType = "textarea",
                    ComponentPropertySchema = new TextareaPropertyModel(),
                    ComponentRenderFragment = (builder) =>
                    {
                        builder.OpenComponent(0, typeof(TextArea));
                        builder.AddAttribute(1, "style", "height:50px;");
                        builder.CloseComponent();
                    }
                },
                new ComponentSchema(){
                    ComponentJsonSchema = new JSchema(){ Title = "日期选择", Type = JSchemaType.String },
                    ComponentType = "datepicker",
                    ComponentPropertySchema = new DatePickerPropertyModel(),
                    ComponentRenderFragment = (builder) =>
                    {
                        builder.OpenComponent(0, typeof(DatePicker<DateTime?>));
                        builder.AddAttribute(1, "Picker", DatePickerType.Date);
                        builder.CloseComponent();
                    }
                },
                new ComponentSchema(){
                    ComponentJsonSchema = new JSchema(){ Title = "数字输入框", Type = JSchemaType.Integer},
                    ComponentType = "inputnumber",
                    ComponentPropertySchema = new InputNumberPropertyModel(),
                    ComponentRenderFragment = (builder) =>
                    {
                        builder.OpenComponent(0, typeof(AntDesign.InputNumber<int>));
                        builder.CloseComponent();
                    }
                },
                new ComponentSchema(){
                    ComponentJsonSchema = new JSchema(){ Title = "是否选择", Type = JSchemaType.Boolean },
                    ComponentType = "checkbox",
                    ComponentPropertySchema = new CheckboxPropertyModel(),
                    ComponentRenderFragment = (builder) =>
                    {
                        builder.OpenComponent(0, typeof(Select<string, string>));
                        builder.CloseComponent();
                    }
                },
                new ComponentSchema(){
                    ComponentJsonSchema = new JSchema(){ Title = "是否switch", Type = JSchemaType.Boolean },
                    ComponentType = "switch",
                    ComponentPropertySchema = new SwitchPropertyModel(),
                    ComponentRenderFragment = (builder) =>
                    {
                        builder.OpenComponent(0, typeof(Switch));
                        builder.CloseComponent();
                    }
                },
                new ComponentSchema(){
                    ComponentJsonSchema = new JSchema(){ Title = "下拉选择" },
                    ComponentType = "radio",
                    ComponentPropertySchema = new RadioPropertyModel(),
                    ComponentRenderFragment = (builder) =>
                    {
                        builder.OpenComponent(0, typeof(Radio<string>));
                        builder.CloseComponent();
                    }
                }
            };
            return components;
        }
    }
}
