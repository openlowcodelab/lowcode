using System.ComponentModel.DataAnnotations;

namespace H.LowCode.MetaSchema;

public enum SupportPlatformEnum
{
    [Display(Name = "Web")]
    Web,
    [Display(Name = "App")]
    Mobile,
    [Display(Name = "小程序")]
    WXMiniApp
}
