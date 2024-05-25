using AntDesign;
using H.LowCode.MetaSchema;
using Microsoft.AspNetCore.Components;
using H.LowCode.DesignEngine.Abstraction;

namespace H.LowCode.DesignEngine.DefaultComponents.ComponentProviders
{
    public class BasicComponentProvider : IComponentProvider
    {
        public string Title { get; set; } = "基础组件";

        public IEnumerable<ComponentSchema> LoadComponent()
        {
            List<ComponentSchema> components =
            [
                new("input"){
                    RenderFragment = (component) =>  (builder) =>
                    {
                        builder.OpenComponent(1, typeof(Input<string>));
                        builder.CloseComponent();
                    },
                    ComponentProperty = new(){
                        Title = "输入框",
                        ComponentValueType = ComponentValueType.String,
                        SupportProperties = ["MaximumLength", "MinimumLength", "Pattern", "Format"],
                        ExtensionProperties =
                            [
                                new() { Label="最大长度", IntValue = 0, SettingItemType = PropertyItemTypeEnum.Text_Int },
                                new() { Label="是否只读", BoolValue = false, SettingItemType = PropertyItemTypeEnum.Checkbox },
                                new() { Label="是否截断", BoolValue = false, SettingItemType = PropertyItemTypeEnum.Checkbox },
                                new() { Label="输入提示", StringValue = "", SettingItemType = PropertyItemTypeEnum.Text, Description = "placehoder 显示文字" }
                            ]
                    }
                },
                new("inputnumber"){
                    RenderFragment = (component) =>  (builder) =>
                    {
                        builder.OpenComponent(0, typeof(InputNumber<int>));
                        builder.CloseComponent();
                    },
                    ComponentProperty = new(){
                        Title = "数字输入框",
                        ComponentValueType = ComponentValueType.Integer,
                        SupportProperties = ["Maximum", "Minimum"]
                    }
                },
                new("textarea"){
                    RenderFragment = (component) =>  (builder) =>
                    {
                        builder.OpenComponent(0, typeof(TextArea));
                        builder.AddAttribute(1, "style", "height:50px;");
                        builder.CloseComponent();
                    },
                    ComponentProperty= new(){
                        Title = "大输入框",
                        ComponentValueType = ComponentValueType.String,
                        SupportProperties = ["MaximumLength", "MinimumLength"]
                    }
                },
                new("checkbox"){
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
                new("radio"){
                    RenderFragment = (component) =>  (builder) =>
                    {
                        builder.OpenComponent(0, typeof(Radio<string>));
                        builder.CloseComponent();
                    },
                    ComponentProperty = new(){
                        Title = "单选框"
                    }
                },
                new("select"){
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
                new("autocomplete"){
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
                new("cascader"){
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
                new("switch"){
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
                new("datepicker"){
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
                new("timepicker"){
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
                new("button"){
                    IsHiddenTitle = true,
                    RenderFragment = (component) =>  (builder) =>
                    {
                        builder.OpenComponent(0, typeof(Button));
                        RenderFragment fragment = (builder) => builder.AddMarkupContent(1, component.ComponentProperty.Title);
                        builder.AddComponentParameter(1, "ChildContent", fragment);
                        builder.CloseComponent();
                    },
                    ComponentProperty = new(){
                        Title = "按钮",
                        ComponentValueType = ComponentValueType.None
                    },
                    ComponentStyle = new()
                    {
                        ItemWidth = 2
                    }
                }
            ];
            return components;
        }
    }
}
