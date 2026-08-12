using System.ComponentModel.DataAnnotations;

namespace H.LowCode.MetaSchema;

public enum PageTypeEnum
{
    [Display(Name = "普通")]
    Normal = 0,
    [Display(Name = "表单")]
    Form = 1,
    [Display(Name = "列表")]
    Table = 2,
    [Display(Name = "报表")]
    Report = 5
}