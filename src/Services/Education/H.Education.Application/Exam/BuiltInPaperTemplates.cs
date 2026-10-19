using H.Education.Application.Contracts;
using H.Education.Application.Exam;

namespace H.Education.Application.Exam;

/// <summary>
/// 内置样卷：按学科与学段提供一套可直接使用的试卷，
/// 用于未配置 AI 模型时快速体验完整的命题—答题—评分链路。
/// 题目顺序固定为 4 单选 + 4 填空 + 2 判断 + 2 解答，分值累加恰为 100 分。
/// </summary>
internal static class BuiltInPaperTemplates
{
    private record Item(
        QuestionTypeEnum Type,
        string Content,
        string Answer,
        string Analysis,
        string[]? Options = null);

    private static readonly decimal[] ScoreSkeleton = [8, 8, 8, 8, 7, 7, 7, 7, 6, 6, 14, 14];

    private static readonly string[] SectionSkeleton =
    [
        "一、单项选择题",
        "一、单项选择题",
        "一、单项选择题",
        "一、单项选择题",
        "二、填空题",
        "二、填空题",
        "二、填空题",
        "二、填空题",
        "三、判断题",
        "三、判断题",
        "四、解答与表达",
        "四、解答与表达"
    ];

    public static GeneratedPaper Build(GeneratePaperInputDto input)
    {
        var lowGrade = EducationGrades.Band(input.Grade) == 0;
        var items = input.Subject switch
        {
            EducationSubjectEnum.Chinese => lowGrade ? ChineseLow : ChineseHigh,
            EducationSubjectEnum.Math => lowGrade ? MathLow : MathHigh,
            EducationSubjectEnum.English => lowGrade ? EnglishLow : EnglishHigh,
            _ => MathLow
        };

        var questions = new List<GeneratedQuestion>();
        for (var i = 0; i < items.Count && i < ScoreSkeleton.Length; i++)
        {
            var item = items[i];
            questions.Add(new GeneratedQuestion(
                SectionSkeleton[i],
                item.Type,
                item.Content,
                BuildOptions(item.Options),
                item.Answer,
                item.Analysis,
                ScoreSkeleton[i]));
        }

        var term = string.IsNullOrWhiteSpace(input.Term) ? string.Empty : input.Term;
        var title = $"{input.Grade}{input.Subject.GetSubjectName()}{term}样卷（内置）";

        return new GeneratedPaper(title, input.DurationMinutes, questions,
            $"内置样卷：{input.Grade} {input.Subject.GetSubjectName()}（{EducationGrades.BandName(EducationGrades.Band(input.Grade))}）", null);
    }

    private static List<QuestionOptionDto> BuildOptions(string[]? options)
    {
        var result = new List<QuestionOptionDto>();
        if (options is null) return result;

        for (var i = 0; i < options.Length; i++)
        {
            result.Add(new QuestionOptionDto { Key = ((char)('A' + i)).ToString(), Text = options[i] });
        }
        return result;
    }

