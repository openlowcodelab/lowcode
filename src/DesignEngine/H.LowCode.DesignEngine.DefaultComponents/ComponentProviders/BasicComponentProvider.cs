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
                    RenderFragment = (component) =>  (builder) =>
                    {
                        builder.OpenComponent(1, typeof(Input<string>));
                        builder.CloseComponent();
                    },
                    ComponentProperty = new(){
                        Title = "输入框",
                        ComponentValueType = ComponentValueType.String
                    }
                },
                new(ComponentType:"inputnumber"){
                    SupportProperties = ["Maximum", "Minimum"],
                    RenderFragment = (component) =>  (builder) =>
                    {
                        builder.OpenComponent(0, typeof(InputNumber<int>));
                        builder.CloseComponent();
                    },
                    ComponentProperty = new(){
                        Title = "数字输入框",
                        ComponentValueType = ComponentValueType.Integer
                    }
                },
                new(ComponentType:"textarea"){
                    SupportProperties = ["MaximumLength", "MinimumLength"],
                    RenderFragment = (component) =>  (builder) =>
                    {
                        builder.OpenComponent(0, typeof(TextArea));
                        builder.AddAttribute(1, "style", "height:50px;");
                        builder.CloseComponent();
                    },
                    ComponentProperty= new(){
                        Title = "大输入框",
                        ComponentValueType = ComponentValueType.String
                    }
                },
                new(ComponentType:"checkbox"){
                    RenderFragment = (component) =>  (builder) =>
                    {
                        builder.OpenComponent(0, typeof(Checkbox));
                        builder.CloseComponent();
                    },
                    ComponentProperty = new(){
                        Title = "多选框",
                    ComponentValueType = ComponentValueType.Boolean
                    }
                },
                new(ComponentType:"radio"){
                    RenderFragment = (component) =>  (builder) =>
                    {
                        builder.OpenComponent(0, typeof(Radio<string>));
                        builder.CloseComponent();
                    },
                    ComponentProperty = new(){
                        Title = "单选框"
                    }
                },
                new(ComponentType:"select"){
                    RenderFragment = (component) =>  (builder) =>
                    {
                        builder.OpenComponent(0, typeof(Select<string, string>));
                        builder.CloseComponent();
                    },
                    ComponentProperty = new(){
                        Title = "选择器",
                        ComponentValueType = ComponentValueType.String
                    }
                },
                new(ComponentType:"autocomplete"){
                    RenderFragment = (component) =>  (builder) =>
                    {
                        builder.OpenComponent(0, typeof(AutoComplete<string>));
                        builder.CloseComponent();
                    },
                    ComponentProperty = new(){
                        Title = "自动完成",
                        ComponentValueType = ComponentValueType.String
                    }
                },
                new(ComponentType:"cascader"){
                    RenderFragment = (component) =>  (builder) =>
                    {
                        builder.OpenComponent(0, typeof(Cascader));
                        builder.CloseComponent();
                    },
                    ComponentProperty = new(){
                        Title = "级联选择",
                        ComponentValueType = ComponentValueType.String
                    }
                },
                new(ComponentType:"switch"){
                    RenderFragment = (component) =>  (builder) =>
                    {
                        builder.OpenComponent(0, typeof(Switch));
                        builder.CloseComponent();
                    },
                    ComponentProperty = new(){
                        Title = "开关",
                        ComponentValueType = ComponentValueType.Boolean
                    }
                },
                new(ComponentType:"datepicker"){
                    RenderFragment = (component) =>  (builder) =>
                    {
                        builder.OpenComponent(0, typeof(DatePicker<DateTime?>));
                        builder.AddAttribute(1, "Picker", DatePickerType.Date);
                        builder.CloseComponent();
                    },
                    ComponentProperty = new(){
                        Title = "日期选择",
                        ComponentValueType = ComponentValueType.Date
                    }
                },
                new(ComponentType:"timepicker"){
                    RenderFragment = (component) =>  (builder) =>
                    {
                        builder.OpenComponent(0, typeof(TimePicker<DateTime?>));
                        builder.CloseComponent();
                    },
                    ComponentProperty = new(){
                        Title = "时间选择",
                        ComponentValueType = ComponentValueType.Date
                    }
                },
                new(ComponentType:"button"){
                    IsHiddenTitle = true,
                    RenderFragment = (component) =>  (builder) =>
                    {
                        builder.OpenComponent(0, typeof(Button));
                        builder.AddAttribute(1,  "Type", ButtonType.Default);
                        builder.AddContent(2, component.ComponentProperty.Title);
                        builder.CloseComponent();
                    },
                    ComponentProperty = new(){
                        Title = "按钮",
                        ComponentValueType = ComponentValueType.None
                    }
                }
            ];
            return components;
        }
    }
}
