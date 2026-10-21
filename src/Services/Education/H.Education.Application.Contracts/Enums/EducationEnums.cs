namespace H.Education.Application.Contracts;

/// <summary>学科</summary>
public enum EducationSubjectEnum
{
    /// <summary>语文</summary>
    Chinese = 1,
    /// <summary>数学</summary>
    Math = 2,
    /// <summary>英语</summary>
    English = 3
}

/// <summary>题型</summary>
public enum QuestionTypeEnum
{
    /// <summary>单选题</summary>
    SingleChoice = 1,
    /// <summary>多选题</summary>
    MultipleChoice = 2,
    /// <summary>判断题</summary>
    TrueFalse = 3,
    /// <summary>填空题</summary>
    FillBlank = 4,
    /// <summary>简答题</summary>
    ShortAnswer = 5,
    /// <summary>计算题</summary>
    Calculation = 6,
    /// <summary>作文/书面表达</summary>
    Composition = 7
}

/// <summary>试卷状态</summary>
public enum PaperStatusEnum
{
    /// <summary>草稿，不可答题</summary>
    Draft = 0,
    /// <summary>已发布，可答题</summary>
    Published = 1
}

/// <summary>试卷来源</summary>
public enum PaperSourceEnum
{
    /// <summary>AI 生成</summary>
    Ai = 0,
    /// <summary>内置样卷</summary>
    BuiltIn = 1
}

/// <summary>试卷类型</summary>
public enum PaperKindEnum
{
    /// <summary>普通练习卷：题型与分值由知识点范围自由决定</summary>
    Normal = 0,
    /// <summary>单元测试卷：按 53 天天练的单元卷体例组卷（分大题、基础/提高/培优分层）</summary>
    UnitTest = 1
}

/// <summary>试卷命题状态</summary>
public enum PaperGenerateStateEnum
{
    /// <summary>生成中（AI 命题排队/执行中，题目尚未写入）</summary>
    Generating = 1,
    /// <summary>已完成，题目可用</summary>
    Completed = 2,
    /// <summary>生成失败，可查看原因或重试</summary>
    Failed = 3
}

/// <summary>答卷状态</summary>
public enum AnswerSheetStatusEnum
{
    /// <summary>答题中</summary>
    Answering = 0,
    /// <summary>已保存未交卷</summary>
    Saved = 1,
    /// <summary>已批改</summary>
    Graded = 2
}

/// <summary>逐题判定方式</summary>
public enum JudgeModeEnum
{
    /// <summary>未判定</summary>
    None = 0,
    /// <summary>客观题自动判定</summary>
    Auto = 1,
    /// <summary>主观题 AI 判定</summary>
    Ai = 2,
    /// <summary>主观题关键词与篇幅兜底判定</summary>
    Keyword = 3
}

/// <summary>趣味练习游戏。1（口算闯关）、6（修改病句）、7（单位换算）已下架，空号保留不重排，免得历史练习记录串位</summary>
public enum PracticeGameTypeEnum
{
    /// <summary>英语单词闯关</summary>
    EnglishWord = 2,
    /// <summary>语文诗词填空</summary>
    ChinesePoem = 3,
    /// <summary>英语听力练习</summary>
    EnglishListening = 4,
    /// <summary>英语对话练习</summary>
    EnglishDialogue = 5,
    /// <summary>语文趣味：飞花令</summary>
    FeihuaLing = 8
}

/// <summary>练习分类</summary>
public enum PracticeCategoryEnum
{
    /// <summary>趣味练习</summary>
    Fun = 1,
    /// <summary>专项练习</summary>
    Topic = 2
}

/// <summary>练习项作答方式</summary>
public enum PracticeInputTypeEnum
{
    /// <summary>选择</summary>
    Choice = 0,
    /// <summary>输入</summary>
    Typing = 1
}

/// <summary>难度</summary>
public enum DifficultyEnum
{
    /// <summary>基础</summary>
    Easy = 1,
    /// <summary>提高</summary>
    Normal = 2,
    /// <summary>挑战</summary>
    Hard = 3
}

/// <summary>性别</summary>
public enum GenderEnum
{
    /// <summary>未填写</summary>
    Unknown = 0,
    /// <summary>男</summary>
    Male = 1,
    /// <summary>女</summary>
    Female = 2
}

/// <summary>学段</summary>
public enum SchoolStageEnum
{
    /// <summary>小学</summary>
    Primary = 1,
    /// <summary>初中</summary>
    JuniorHigh = 2,
    /// <summary>高中</summary>
    SeniorHigh = 3
}

/// <summary>教材学科，取值覆盖中小学与高中的开设科目</summary>
public enum TextbookSubjectEnum
{
    /// <summary>语文</summary>
    Chinese = 1,
    /// <summary>数学</summary>
    Math = 2,
    /// <summary>英语</summary>
    English = 3,
    /// <summary>道德与法治</summary>
    EthicsAndRuleOfLaw = 4,
    /// <summary>物理</summary>
    Physics = 5,
    /// <summary>化学</summary>
    Chemistry = 6,
    /// <summary>生物学</summary>
    Biology = 7,
    /// <summary>历史</summary>
    History = 8
}

/// <summary>教材册次</summary>
public enum TextbookVolumeEnum
{
    /// <summary>上册</summary>
    First = 1,
    /// <summary>下册</summary>
    Second = 2,
    /// <summary>全一册</summary>
    Whole = 3
}

/// <summary>
/// 课时资源类型，与平台"课程教学"板块每一课挂载的五类资源一一对应
/// </summary>
public enum TeachingResourceTypeEnum
{
    /// <summary>视频课程</summary>
    VideoCourse = 1,
    /// <summary>课件</summary>
    Courseware = 2,
    /// <summary>教学设计</summary>
    InstructionDesign = 3,
    /// <summary>学习任务单</summary>
    LearningTaskSheet = 4,
    /// <summary>课后练习</summary>
    AfterClassExercise = 5
}

/// <summary>教材条目来源</summary>
public enum TextbookSourceEnum
{
    /// <summary>内置公开目录导入</summary>
    BuiltIn = 0,
    /// <summary>手工录入</summary>
    Manual = 1
}
