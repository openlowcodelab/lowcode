using H.Abp.Application.Contracts;
using H.Education.Application.Contracts;
using H.Education.Application.Exam;
using H.Education.EntityFrameworkCore;
using H.Util.Base;
using Hangfire;
using Volo.Abp;
using Volo.Abp.Application.Services;
using Volo.Abp.Domain.Repositories;

namespace H.Education.Application.Services;

/// <summary>
/// 试卷应用服务：AI 命题、内置样卷、试卷维护与统计
/// </summary>
public class ExamPaperAppService : ApplicationService, IExamPaperAppService
{
    private readonly IRepository<ExamPaperEntity, Guid> _paperRepository;
    private readonly IRepository<ExamQuestionEntity, Guid> _questionRepository;
    private readonly IRepository<AnswerSheetEntity, Guid> _sheetRepository;
    private readonly IBackgroundJobClient _backgroundJobClient;

    public ExamPaperAppService(
        IRepository<ExamPaperEntity, Guid> paperRepository,
        IRepository<ExamQuestionEntity, Guid> questionRepository,
        IRepository<AnswerSheetEntity, Guid> sheetRepository,
        IBackgroundJobClient backgroundJobClient)
    {
        _paperRepository = paperRepository;
        _questionRepository = questionRepository;
        _sheetRepository = sheetRepository;
        _backgroundJobClient = backgroundJobClient;
    }

    public async Task<BaseOutput<PagedResultDto<ExamPaperDto>>> GetListAsync(ExamPaperQueryDto input)
    {
        var query = await _paperRepository.GetQueryableAsync();

        if (!string.IsNullOrWhiteSpace(input.Filter))
        {
            query = query.Where(x => x.Title.Contains(input.Filter) || x.Grade.Contains(input.Filter));
        }
        if (input.Subject.HasValue)
        {
            var subject = (int)input.Subject.Value;
            query = query.Where(x => x.Subject == subject);
        }
        if (!string.IsNullOrWhiteSpace(input.Grade))
        {
            query = query.Where(x => x.Grade == input.Grade);
        }
        if (input.Status.HasValue)
        {
            var status = (int)input.Status.Value;
            query = query.Where(x => x.Status == status);
        }
        if (input.Kind.HasValue)
        {
            var kind = (int)input.Kind.Value;
            query = query.Where(x => x.Kind == kind);
        }

        var totalCount = await AsyncExecuter.CountAsync(query);
        var maxResult = input.MaxResultCount > 0 ? input.MaxResultCount : 10;
        var items = await AsyncExecuter.ToListAsync(
            query.OrderByDescending(x => x.CreationTime).Skip(input.SkipCount).Take(maxResult));

        return new(new PagedResultDto<ExamPaperDto>(totalCount, items.Select(x => x.ToDto()).ToList()));
    }

    public async Task<BaseOutput<List<ExamPaperStatDto>>> GetStatisticsAsync()
    {
        var paperRows = await _paperRepository.GetQueryableAsync();
        var sheetRows = await _sheetRepository.GetQueryableAsync();

        var papers = await AsyncExecuter.ToListAsync(
            paperRows.Select(x => new { x.Id, x.Subject }));
        var graded = await AsyncExecuter.ToListAsync(
            sheetRows
                .Where(x => x.Status == (int)AnswerSheetStatusEnum.Graded)
                .Select(x => new { x.PaperId, x.AccuracyRate }));

        var result = new List<ExamPaperStatDto>();
        foreach (var subject in new[] { (int)EducationSubjectEnum.Chinese, (int)EducationSubjectEnum.Math, (int)EducationSubjectEnum.English })
        {
            var paperIds = papers.Where(x => x.Subject == subject).Select(x => x.Id).ToHashSet();
            var sheets = graded.Where(x => paperIds.Contains(x.PaperId)).ToList();

            result.Add(new ExamPaperStatDto
            {
                Subject = (EducationSubjectEnum)subject,
                PaperCount = paperIds.Count,
                AnswerCount = sheets.Count,
                AvgAccuracyRate = sheets.Count == 0 ? 0 : Math.Round(sheets.Average(x => x.AccuracyRate), 1)
            });
        }

        // 只返回有内容的学科，避免全 0 的行占版面
        return new(result.Where(x => x.PaperCount > 0 || x.AnswerCount > 0).ToList());
    }

