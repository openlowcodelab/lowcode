using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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