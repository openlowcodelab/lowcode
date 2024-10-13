using AntDesign;
using H.LowCode.MetaSchema;
using H.LowCode.DesignEngine.Abstraction;
using System.Text.Json;

namespace H.LowCode.ComponentParts.BasicComponents;

public class BasicComponentProvider : IComponentProvider
{
    public string Title { get; set; } = "基础组件";

    public IEnumerable<ComponentSchema> LoadComponent()
    {
        List<ComponentSchema> components =
        [
            new("input"){
                ComponentFragments =
                [
                    new(){ Index = 0, FragmentEnum = FragmentEnum.Component, ComponentFragmentName = typeof(Input<string>).GetFullNameWithAssemblyName() }
                ],
                ComponentProperty = new(){
                    Title = "输入框",
                    ComponentValueType = ComponentValueTypeEnum.String,
                    SupportProperties = ["MaximumLength", "MinimumLength", "Pattern", "Format"],
                    ExtraProperties = new Dictionary<string,PropertyItemSchema>
                    {
                        { "maxlength", new() { Label="最大长度", IntValue = 0, SettingItemType = PropertyItemTypeEnum.Text_Int } },
                        { "isreadonly", new() { Label="是否只读", BoolValue = false, SettingItemType = PropertyItemTypeEnum.Checkbox } },
                        { "isslice", new() { Label="是否截断", BoolValue = false, SettingItemType = PropertyItemTypeEnum.Checkbox } },
                        { "tips", new() { Label="输入提示", StringValue = "", SettingItemType = PropertyItemTypeEnum.Text, Description = "placehoder 显示文字" } }
                    }
                }
            },
            new("inputnumber"){
                ComponentFragments =
                [
                    new(){ Index = 0, FragmentEnum = FragmentEnum.Component, ComponentFragmentName = typeof(InputNumber<int>).GetFullNameWithAssemblyName()}
                ],
                ComponentProperty = new(){
                    Title = "数字输入框",
                    ComponentValueType = ComponentValueTypeEnum.Integer,
                    SupportProperties = ["Maximum", "Minimum"]
                }
            },
            new("textarea"){
                ComponentFragments =
                [
                    new(){ Index = 0, FragmentEnum = FragmentEnum.Component, ComponentFragmentName = typeof(TextArea).GetFullNameWithAssemblyName()},
                    new(){ Index = 1, FragmentEnum = FragmentEnum.Attribute, Name = "style", ValueType = ComponentValueTypeEnum.String, StringValue = "height:50px;" }
                ],
                ComponentProperty= new(){
                    Title = "大输入框",
                    ComponentValueType = ComponentValueTypeEnum.String,
                    SupportProperties = ["MaximumLength", "MinimumLength"]
                }
            },
            new("checkbox"){
                ComponentFragments =
                [
                    new(){ Index = 0, FragmentEnum = FragmentEnum.Component, ComponentFragmentName = typeof(Checkbox).GetFullNameWithAssemblyName() }
                ],
                ComponentProperty = new(){
                    Title = "多选框",
                    IsSupportDataSource = true,
                    ComponentValueType = ComponentValueTypeEnum.Boolean
                }
            },
            new("radio"){
                ComponentFragments =
                [
                    new(){ Index = 0, FragmentEnum = FragmentEnum.Component, ComponentFragmentName = typeof(Radio<string>).GetFullNameWithAssemblyName()  }
                ],
                ComponentProperty = new(){
                    Title = "单选框",
                    IsSupportDataSource = true
                }
            },
            new("select"){
                ComponentFragments =
                [
                    new(){ Index = 0, FragmentEnum = FragmentEnum.Component, ComponentFragmentName = typeof(Select<string, string>).GetFullNameWithAssemblyName() }
                ],
                ComponentProperty = new(){
                    Title = "选择器",
                    IsSupportDataSource = true,
                    ComponentValueType = ComponentValueTypeEnum.String
                }
            },
            new("autocomplete"){
                ComponentFragments =
                [
                    new(){ Index = 0, FragmentEnum = FragmentEnum.Component, ComponentFragmentName = typeof(AutoComplete<string>).GetFullNameWithAssemblyName() }
                ],
                ComponentProperty = new(){
                    Title = "自动完成",
                    IsSupportDataSource = true,
                    ComponentValueType = ComponentValueTypeEnum.String
                }
            },
            new("cascader"){
                ComponentFragments =
                [
                    new(){ Index = 0, FragmentEnum = FragmentEnum.Component, ComponentFragmentName = typeof(Cascader).GetFullNameWithAssemblyName() }
                ],
                ComponentProperty = new(){
                    Title = "级联选择",
                    IsSupportDataSource = true,
                    ComponentValueType = ComponentValueTypeEnum.String
                }
            },
            new("switch"){
                ComponentFragments =
                [
                    new(){ Index = 0, FragmentEnum = FragmentEnum.Component, ComponentFragmentName = typeof(Switch).GetFullNameWithAssemblyName() }
                ],
                ComponentProperty = new(){
                    Title = "开关",
                    ComponentValueType = ComponentValueTypeEnum.Boolean
                }
            },
            new("datepicker"){
                ComponentFragments =
                [
                    new(){ Index = 0, FragmentEnum = FragmentEnum.Component, ComponentFragmentName = typeof(DatePicker<DateTime?>).GetFullNameWithAssemblyName() },
                    new(){ Index = 1, FragmentEnum = FragmentEnum.Attribute, Name = "Picker", ValueType = ComponentValueTypeEnum.Integer, IntValue = DatePickerType.Date.Value }
                ],
                ComponentProperty = new(){
                    Title = "日期选择",
                    ComponentValueType = ComponentValueTypeEnum.Date
                }
            },
            new("timepicker"){
                ComponentFragments =
                [
                    new(){ Index = 0, FragmentEnum = FragmentEnum.Component, ComponentFragmentName = typeof(TimePicker<DateTime?>).GetFullNameWithAssemblyName() }
                ],
                ComponentProperty = new(){
                    Title = "时间选择",
                    ComponentValueType = ComponentValueTypeEnum.Date
                }
            },
            new("button"){
                IsHiddenTitle = true,
                ComponentFragments =
                [
                    new(){ Index = 0, FragmentEnum = FragmentEnum.Component, ComponentFragmentName = typeof(LcButton).GetFullNameWithAssemblyName() },
                    new(){ Index = 1, FragmentEnum = FragmentEnum.Parameter, Name = "Text", ValueType = ComponentValueTypeEnum.String, StringValue = "按钮" }
                ],
                ComponentProperty = new(){
                    Title = "按钮",
                    ComponentValueType = ComponentValueTypeEnum.None
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