    public async Task<BaseOutput<ExamPaperDto>> GetAsync(Guid id)
    {
        var paper = await _paperRepository.GetAsync(id);
        return new(paper.ToDto());
    }

    public async Task<BaseOutput<ExamPaperDetailDto>> GetDetailAsync(Guid id)
    {
        var paper = await _paperRepository.GetAsync(id);
        var query = await _questionRepository.GetQueryableAsync();
        var questions = await AsyncExecuter.ToListAsync(
            query.Where(x => x.PaperId == id).OrderBy(x => x.Sort).ThenBy(x => x.CreationTime));

        return new(BuildDetail(paper, questions));
    }

    public async Task<BaseOutput<ExamPaperDetailDto>> GenerateAsync(GeneratePaperInputDto input)
    {
        if (string.IsNullOrWhiteSpace(input.Grade))
        {
            throw new UserFriendlyException("请填写年级，例如「四年级」");
        }

        if (input.Kind == PaperKindEnum.UnitTest)
        {
            if (string.IsNullOrWhiteSpace(input.Unit))
            {
                throw new UserFriendlyException("单元测试卷需要填单元，例如「第三单元」");
            }

            // 单元卷的大题结构与分值都依赖蓝图，内置样卷撑不起来，只能走 AI
            input.Source = PaperSourceEnum.Ai;
        }

        if (input.Source == PaperSourceEnum.Ai)
        {
            return await StartAiGenerateAsync(input);
        }

        var generated = BuiltInPaperTemplates.Build(input);

        var paper = new ExamPaperEntity
        {
            Title = generated.Title,
            Subject = (int)input.Subject,
            Grade = input.Grade.Trim(),
            Term = string.IsNullOrWhiteSpace(input.Term) ? null : input.Term.Trim(),
            Kind = (int)input.Kind,
            Unit = NormalizeOptional(input.Unit),
            Textbook = NormalizeOptional(input.Textbook),
            DurationMinutes = generated.DurationMinutes,
            TotalScore = generated.Questions.Sum(x => x.Score),
            Status = (int)PaperStatusEnum.Draft,
            Source = (int)input.Source,
            Description = BuildDescription(input),
            AiPrompt = generated.Prompt,
            AiModel = generated.Model,
            GenerateState = (int)PaperGenerateStateEnum.Completed,
            QuestionCount = generated.Questions.Count
        };

        paper = await _paperRepository.InsertAsync(paper);

        var questions = BuildQuestions(paper.Id, generated.Questions);
        await _questionRepository.InsertManyAsync(questions);
        await CurrentUnitOfWork!.SaveChangesAsync();

        return new(BuildDetail(paper, questions));
    }

    /// <summary>
    /// AI 命题：整卷生成要跑几分钟，同步等待会撞上浏览器 100 秒请求超时，
    /// 所以先落一张"生成中"的空卷交给 Hangfire，前端轮询列表看进度。
    /// </summary>
    private async Task<BaseOutput<ExamPaperDetailDto>> StartAiGenerateAsync(GeneratePaperInputDto input)
    {
        var title = input.Kind == PaperKindEnum.UnitTest
            ? $"{UnitPaperBlueprint.BuildScope(input)}{input.Subject.GetSubjectName()}单元测试卷"
            : $"{input.Grade.Trim()}{input.Subject.GetSubjectName()}" +
              (string.IsNullOrWhiteSpace(input.Term) ? string.Empty : input.Term!.Trim()) + "练习卷";

        var paper = new ExamPaperEntity
        {
            Title = title,
            Subject = (int)input.Subject,
            Grade = input.Grade.Trim(),
            Term = string.IsNullOrWhiteSpace(input.Term) ? null : input.Term.Trim(),
            Kind = (int)input.Kind,
            Unit = NormalizeOptional(input.Unit),
            Textbook = NormalizeOptional(input.Textbook),
            DurationMinutes = input.DurationMinutes,
            TotalScore = 0,
            Status = (int)PaperStatusEnum.Draft,
            Source = (int)PaperSourceEnum.Ai,
            Description = BuildDescription(input),
            GenerateInput = EduJson.Write(input),
            GenerateState = (int)PaperGenerateStateEnum.Generating,
            QuestionCount = 0
        };

        paper = await _paperRepository.InsertAsync(paper, autoSave: true);
        EnqueueGeneration(paper);

        return new(BuildDetail(paper, []));
    }

