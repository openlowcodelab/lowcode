using AntDesign;
using H.LowCode.Schema;

namespace H.LowCode.DesignEngine.Components.ComponentProviders
{
    public class BasicComponentProvider : IComponentProvider
    {
        public string Title { get; set; } = "基础组件";

        public IEnumerable<ComponentSchema> LoadComponent()
        {
            List<ComponentSchema> components = new List<ComponentSchema>()
            {
                new ComponentSchema(ComponentType:"input"){
                    SupportProperties = new List<string>() { "MaximumLength", "MinimumLength", "Pattern", "Format" },
                    RenderFragment = (builder) =>
                    {
                        builder.OpenComponent(1, typeof(Input<string>));
                        builder.CloseComponent();
                    },
                    ComponentPropertySchema = new ComponentPropertySchema(){
                        Title = "输入框",
                        ComponentValueType = ComponentValueType.String
                    }
                },
                new ComponentSchema(ComponentType:"textarea"){
                    SupportProperties = new List<string>(){ "MaximumLength", "MinimumLength" },
                    RenderFragment = (builder) =>
                    {
                        builder.OpenComponent(0, typeof(TextArea));
                        builder.AddAttribute(1, "style", "height:50px;");
                        builder.CloseComponent();
                    },
                    ComponentPropertySchema= new ComponentPropertySchema(){
                        Title = "大输入框",
                        ComponentValueType = ComponentValueType.String
                    }
                },
                new ComponentSchema(ComponentType:"datepicker"){
                    RenderFragment = (builder) =>
                    {
                        builder.OpenComponent(0, typeof(DatePicker<DateTime?>));
                        builder.AddAttribute(1, "Picker", DatePickerType.Date);
                        builder.CloseComponent();
                    },
                    ComponentPropertySchema = new ComponentPropertySchema(){
                        Title = "日期选择",
                        ComponentValueType = ComponentValueType.String
                    }
                },
                new ComponentSchema(ComponentType:"inputnumber"){
                    SupportProperties = new List<string>{ "Maximum", "Minimum" },
                    RenderFragment = (builder) =>
                    {
                        builder.OpenComponent(0, typeof(AntDesign.InputNumber<int>));
                        builder.CloseComponent();
                    },
                    ComponentPropertySchema = new ComponentPropertySchema(){
                        Title = "数字输入框",
                        ComponentValueType = ComponentValueType.Integer
                    }
                },
                new ComponentSchema(ComponentType:"checkbox"){
                    RenderFragment = (builder) =>
                    {
                        builder.OpenComponent(0, typeof(Select<string, string>));
                        builder.CloseComponent();
                    },
                    ComponentPropertySchema = new ComponentPropertySchema(){
                        Title = "是否选择",
                    ComponentValueType = ComponentValueType.Boolean
                    }
                },
                new ComponentSchema(ComponentType:"switch"){
                    RenderFragment = (builder) =>
                    {
                        builder.OpenComponent(0, typeof(Switch));
                        builder.CloseComponent();
                    },
                    ComponentPropertySchema = new ComponentPropertySchema(){
                        Title = "是否switch",
                        ComponentValueType = ComponentValueType.Boolean
                    }
                },
                new ComponentSchema(ComponentType:"radio"){
                    RenderFragment = (builder) =>
                    {
                        builder.OpenComponent(0, typeof(Radio<string>));
                        builder.CloseComponent();
                    },
                    ComponentPropertySchema = new ComponentPropertySchema(){
                        Title = "下拉选择"
                    }
                }
            };
            return components;
        }
    }
}
