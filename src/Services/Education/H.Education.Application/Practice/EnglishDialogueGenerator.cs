using H.Education.Application.Contracts;

namespace H.Education.Application.Practice;

/// <summary>
/// 英语对话练习出题器：给上句，由学生自己输入英文应答（不再选选项）。
/// 判定只要求参考答案的关键词都说到，"I'm fine, thanks." 写成 "fine thanks" 或 "I am fine" 都算对；
/// 第 1~3 关给出中文提示（想说：…），第 4 关起只给场景，得自己组织句子。
/// </summary>
public class EnglishDialogueGenerator : IPracticeItemGenerator
{
    public PracticeGameTypeEnum GameType => PracticeGameTypeEnum.EnglishDialogue;

    public EducationSubjectEnum Subject => EducationSubjectEnum.English;

    public List<GeneratedPracticeItem> Generate(string grade, int level, int count)
    {
        var band = Math.Clamp(EducationGrades.Band(grade), 0, 2);
        var pool = PracticeContentBanks.EnglishDialogues[band];
        var showChinese = Math.Clamp(level, 1, 6) <= 3;

        var items = new List<GeneratedPracticeItem>(count);
        for (var i = 0; i < count; i++)
        {
            var picked = pool[Random.Shared.Next(pool.Length)];
            var blankCount = picked.Reply.Split('|', StringSplitOptions.RemoveEmptyEntries)
                .First().Split(' ', StringSplitOptions.RemoveEmptyEntries).Length;

            items.Add(new GeneratedPracticeItem(
                PracticeInputTypeEnum.Typing,
                $"根据上句输入你的英文应答：\nA: {picked.Ask}\nB: ____",
                [],
                picked.Reply,
                showChinese
                    ? $"情景：{picked.Scene} · 想说：{picked.ReplyZh}"
                    : $"情景：{picked.Scene}，用英文回答")
            { AudioText = picked.Ask, LooseMatch = true, BlankCount = blankCount });
        }

        return items;
    }
}
