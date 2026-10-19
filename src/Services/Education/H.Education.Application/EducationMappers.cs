using System.Text.Json;
using H.Education.Application.Contracts;
using H.Education.EntityFrameworkCore;

namespace H.Education.Application;

/// <summary>
/// 教育应用内部统一的 JSON 读写（题目选项、作答快照等冗余字段）
/// </summary>
internal static class EduJson
{
    internal static readonly JsonSerializerOptions Options = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        PropertyNameCaseInsensitive = true
    };

    public static string? Write<T>(T? value) =>
        value is null ? null : JsonSerializer.Serialize(value, Options);

    public static T? Read<T>(string? json)
    {
        if (string.IsNullOrWhiteSpace(json)) return default;

        try
        {
            return JsonSerializer.Deserialize<T>(json, Options);
        }
        catch (JsonException)
        {
            return default;
        }
    }

    public static List<QuestionOptionDto> ReadOptions(string? json)
    {
        if (string.IsNullOrWhiteSpace(json)) return [];
        try
        {
            return JsonSerializer.Deserialize<List<QuestionOptionDto>>(json, Options) ?? [];
        }
        catch (JsonException)
        {
            return [];
        }
    }

    public static List<string> ReadStringList(string? json)
    {
        if (string.IsNullOrWhiteSpace(json)) return [];
        try
        {
            return JsonSerializer.Deserialize<List<string>>(json, Options) ?? [];
        }
        catch (JsonException)
        {
            return [];
        }
    }
}

/// <summary>
/// 实体与 DTO 转换
/// </summary>
internal static class EducationMappers
{
    public static ExamPaperDto ToDto(this ExamPaperEntity entity) => Fill(new ExamPaperDto(), entity);

    public static ExamPaperDetailDto ToDetailDto(this ExamPaperEntity entity) => Fill(new ExamPaperDetailDto(), entity);

    private static T Fill<T>(T dto, ExamPaperEntity entity) where T : ExamPaperDto, new()
    {
        dto.Id = entity.Id;
        dto.Title = entity.Title;
        dto.Subject = (EducationSubjectEnum)entity.Subject;
        dto.Grade = entity.Grade;
        dto.Term = entity.Term;
        dto.Kind = (PaperKindEnum)entity.Kind;
        dto.Unit = entity.Unit;
        dto.Textbook = entity.Textbook;
        dto.DurationMinutes = entity.DurationMinutes;
        dto.TotalScore = entity.TotalScore;
        dto.Status = (PaperStatusEnum)entity.Status;
        dto.Source = (PaperSourceEnum)entity.Source;
        dto.Description = entity.Description;
        dto.AiModel = entity.AiModel;
        dto.AiPrompt = entity.AiPrompt;
        dto.GenerateState = (PaperGenerateStateEnum)entity.GenerateState;
        dto.GenerateError = entity.GenerateError;
        dto.QuestionCount = entity.QuestionCount;
        dto.CreationTime = entity.CreationTime;
        return dto;
    }

    public static ExamQuestionDto ToDto(this ExamQuestionEntity entity) => new()
    {
        Id = entity.Id,
        Section = entity.Section,
        Type = (QuestionTypeEnum)entity.Type,
        Content = entity.Content,
        Options = EduJson.ReadOptions(entity.OptionsJson),
        Answer = entity.Answer,
        Analysis = entity.Analysis,
        Score = entity.Score,
        Sort = entity.Sort
    };

    public static StudentDto ToDto(this StudentEntity entity) => new()
    {
        Id = entity.Id,
        StudentNo = entity.StudentNo,
        Name = entity.Name,
        Gender = (GenderEnum)entity.Gender,
        Grade = entity.Grade,
        ClassName = entity.ClassName,
        GuardianName = entity.GuardianName,
        GuardianPhone = entity.GuardianPhone,
        Remark = entity.Remark,
        CreationTime = entity.CreationTime
    };

    public static AnswerSheetDto ToDto(this AnswerSheetEntity entity) => FillSheet(new AnswerSheetDto(), entity);

    public static AnswerResultDto ToResultDto(this AnswerSheetEntity entity) => FillSheet(new AnswerResultDto(), entity);

