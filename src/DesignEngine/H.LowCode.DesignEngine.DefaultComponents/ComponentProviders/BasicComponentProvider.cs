using AntDesign;
using H.LowCode.MetaSchema;

namespace H.LowCode.DesignEngine.DefaultComponents.ComponentProviders
{
    public class BasicComponentProvider : IComponentProvider
    {
        public string Title { get; set; } = "基础组件";

        public IEnumerable<ComponentSchema> LoadComponent()
        {
            List<ComponentSchema> components =
            [
                new(ComponentType:"input"){
                    SupportProperties = ["MaximumLength", "MinimumLength", "Pattern", "Format"],
                    RenderFragment = (componentSchema) =>  (builder) =>
                    {
                        builder.OpenComponent(1, typeof(Input<string>));
                        builder.CloseComponent();
                    },
                    ComponentPropertySchema = new(){
                        Title = "输入框",
                        ComponentValueType = ComponentValueType.String
                    }
                },
                new(ComponentType:"textarea"){
                    SupportProperties = ["MaximumLength", "MinimumLength"],
                    RenderFragment = (componentSchema) =>  (builder) =>
                    {
                        builder.OpenComponent(0, typeof(TextArea));
                        builder.AddAttribute(1, "style", "height:50px;");
                        builder.CloseComponent();
                    },
                    ComponentPropertySchema= new(){
                        Title = "大输入框",
                        ComponentValueType = ComponentValueType.String
                    }
                },
                new(ComponentType:"datepicker"){
                    RenderFragment = (componentSchema) =>  (builder) =>
                    {
                        builder.OpenComponent(0, typeof(DatePicker<DateTime?>));
                        builder.AddAttribute(1, "Picker", DatePickerType.Date);
                        builder.CloseComponent();
                    },
                    ComponentPropertySchema = new(){
                        Title = "日期选择",
                        ComponentValueType = ComponentValueType.String
                    }
                },
                new(ComponentType:"inputnumber"){
                    SupportProperties = ["Maximum", "Minimum"],
                    RenderFragment = (componentSchema) =>  (builder) =>
                    {
                        builder.OpenComponent(0, typeof(AntDesign.InputNumber<int>));
                        builder.CloseComponent();
                    },
                    ComponentPropertySchema = new(){
                        Title = "数字输入框",
                        ComponentValueType = ComponentValueType.Integer
                    }
                },
                new(ComponentType:"checkbox"){
                    RenderFragment = (componentSchema) =>  (builder) =>
                    {
                        builder.OpenComponent(0, typeof(Select<string, string>));
                        builder.CloseComponent();
                    },
                    ComponentPropertySchema = new(){
                        Title = "是否选择",
                    ComponentValueType = ComponentValueType.Boolean
                    }
                },
                new(ComponentType:"switch"){
                    RenderFragment = (componentSchema) =>  (builder) =>
                    {
                        builder.OpenComponent(0, typeof(Switch));
                        builder.CloseComponent();
                    },
                    ComponentPropertySchema = new(){
                        Title = "是否switch",
                        ComponentValueType = ComponentValueType.Boolean
                    }
                },
                new(ComponentType:"radio"){
                    RenderFragment = (componentSchema) =>  (builder) =>
                    {
                        builder.OpenComponent(0, typeof(Radio<string>));
                        builder.CloseComponent();
                    },
                    ComponentPropertySchema = new(){
                        Title = "下拉选择"
                    }
                }
            ];
            return components;
        }
    }
}