    private static readonly List<Item> MathLow =
    [
        new(QuestionTypeEnum.SingleChoice, "下面哪个数最大？", "B",
            "54 比 45、49、50 都大，选 B。比较两位数先看十位，十位大的数就大。",
            ["45", "54", "49", "50"]),
        new(QuestionTypeEnum.SingleChoice, "7 × 8 的结果是？", "B",
            "由乘法口诀「七八五十六」得 56，选 B。",
            ["54", "56", "64", "48"]),
        new(QuestionTypeEnum.SingleChoice, "一个三角形有几条边？", "B",
            "三角形由三条线段围成，有 3 条边、3 个角，选 B。",
            ["2", "3", "4", "5"]),
        new(QuestionTypeEnum.SingleChoice, "36 加上多少等于 50？", "A",
            "50 − 36 = 14，所以要加 14，选 A。",
            ["14", "16", "24", "86"]),
        new(QuestionTypeEnum.FillBlank, "计算：25 + 17 = ____", "42",
            "个位 5 + 7 = 12，向十位进 1，十位 2 + 1 + 1 = 4，结果是 42。"),
        new(QuestionTypeEnum.FillBlank, "计算：63 − 28 = ____", "35",
            "个位 3 减 8 不够，向十位借 1 当 10，13 − 8 = 5；十位 5 − 2 = 3，结果是 35。"),
        new(QuestionTypeEnum.FillBlank, "计算：9 × 6 = ____", "54",
            "乘法口诀「六九五十四」，结果是 54。"),
        new(QuestionTypeEnum.FillBlank, "一个星期有 7 天，4 个星期一共有 ____ 天。", "28",
            "求 4 个 7 是多少，用乘法 7 × 4 = 28 天。"),
        new(QuestionTypeEnum.TrueFalse, "判断：12 ÷ 3 的商是 4。", "对",
            "12 ÷ 3 = 4，商确实是 4，所以这句话正确。"),
        new(QuestionTypeEnum.TrueFalse, "判断：长方形的四条边都相等。", "错",
            "长方形是对边相等，四条边都相等的是正方形，所以这句话错误。"),
        new(QuestionTypeEnum.ShortAnswer,
            "小明有 48 张卡片，送给同学 15 张，后来又买了 20 张。小明现在有多少张卡片？写出算式和得数。",
            "48 - 15 + 20 = 53（张）",
            "评分要点：先算送出后剩下 48 − 15 = 33 张，再算买来 20 张后 33 + 20 = 53 张；列出正确算式得 8 分，得数 53 得 6 分。"),
        new(QuestionTypeEnum.Calculation,
            "学校把 36 本笔记本平均分给 9 个同学，每人分到几本？如果每人用掉 2 本，一共还剩几本？",
            "每人 4 本；还剩 18 本",
            "评分要点：36 ÷ 9 = 4 本（7 分）；每人用 2 本共用去 9 × 2 = 18 本，剩下 36 − 18 = 18 本（7 分）。")
    ];

    private static readonly List<Item> MathHigh =
    [
        new(QuestionTypeEnum.SingleChoice, "下面哪个数是质数？", "B",
            "2 只有 1 和它本身两个因数，是质数；51 = 3 × 17、9 = 3 × 3、15 = 3 × 5 都是合数。",
            ["51", "2", "9", "15"]),
        new(QuestionTypeEnum.SingleChoice, "长 8 厘米、宽 5 厘米的长方形，周长是多少厘米？", "B",
            "周长 =（长 + 宽）× 2 =（8 + 5）× 2 = 26 厘米，选 B。面积才是 40 平方厘米。",
            ["13", "26", "40", "52"]),
        new(QuestionTypeEnum.SingleChoice, "3/4 + 1/4 的结果是？", "C",
            "同分母分数相加，分母不变、分子相加：(3 + 1)/4 = 4/4 = 1，选 C。",
            ["1/2", "4/8", "1", "3/2"]),
        new(QuestionTypeEnum.SingleChoice, "下面算式中结果最大的是？", "D",
            "25 × 4 = 100，30 × 3 = 90，20 × 5 = 100，24 × 5 = 120，最大的是 24 × 5，选 D。",
            ["25 × 4", "30 × 3", "20 × 5", "24 × 5"]),
        new(QuestionTypeEnum.FillBlank, "把 0.75 化成最简分数是 ____。", "3/4|四分之三|4分之3",
            "0.75 = 75/100，分子分母同除以 25 得 3/4。"),
        new(QuestionTypeEnum.FillBlank, "一个三角形的三个内角和是 ____ 度。", "180",
            "任意三角形的内角和都是 180 度。"),
        new(QuestionTypeEnum.FillBlank, "计算：3.6 × 25 = ____。", "90",
            "3.6 × 25 = 3.6 × 100 ÷ 4 = 360 ÷ 4 = 90。"),
        new(QuestionTypeEnum.FillBlank, "数据 12、18、15、15 的平均数是 ____。", "15",
            "平均数 =（12 + 18 + 15 + 15）÷ 4 = 60 ÷ 4 = 15。"),
        new(QuestionTypeEnum.TrueFalse, "判断：一个数的 20% 等于这个数的 1/5。", "对",
            "20% = 20/100 = 1/5，两者表示同一个倍数关系，所以正确。"),
        new(QuestionTypeEnum.TrueFalse, "判断：面积相等的两个三角形一定完全一样（全等）。", "错",
            "面积只取决于底与高的乘积，形状可以不同，例如底 6 高 2 与底 4 高 3 的三角形面积都是 6，但并不全等。"),
        new(QuestionTypeEnum.Calculation,
            "一件衣服原价 200 元，商场打八五折出售。现价是多少元？比原价便宜了多少元？",
            "现价 170 元；便宜 30 元",
            "评分要点：八五折即按 85% 计算，200 × 0.85 = 170 元（8 分）；200 − 170 = 30 元（6 分）。"),
        new(QuestionTypeEnum.ShortAnswer,
            "甲、乙两地相距 360 千米，一辆汽车 3 小时行完全程的 2/3。照这样的速度，剩下的路程还要行多少小时？写出思路与得数。",
            "还要 1.5 小时",
            "评分要点：已行 360 × 2/3 = 240 千米，速度 240 ÷ 3 = 80 千米/时（7 分）；剩下 360 − 240 = 120 千米，120 ÷ 80 = 1.5 小时（7 分）。")
    ];