    private static T FillSheet<T>(T dto, AnswerSheetEntity entity) where T : AnswerSheetDto, new()
    {
        dto.Id = entity.Id;
        dto.PaperId = entity.PaperId;
        dto.PaperTitle = entity.PaperTitle;
        dto.StudentId = entity.StudentId;
        dto.StudentName = entity.StudentName;
        dto.Status = (AnswerSheetStatusEnum)entity.Status;
        dto.StartTime = entity.StartTime;
        dto.SubmitTime = entity.SubmitTime;
        dto.DurationMinutes = entity.DurationMinutes;
        dto.DurationSeconds = entity.DurationSeconds;
        dto.FullScore = entity.FullScore;
        dto.ObjectiveScore = entity.ObjectiveScore;
        dto.SubjectiveScore = entity.SubjectiveScore;
        dto.TotalScore = entity.TotalScore;
        dto.QuestionCount = entity.QuestionCount;
        dto.CorrectCount = entity.CorrectCount;
        dto.AccuracyRate = entity.AccuracyRate;
        dto.GradeNote = entity.GradeNote;
        return dto;
    }

    public static AnswerDetailDto ToDto(this AnswerDetailEntity entity) => new()
    {
        Id = entity.Id,
        QuestionId = entity.QuestionId,
        Sort = entity.Sort,
        QuestionType = (QuestionTypeEnum)entity.QuestionType,
        QuestionContent = entity.QuestionContent,
        Options = EduJson.ReadOptions(entity.OptionsJson),
        StudentAnswer = entity.StudentAnswer,
        CorrectAnswer = entity.CorrectAnswer,
        Analysis = entity.Analysis,
        Score = entity.Score,
        AwardedScore = entity.AwardedScore,
        IsCorrect = entity.IsCorrect,
        JudgeMode = (JudgeModeEnum)entity.JudgeMode,
        Feedback = entity.Feedback
    };

    public static PracticeRecordDto ToDto(this PracticeSessionEntity entity) => new()
    {
        Id = entity.Id,
        GameType = (PracticeGameTypeEnum)entity.GameType,
        Subject = (EducationSubjectEnum)entity.Subject,
        Grade = entity.Grade,
        Level = entity.Level,
        StudentName = entity.StudentName,
        TotalCount = entity.TotalCount,
        CorrectCount = entity.CorrectCount,
        Score = entity.Score,
        MaxCombo = entity.MaxCombo,
        DurationSeconds = entity.DurationSeconds,
        CreationTime = entity.CreationTime
    };

    public static TextbookDto ToDto(this TextbookEntity entity) => Fill(new TextbookDto(), entity);

    public static TextbookDetailDto ToDetailDto(this TextbookEntity entity) => Fill(new TextbookDetailDto(), entity);

    private static T Fill<T>(T dto, TextbookEntity entity) where T : TextbookDto, new()
    {
        dto.Id = entity.Id;
        dto.Name = entity.Name;
        dto.Stage = (SchoolStageEnum)entity.Stage;
        dto.Subject = (TextbookSubjectEnum)entity.Subject;
        dto.Version = entity.Version;
        dto.Grade = entity.Grade;
        dto.Volume = (TextbookVolumeEnum)entity.Volume;
        dto.Publisher = entity.Publisher;
        dto.UnitCount = entity.UnitCount;
        dto.UnitLabel = entity.UnitLabel;
        dto.BookUrl = entity.BookUrl;
        dto.CourseUrl = entity.CourseUrl;
        dto.Source = (TextbookSourceEnum)entity.Source;
        dto.Remark = entity.Remark;
        dto.CreationTime = entity.CreationTime;
        return dto;
    }

    public static TeachingResourceDto ToDto(this TeachingResourceEntity entity) => new()
    {
        Id = entity.Id,
        TextbookId = entity.TextbookId,
        Unit = entity.Unit,
        UnitSort = entity.UnitSort,
        Lesson = entity.Lesson,
        Type = (TeachingResourceTypeEnum)entity.Type,
        Title = entity.Title,
        Url = entity.Url,
        SourceName = entity.SourceName,
        Remark = entity.Remark
    };
}
