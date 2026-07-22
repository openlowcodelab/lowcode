using System.ComponentModel.DataAnnotations;

namespace H.LowCode.MetaSchema.DesignEngine;

/// <summary>
/// 页面模板分类
/// </summary>
public enum PageTemplateCategoryEnum
{
    [Display(Name = "空白页")]
    Blank = 0,

    [Display(Name = "仪表盘")]
    Dashboard = 1,

    [Display(Name = "表单页")]
    Form = 2,

    [Display(Name = "列表页")]
    List = 3,

    [Display(Name = "统计页")]
    Statistic = 4,

    [Display(Name = "其他")]
    Other = 99
}