    private static readonly List<Item> ChineseLow =
    [
        new(QuestionTypeEnum.SingleChoice, "「明」的偏旁部首是？", "A",
            "「明」是左右结构，左边「日」是部首，右边「月」表音表义的一部分，故选 A。",
            ["日", "月", "目", "田"]),
        new(QuestionTypeEnum.SingleChoice, "下面哪个词语形容天气非常炎热？", "A",
            "「骄阳似火」指强烈的阳光像火一样热，形容天气炎热；「秋高气爽」是秋天，「冰天雪地」是冬天，「风和日丽」是暖和晴朗。",
            ["骄阳似火", "秋高气爽", "冰天雪地", "风和日丽"]),
        new(QuestionTypeEnum.SingleChoice, "「一（ ）小鸟」括号里应填的量词是？", "A",
            "小动物一般用「只」：一条鱼、一张纸、一头牛都不适用于小鸟。",
            ["只", "条", "张", "头"]),
        new(QuestionTypeEnum.SingleChoice, "下面哪个字的意思是「用眼睛看」？", "B",
            "「瞧」表示看；「听」用耳朵，「闻」用鼻子，「说」用嘴巴。",
            ["听", "瞧", "闻", "说"]),
        new(QuestionTypeEnum.FillBlank, "《静夜思》的作者是唐代诗人 ____。", "李白|李太白",
            "《静夜思》是李白的作品，通过月光写出思乡之情。"),
        new(QuestionTypeEnum.FillBlank, "「白毛浮绿水」的下一句是 ____。", "红掌拨清波",
            "出自骆宾王《咏鹅》：白毛浮绿水，红掌拨清波。"),
        new(QuestionTypeEnum.FillBlank, "把成语补充完整：守株待 ____。", "兔",
            "「守株待兔」出自《韩非子》，比喻不想努力而希望得到成功的侥幸心理。"),
        new(QuestionTypeEnum.FillBlank, "写出「远」的反义词：____。", "近",
            "「远」与「近」表示距离的相对关系，是反义词。"),
        new(QuestionTypeEnum.TrueFalse, "判断：「的」一般用在名词前面，例如「美丽的花」。", "对",
            "「的」后面通常跟名词，「地」后面跟动词，「得」前面是动词或形容词。"),
        new(QuestionTypeEnum.TrueFalse, "判断：「再」和「在」是同一个字，写句子时可以随便替换。", "错",
            "「再」表示重复或继续，「在」表示时间、地点或状态，两者不能混用。"),
        new(QuestionTypeEnum.ShortAnswer, "用「一边……一边……」写一句话，注意两件事要能同时做。",
            "示例：妈妈一边做饭一边听音乐。",
            "评分要点：正确使用关联词且两件事能同时进行得 8 分，句子通顺、没有错别字得 6 分。"),
        new(QuestionTypeEnum.Composition, "以「我的课间十分钟」为题，写 3~5 句话，写清楚你在课间做了什么、心情怎样。",
            "围绕课间活动写一段话，语句通顺，内容具体。",
            "评分要点：内容与题目相关（6 分）、语句通顺（4 分）有具体活动描写（4 分）。")
    ];