    public async Task<BaseOutput<ExamPaperDto>> RetryGenerateAsync(Guid id)
    {
        var paper = await _paperRepository.GetAsync(id);

        if ((PaperSourceEnum)paper.Source != PaperSourceEnum.Ai)
        {
            throw new UserFriendlyException("内置样卷是同步生成的，不需要重试");
        }
        if (paper.GenerateState == (int)PaperGenerateStateEnum.Generating)
        {
            return new(paper.ToDto());
        }
        if (string.IsNullOrWhiteSpace(paper.GenerateInput))
        {
            throw new UserFriendlyException("找不到原始命题参数，请重新生成一份");
        }

        paper.GenerateState = (int)PaperGenerateStateEnum.Generating;
        paper.GenerateError = null;
        paper.QuestionCount = 0;
        paper.TotalScore = 0;

        await _questionRepository.DeleteAsync(x => x.PaperId == id, autoSave: true);
        await _paperRepository.UpdateAsync(paper, autoSave: true);
        EnqueueGeneration(paper);

        return new(paper.ToDto());
    }

    private void EnqueueGeneration(ExamPaperEntity paper) =>
        _backgroundJobClient.Enqueue<PaperGenerationJob>(job => job.ExecuteAsync(paper.Id, paper.TenantId));

    public async Task<BaseOutput<ExamPaperDto>> UpdateAsync(Guid id, UpdateExamPaperDto input)
    {
        var paper = await _paperRepository.GetAsync(id);

        paper.Title = input.Title.Trim();
        paper.DurationMinutes = input.DurationMinutes > 0 ? input.DurationMinutes : paper.DurationMinutes;
        paper.Description = input.Description;
        paper.Status = (int)input.Status;

        await _paperRepository.UpdateAsync(paper);
        return new(paper.ToDto());
    }

    public async Task<BaseOutput<ExamPaperDto>> PublishAsync(Guid id, PublishPaperInputDto input)
    {
        var paper = await _paperRepository.GetAsync(id);

        if (input.Status == PaperStatusEnum.Published && paper.QuestionCount == 0)
        {
            throw new UserFriendlyException("试卷还没有题目，不能发布");
        }

        paper.Status = (int)input.Status;
        await _paperRepository.UpdateAsync(paper);
        return new(paper.ToDto());
    }

    public async Task<BaseOutput> DeleteAsync(Guid id)
    {
        var paper = await _paperRepository.GetAsync(id);

        // 题目与答卷由数据库级联删除，这里显式删除题目以便在未启用级联的环境里同样干净
        await _questionRepository.DeleteAsync(x => x.PaperId == id);
        await _sheetRepository.DeleteAsync(x => x.PaperId == id);
        await _paperRepository.DeleteAsync(paper);

        return new();
    }

    private static string BuildDescription(GeneratePaperInputDto input)
    {
        var unit = input.Kind == PaperKindEnum.UnitTest;
        var difficulty = input.Difficulty switch
        {
            DifficultyEnum.Easy => unit ? "基础达标" : "基础",
            DifficultyEnum.Hard => unit ? "尖子培优" : "挑战",
            _ => unit ? "能力提升" : "提高"
        };
        var points = string.IsNullOrWhiteSpace(input.KnowledgePoints)
            ? (unit ? "本单元全部考点" : "常见考点")
            : input.KnowledgePoints.Trim();
        return $"{(unit ? "单元卷" : input.Subject.GetSubjectName())} · {difficulty} · {points}";
    }

    private static string? NormalizeOptional(string? value) => string.IsNullOrWhiteSpace(value) ? null : value.Trim();

    private static List<ExamQuestionEntity> BuildQuestions(
        Guid paperId, List<GeneratedQuestion> generatedQuestions)
    {
        var sort = 1;
        return generatedQuestions.Select(question => new ExamQuestionEntity
        {
            PaperId = paperId,
            Section = question.Section,
            Type = (int)question.Type,
            Content = question.Content,
            OptionsJson = question.Options.Count == 0 ? null : EduJson.Write(question.Options),
            Answer = question.Answer,
            Analysis = question.Analysis,
            Score = question.Score,
            Sort = sort++
        }).ToList();
    }

    private static ExamPaperDetailDto BuildDetail(ExamPaperEntity paper, List<ExamQuestionEntity> questions)
    {
        var detail = paper.ToDetailDto();
        detail.Questions = questions.Select(x => x.ToDto()).ToList();
        return detail;
    }
}
