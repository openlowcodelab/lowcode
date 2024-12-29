using AntDesign;
using H.LowCode.MetaSchema;
using H.LowCode.DesignEngine.Abstraction;
using System.Text.Json;
using H.LowCode.PartsMetaSchema;

namespace H.LowCode.Components.AntBlazor;

public class BasicComponentProvider : IComponentProvider
{
    public string Name { get; } = "BasicComponent";

    public string Label { get; } = "基础组件";

    public IEnumerable<ComponentPartsSchema> LoadComponents()
    {
        List<ComponentPartsSchema> componentParts =
        [
            new(){
                ComponentName = "input",
                Label = "输入框",
                Fragment = new(){
                    DefaultFullTypeName = "AntDesign.Input`1[System.String], AntDesign",
                    FullTypeOptions = new Dictionary<string, string>(){
                        {"Input<string>", "字符串" },
                        {"Input<int>", "数值" }
                    },
                    Parameters = []
                },
                //Property = new(){
                //    IsSupportDataSource = false,
                    //ExtraProperties = new Dictionary<string,PropertyItemSchema>
                    //{
                    //    { "maxlength", new() { Label="最大长度", IntValue = 0, SettingItemType = PropertyItemTypeEnum.Text_Int } },
                    //    { "isreadonly", new() { Label="是否只读", BoolValue = false, SettingItemType = PropertyItemTypeEnum.Checkbox } },
                    //    { "isslice", new() { Label="是否截断", BoolValue = false, SettingItemType = PropertyItemTypeEnum.Checkbox } },
                    //    { "tips", new() { Label="输入提示", StringValue = "", SettingItemType = PropertyItemTypeEnum.Text, Description = "placehoder 显示文字" } }
                    //}
                //},
                Style = new(){

                },
                Event = new(){

                },
                Childrens = [],
                Order = 0,
                PublishStatus = 1
            }
        ];
        return componentParts;
    }