    private static readonly List<Item> ChineseHigh =
    [
        new(QuestionTypeEnum.SingleChoice, "下列词语中没有错别字的一项是？", "B",
            "应为「再接再厉」（A）、「走投无路」（C）、「意气用事」（D），只有「迫不及待」书写正确。",
            ["再接再励", "迫不及待", "走头无路", "义气用事"]),
        new(QuestionTypeEnum.SingleChoice, "下列句子中使用比喻修辞的一项是？", "B",
            "B 把月光比作薄纱，是比喻；A 是夸张，C「好像」表示猜测不是比喻，D 是拟人。",
            ["教室里安静得连针掉在地上都听得见。", "月光像一层薄纱洒在湖面上。", "他好像在哪里见过我。", "大海喘着粗气。"]),
        new(QuestionTypeEnum.SingleChoice, "《少年闰土》的作者是谁？", "A",
            "《少年闰土》节选自鲁迅的小说《故乡》。",
            ["鲁迅", "老舍", "沈从文", "巴金"]),
        new(QuestionTypeEnum.SingleChoice, "「温故而知新，可以为师矣」出自哪部经典？", "B",
            "这句话出自《论语·为政》，强调在复习旧知识中获得新体会。",
            ["《孟子》", "《论语》", "《大学》", "《中庸》"]),
        new(QuestionTypeEnum.FillBlank, "补全诗句：少壮不努力，____。", "老大徒伤悲",
            "出自汉乐府《长歌行》，劝人珍惜时间。"),
        new(QuestionTypeEnum.FillBlank, "补全名句：先天下之忧而忧，____。", "后天下之乐而乐",
            "出自范仲淹《岳阳楼记》，表达以天下为己任的政治抱负。"),
        new(QuestionTypeEnum.FillBlank, "成语「卧薪尝胆」讲的是 ____（人名）的故事。", "勾践",
            "「卧薪尝胆」讲的是越王勾践战败后刻苦自励、最终复国的故事。"),
        new(QuestionTypeEnum.FillBlank, "「人固有一死，或重于泰山，或轻于鸿毛」出自 ____（作者）的《报任安书》。", "司马迁",
            "这句话出自西汉司马迁写给友人任安的信，表明自己忍辱著史的心志。"),
        new(QuestionTypeEnum.TrueFalse, "判断：说明书、通知、启事都属于应用文。", "对",
            "三者都是为处理实际事务而写的常用文体，属于应用文范畴。"),
        new(QuestionTypeEnum.TrueFalse, "判断：「他跑得很快」中「很快」是这句话的主语。", "错",
            "主语是「他」，「很快」补充说明「跑」的程度，是补语。"),
        new(QuestionTypeEnum.ShortAnswer, "结合自己的学习，说说你对「温故而知新」的理解。",
            "示例：复习学过的内容时能有新的体会和发现，这样的学习方法才有效。",
            "评分要点：解释句意（7 分）、结合自身学习举例（7 分）。"),
        new(QuestionTypeEnum.Composition, "以「一次难忘的尝试」为题写一篇 400 字左右的记叙文，要求写清过程，并有心理描写。",
            "记叙文，过程完整、有细节与心理描写。",
            "评分要点：内容具体、中心明确（6 分），条理清楚、详略得当（4 分），有真实心理描写（3 分），语言通顺、书写规范（1 分）。")
    ];

    private static readonly List<Item> EnglishLow =
    [
        new(QuestionTypeEnum.SingleChoice, "单词 「apple」 的中文意思是？", "A",
            "apple 是「苹果」；banana 香蕉，orange 橘子/橙色，grape 葡萄。",
            ["苹果", "香蕉", "橘子", "葡萄"]),
        new(QuestionTypeEnum.SingleChoice, "打招呼说「你好」，英文应该用？", "B",
            "Hello 用于打招呼；Goodbye 告别，Thank you 致谢，Sorry 道歉。",
            ["Goodbye", "Hello", "Thank you", "Sorry"]),
        new(QuestionTypeEnum.SingleChoice, "选出与其他三个不同类的一项：", "C",
            "cat、dog、bird 都是动物，red 是颜色，与其它三项不同类。",
            ["cat", "dog", "red", "bird"]),
        new(QuestionTypeEnum.SingleChoice, "「I ___ a student.」 空格处应填？", "A",
            "主语是 I，be 动词用 am；is 配第三人称单数，are 配复数或 you。",
            ["am", "is", "are", "be"]),
        new(QuestionTypeEnum.FillBlank, "写出 「book」 的中文意思：____", "书|书本",
            "book 表示「书、书本」。"),
        new(QuestionTypeEnum.FillBlank, "补全单词：_at（吃）____", "e",
            "eat 表示「吃」，缺少的首字母是 e。"),
        new(QuestionTypeEnum.FillBlank, "写出 「two」 后面那个数字的英文单词：____", "three",
            "two 是 2，后面是 3，英文为 three。"),
        new(QuestionTypeEnum.FillBlank, "把 「猫」 翻译成英文单词：____", "cat",
            "猫的英文是 cat。"),
        new(QuestionTypeEnum.TrueFalse, "判断：「Thank you」 表示「谢谢你」。", "对",
            "Thank you 用于表达感谢。"),
        new(QuestionTypeEnum.TrueFalse, "判断：「orange」 只能表示「橙子」，不能表示颜色。", "错",
            "orange 既能表示橙子，也能表示橙色。"),
        new(QuestionTypeEnum.ShortAnswer, "把句子翻译成英文：「我有一个苹果。」",
            "I have an apple.",
            "评分要点：主语 I 与谓语 have 正确（4 分），冠词用 an（4 分），单词拼写正确（6 分）。"),
        new(QuestionTypeEnum.Composition, "用 3~5 句英文介绍你的家庭，可用提示词：family, father, mother。",
            "围绕家庭成员用简单句介绍，如 There are ... in my family.",
            "评分要点：句子数量达标（6 分）、内容切题（4 分）、拼写与基本句式正确（4 分）。")
    ];

