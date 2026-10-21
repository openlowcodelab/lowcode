using H.Education.Application.Contracts;

namespace H.Education.Application.Textbook;

/// <summary>
/// 内置的公开教材目录：登记到「单元」粒度，五类课时资源按单元逐类展开。
/// 学科、版本、年级、册次、单元编排均取自公开教材信息；具体课文/课时标题与资源直链需要在平台内核对后补录，
/// 因此这里不预置任何未经核实的课文名称。
/// </summary>
internal static class TextbookCatalogData
{
    /// <summary>教材册种子</summary>
    internal sealed record Seed(
        string Name,
        SchoolStageEnum Stage,
        TextbookSubjectEnum Subject,
        string Version,
        string Grade,
        TextbookVolumeEnum Volume,
        string Publisher,
        int UnitCount,
        string UnitLabel,
        string? Remark,
        /// <summary>ChinaTextbook 仓库内的 PDF 相对路径，null 表示该册没有可用电子课本</summary>
        string? EbookPath);

    private const string RenJiao = "人民教育出版社";
    private const string WaiYan = "外语教学与研究出版社";

    /// <summary>内置目录，按 小学 → 初中 → 高中、主科优先排列</summary>
    internal static readonly Seed[] All =
    [
        // 小学
        new("一年级语文上册（统编版）", SchoolStageEnum.Primary, TextbookSubjectEnum.Chinese, "统编版", "一年级", TextbookVolumeEnum.First, RenJiao, 8, "第{0}单元", null, "小学/语文/统编版/义务教育教科书·语文一年级上册.pdf"),
        new("三年级语文上册（统编版）", SchoolStageEnum.Primary, TextbookSubjectEnum.Chinese, "统编版", "三年级", TextbookVolumeEnum.First, RenJiao, 8, "第{0}单元", null, "小学/语文/统编版/义务教育教科书·语文三年级上册.pdf"),
        new("五年级语文上册（统编版）", SchoolStageEnum.Primary, TextbookSubjectEnum.Chinese, "统编版", "五年级", TextbookVolumeEnum.First, RenJiao, 8, "第{0}单元", null, "小学/语文/统编版/义务教育教科书·语文五年级上册.pdf"),
        new("一年级数学上册（人教版）", SchoolStageEnum.Primary, TextbookSubjectEnum.Math, "人教版", "一年级", TextbookVolumeEnum.First, RenJiao, 6, "第{0}单元", "人教版一年级上册以数与运算、认识图形等主题编排，未显式标注单元号", "小学/数学/人教版/义务教育教科书 · 数学一年级上册.pdf"),
        new("三年级数学上册（人教版）", SchoolStageEnum.Primary, TextbookSubjectEnum.Math, "人教版", "三年级", TextbookVolumeEnum.First, RenJiao, 8, "第{0}单元", null, "小学/数学/人教版/义务教育教科书 · 数学三年级上册.pdf"),
        new("五年级数学上册（人教版）", SchoolStageEnum.Primary, TextbookSubjectEnum.Math, "人教版", "五年级", TextbookVolumeEnum.First, RenJiao, 6, "第{0}单元", null, "小学/数学/人教版/义务教育教科书 · 数学五年级上册.pdf"),
        new("三年级英语上册（人教版）", SchoolStageEnum.Primary, TextbookSubjectEnum.English, "人教版", "三年级", TextbookVolumeEnum.First, RenJiao, 6, "Unit {0}", "人教版 PEP 三年级英语上册", "小学/英语/人教版（PEP）（三年级起点）（主编：吴欣）/义务教育教科书·英语（PEP）（三年级起点）三年级上册.pdf"),

        // 初中
        new("七年级语文上册（统编版）", SchoolStageEnum.JuniorHigh, TextbookSubjectEnum.Chinese, "统编版", "七年级", TextbookVolumeEnum.First, RenJiao, 6, "第{0}单元", null, "初中/语文/统编版-人民教育出版社/七年级/义务教育教科书·语文七年级上册.pdf"),
        new("九年级语文上册（统编版）", SchoolStageEnum.JuniorHigh, TextbookSubjectEnum.Chinese, "统编版", "九年级", TextbookVolumeEnum.First, RenJiao, 6, "第{0}单元", null, "初中/语文/统编版-人民教育出版社/九年级/义务教育教科书·语文九年级上册.pdf"),
        new("七年级数学上册（人教版）", SchoolStageEnum.JuniorHigh, TextbookSubjectEnum.Math, "人教版", "七年级", TextbookVolumeEnum.First, RenJiao, 4, "第{0}章", null, "初中/数学/人教版-人民教育出版社/七年级/义务教育教科书·数学七年级上册.pdf"),
        new("八年级数学上册（人教版）", SchoolStageEnum.JuniorHigh, TextbookSubjectEnum.Math, "人教版", "八年级", TextbookVolumeEnum.First, RenJiao, 6, "第{0}章", null, "初中/数学/人教版-人民教育出版社/八年级/义务教育教科书·数学八年级上册.pdf"),
        new("七年级英语上册（人教版）", SchoolStageEnum.JuniorHigh, TextbookSubjectEnum.English, "人教版", "七年级", TextbookVolumeEnum.First, RenJiao, 8, "Unit {0}", "含 Starter Unit，平台课程教学按正式单元登记", "初中/英语/人教版-人民教育出版社/七年级/义务教育教科书·英语七年级上册.pdf"),
        new("七年级道德与法治上册（统编版）", SchoolStageEnum.JuniorHigh, TextbookSubjectEnum.EthicsAndRuleOfLaw, "统编版", "七年级", TextbookVolumeEnum.First, RenJiao, 4, "第{0}单元", null, "初中/道德与法治/统编版-人民教育出版社/七年级/义务教育教科书·道德与法治七年级上册.pdf"),

        // 高中
        new("高一语文必修上册（统编版）", SchoolStageEnum.SeniorHigh, TextbookSubjectEnum.Chinese, "统编版", "高一", TextbookVolumeEnum.First, RenJiao, 8, "第{0}单元", null, "高中/语文/统编版-人民教育出版社/普通高中教科书·语文必修 上册.pdf"),
        new("高一数学必修第一册（人教A版）", SchoolStageEnum.SeniorHigh, TextbookSubjectEnum.Math, "人教A版", "高一", TextbookVolumeEnum.First, RenJiao, 5, "第{0}章", "目录取自公开教材信息，只到单元级；课时名称与资源直链需在平台内核对后补录。该册 PDF 在 ChinaTextbook 仓库里按 .pdf.1/.pdf.2 分卷存放，没有可直接打开的单文件，故未登记电子课本直链", null),
        new("高一英语必修第一册（外研版）", SchoolStageEnum.SeniorHigh, TextbookSubjectEnum.English, "外研版", "高一", TextbookVolumeEnum.First, WaiYan, 6, "Unit {0}", null, "高中/英语/外研社版-外语教学与研究出版社/普通高中教科书·英语必修 第一册.pdf"),
        new("高一化学必修第一册（人教版）", SchoolStageEnum.SeniorHigh, TextbookSubjectEnum.Chemistry, "人教版", "高一", TextbookVolumeEnum.First, RenJiao, 4, "第{0}章", null, "高中/化学/人教版-人民教育出版社/普通高中教科书·化学必修 第一册.pdf"),
        new("高一生物学必修1分子与细胞（人教版）", SchoolStageEnum.SeniorHigh, TextbookSubjectEnum.Biology, "人教版", "高一", TextbookVolumeEnum.First, RenJiao, 6, "第{0}章", null, "高中/生物学/人教版-人民教育出版社/普通高中教科书·生物学必修1 分子与细胞.pdf")
    ];

    private static readonly string[] ChineseNumerals =
    [
        "零", "一", "二", "三", "四", "五", "六", "七", "八", "九", "十",
        "十一", "十二", "十三", "十四", "十五", "十六", "十七", "十八", "十九", "二十"
    ];

    /// <summary>把单元序号套进单元名称样式，"第{0}单元"、"第{0}章" 用中文数字，"Unit {0}" 用阿拉伯数字</summary>
    public static string FormatUnit(string? unitLabel, int index)
    {
        var label = string.IsNullOrWhiteSpace(unitLabel) ? "第{0}单元" : unitLabel;
        var chinese = label.Contains('第');
        var text = chinese && index < ChineseNumerals.Length ? ChineseNumerals[index] : index.ToString();
        return string.Format(label, text);
    }
}