    //public IEnumerable<ComponentSchema> LoadComponent()
    //{
    //    List<ComponentSchema> components =
    //    [
    //        new("input"){
    //            ComponentFragments =
    //            [
    //                new(){ Index = 0, FragmentEnum = FragmentEnum.Component, ComponentFragmentName = typeof(Input<string>).GetFullNameWithAssemblyName() }
    //            ],
    //            Property = new(){
    //                Label = "输入框",
    //                ComponentValueType = ComponentValueTypeEnum.String,
    //                SupportProperties = ["MaximumLength", "MinimumLength", "Pattern", "Format"],
    //                ExtraProperties = new Dictionary<string,PropertyItemSchema>
    //                {
    //                    { "maxlength", new() { Label="最大长度", IntValue = 0, SettingItemType = PropertyItemTypeEnum.Text_Int } },
    //                    { "isreadonly", new() { Label="是否只读", BoolValue = false, SettingItemType = PropertyItemTypeEnum.Checkbox } },
    //                    { "isslice", new() { Label="是否截断", BoolValue = false, SettingItemType = PropertyItemTypeEnum.Checkbox } },
    //                    { "tips", new() { Label="输入提示", StringValue = "", SettingItemType = PropertyItemTypeEnum.Text, Description = "placehoder 显示文字" } }
    //                }
    //            }
    //        },
    //        new("inputnumber"){
    //            ComponentFragments =
    //            [
    //                new(){ Index = 0, FragmentEnum = FragmentEnum.Component, ComponentFragmentName = typeof(InputNumber<int>).GetFullNameWithAssemblyName()}
    //            ],
    //            Property = new(){
    //                Label = "数字输入框",
    //                ComponentValueType = ComponentValueTypeEnum.Integer,
    //                SupportProperties = ["Maximum", "Minimum"]
    //            }
    //        },
    //        new("textarea"){
    //            ComponentFragments =
    //            [
    //                new(){ Index = 0, FragmentEnum = FragmentEnum.Component, ComponentFragmentName = typeof(TextArea).GetFullNameWithAssemblyName()},
    //                new(){ Index = 1, FragmentEnum = FragmentEnum.Attribute, Name = "style", ValueType = ComponentValueTypeEnum.String, StringValue = "height:50px;" }
    //            ],
    //            Property= new(){
    //                Label = "大输入框",
    //                ComponentValueType = ComponentValueTypeEnum.String,
    //                SupportProperties = ["MaximumLength", "MinimumLength"]
    //            }
    //        },
    //        new("checkbox"){
    //            ComponentFragments =
    //            [
    //                new(){ Index = 0, FragmentEnum = FragmentEnum.Component, ComponentFragmentName = typeof(Checkbox).GetFullNameWithAssemblyName() }
    //            ],
    //            Property = new(){
    //                Label = "多选框",
    //                IsSupportDataSource = true,
    //                ComponentValueType = ComponentValueTypeEnum.Boolean
    //            }
    //        },
    //        new("radio"){
    //            ComponentFragments =
    //            [
    //                new(){ Index = 0, FragmentEnum = FragmentEnum.Component, ComponentFragmentName = typeof(Radio<string>).GetFullNameWithAssemblyName()  }
    //            ],
    //            Property = new(){
    //                Label = "单选框",
    //                IsSupportDataSource = true
    //            }
    //        },
    //        new("select"){
    //            ComponentFragments =
    //            [
    //                new(){ Index = 0, FragmentEnum = FragmentEnum.Component, ComponentFragmentName = typeof(Select<string, string>).GetFullNameWithAssemblyName() }
    //            ],
    //            Property = new(){
    //                Label = "选择器",
    //                IsSupportDataSource = true,
    //                ComponentValueType = ComponentValueTypeEnum.String
    //            }
    //        },
    //        new("autocomplete"){
    //            ComponentFragments =
    //            [
    //                new(){ Index = 0, FragmentEnum = FragmentEnum.Component, ComponentFragmentName = typeof(AutoComplete<string>).GetFullNameWithAssemblyName() },
    //                new(){ Index = 1, FragmentEnum = FragmentEnum.Attribute, Name = "Options", ValueType = ComponentValueTypeEnum.StringList, StringValues = [] }
    //            ],
    //            Property = new(){
    //                Label = "自动完成",
    //                IsSupportDataSource = true,
    //                ComponentValueType = ComponentValueTypeEnum.String
    //            }
    //        },
    //        new("cascader"){
    //            ComponentFragments =
    //            [
    //                new(){ Index = 0, FragmentEnum = FragmentEnum.Component, ComponentFragmentName = typeof(Cascader).GetFullNameWithAssemblyName() }
    //            ],
    //            Property = new(){
    //                Label = "级联选择",
    //                IsSupportDataSource = true,
    //                ComponentValueType = ComponentValueTypeEnum.String
    //            }
    //        },
    //        new("switch"){
    //            ComponentFragments =
    //            [
    //                new(){ Index = 0, FragmentEnum = FragmentEnum.Component, ComponentFragmentName = typeof(Switch).GetFullNameWithAssemblyName() }
    //            ],
    //            Property = new(){
    //                Label = "开关",
    //                ComponentValueType = ComponentValueTypeEnum.Boolean
    //            }
    //        },
    //        new("datepicker"){
    //            ComponentFragments =
    //            [
    //                new(){ Index = 0, FragmentEnum = FragmentEnum.Component, ComponentFragmentName = typeof(DatePicker<DateTime?>).GetFullNameWithAssemblyName() },
    //                new(){ Index = 1, FragmentEnum = FragmentEnum.Attribute, Name = "Picker", ValueType = ComponentValueTypeEnum.Integer, IntValue = DatePickerType.Date.Value }
    //            ],
    //            Property = new(){
    //                Label = "日期选择",
    //                ComponentValueType = ComponentValueTypeEnum.Date
    //            }
    //        },
    //        new("timepicker"){
    //            ComponentFragments =
    //            [
    //                new(){ Index = 0, FragmentEnum = FragmentEnum.Component, ComponentFragmentName = typeof(TimePicker<DateTime?>).GetFullNameWithAssemblyName() }
    //            ],
    //            Property = new(){
    //                Label = "时间选择",
    //                ComponentValueType = ComponentValueTypeEnum.Date
    //            }
    //        },
    //        new("button"){
    //            IsHiddenTitle = true,
    //            ComponentFragments =
    //            [
    //                new(){ Index = 0, FragmentEnum = FragmentEnum.Component, ComponentFragmentName = typeof(LcButton).GetFullNameWithAssemblyName() },
    //                new(){ Index = 1, FragmentEnum = FragmentEnum.Parameter, Name = "Text", ValueType = ComponentValueTypeEnum.String, StringValue = "按钮" }
    //            ],
    //            Property = new(){
    //                Label = "按钮",
    //                ComponentValueType = ComponentValueTypeEnum.None
    //            },
    //            Style = new()
    //            {
    //                ItemWidth = 2
    //            }
    //        }
    //    ];
    //    return components;
    //}
}