    private static readonly List<Item> EnglishHigh =
    [
        new(QuestionTypeEnum.SingleChoice, "「He ___ to school by bike every day.」 空格处应填？", "B",
            "主语 He 是第三人称单数，一般现在时动词加 -es，用 goes。",
            ["go", "goes", "going", "went"]),
        new(QuestionTypeEnum.SingleChoice, "下列与 「beautiful」 意思相近的词是？", "B",
            "pretty 与 beautiful 都表示「漂亮的」；ugly 是反义词，quiet 安静，common 普通。",
            ["ugly", "pretty", "quiet", "common"]),
        new(QuestionTypeEnum.SingleChoice, "「I have ___ homework today.」 空格处应填？", "B",
            "homework 是不可数名词，用 much 修饰；many、a few、several 都修饰可数名词复数。",
            ["many", "much", "a few", "several"]),
        new(QuestionTypeEnum.SingleChoice, "下列哪个是动词的过去式形式？", "A",
            "walked 是一般过去时；walk 是原形，walking 是现在分词，walks 是第三人称单数现在时。",
            ["walked", "walk", "walking", "walks"]),
        new(QuestionTypeEnum.FillBlank, "写出 「library」 的中文意思：____", "图书馆",
            "library 表示「图书馆、藏书室」。"),
        new(QuestionTypeEnum.FillBlank, "用括号内动词的适当形式填空：She ___ (read) English every morning.", "reads",
            "every morning 表示习惯，用一般现在时；主语 She 三单，动词加 s，读作 reads。"),
        new(QuestionTypeEnum.FillBlank, "把 「Three hundred and fifty」 写成数字：____", "350",
            "three hundred 是 300，and fifty 是 50，合起来 350。"),
        new(QuestionTypeEnum.FillBlank, "把 「健康」 翻译成英文单词：____", "health",
            "health 是名词「健康」，形容词是 healthy。"),
        new(QuestionTypeEnum.TrueFalse, "判断：「How much is this book?」 是在询问价格。", "对",
            "How much 用于询问不可数名词数量或商品价格，本句问的是这本书多少钱。"),
        new(QuestionTypeEnum.TrueFalse, "判断：「children」 是 「child」 的复数，属于规则变化。", "错",
            "child → children 改变了词尾并增加 -ren，是不规则复数变化。"),
        new(QuestionTypeEnum.ShortAnswer, "把句子翻译成英文：「他昨天没有去公园。」",
            "He didn't go to the park yesterday.|He did not go to the park yesterday.",
            "评分要点：用一般过去时的否定式 didn't go（7 分），时间状语 yesterday 与地点表达正确（7 分）。"),
        new(QuestionTypeEnum.Composition, "以 「My weekend plan」 为题写 5~7 句英文，说明你周末打算做什么。",
            "使用 be going to / will 等结构描述周末安排。",
            "评分要点：句式正确使用将来表达（6 分）、内容完整切题（4 分）、拼写与标点规范（4 分）。")
    ];
}
