using H.Abp.Application.Contracts;
using H.Education.Application.Contracts;
using H.Education.Application.Grading;
using H.Education.EntityFrameworkCore;
using H.Util.Base;
using Volo.Abp;
using Volo.Abp.Application.Services;
using Volo.Abp.Domain.Repositories;

namespace H.Education.Application.Services;

/// <summary>
/// 答题应用服务：开始答题、保存进度、交卷并自动评分。
/// 题目在创建答卷时按题快照到明细表，之后即使改卷也能保持当时看到的题目与分值。
/// </summary>
public class AnswerSheetAppService : ApplicationService, IAnswerSheetAppService
{
    private readonly IRepository<ExamPaperEntity, Guid> _paperRepository;
    private readonly IRepository<ExamQuestionEntity, Guid> _questionRepository;
    private readonly IRepository<AnswerSheetEntity, Guid> _sheetRepository;
    private readonly IRepository<AnswerDetailEntity, Guid> _detailRepository;
    private readonly IRepository<StudentEntity, Guid> _studentRepository;
    private readonly AnswerGrader _grader;

    public AnswerSheetAppService(
        IRepository<ExamPaperEntity, Guid> paperRepository,
        IRepository<ExamQuestionEntity, Guid> questionRepository,
        IRepository<AnswerSheetEntity, Guid> sheetRepository,
        IRepository<AnswerDetailEntity, Guid> detailRepository,
        IRepository<StudentEntity, Guid> studentRepository,
        AnswerGrader grader)
    {
        _paperRepository = paperRepository;
        _questionRepository = questionRepository;
        _sheetRepository = sheetRepository;
        _detailRepository = detailRepository;
        _studentRepository = studentRepository;
        _grader = grader;
    }

    public async Task<BaseOutput<PagedResultDto<AnswerSheetDto>>> GetListAsync(AnswerSheetQueryDto input)
    {
        var query = await _sheetRepository.GetQueryableAsync();

        if (!string.IsNullOrWhiteSpace(input.Filter))
        {
            query = query.Where(x => x.StudentName.Contains(input.Filter) || x.PaperTitle.Contains(input.Filter));
        }
        if (input.PaperId.HasValue)
        {
            query = query.Where(x => x.PaperId == input.PaperId.Value);
        }
        if (input.Status.HasValue)
        {
            var status = (int)input.Status.Value;
            query = query.Where(x => x.Status == status);
        }

        var totalCount = await AsyncExecuter.CountAsync(query);
        var maxResult = input.MaxResultCount > 0 ? input.MaxResultCount : 10;
        var items = await AsyncExecuter.ToListAsync(
            query.OrderByDescending(x => x.CreationTime).Skip(input.SkipCount).Take(maxResult));

        return new(new PagedResultDto<AnswerSheetDto>(totalCount, items.Select(x => x.ToDto()).ToList()));
    }

    public async Task<BaseOutput<AnswerResultDto>> GetResultAsync(Guid id)
    {
        var sheet = await _sheetRepository.GetAsync(id);
        var details = await _detailRepository.GetListAsync(x => x.SheetId == id);
        details.Sort((a, b) => a.Sort.CompareTo(b.Sort));

        return new(BuildResult(sheet, details));
    }

    public async Task<BaseOutput<AnswerStartDto>> StartAsync(StartAnswerInputDto input)
    {
        var paper = await _paperRepository.GetAsync(input.PaperId);
        var student = input.StudentId is Guid studentId ? await _studentRepository.FindAsync(studentId) : null;

        if ((PaperStatusEnum)paper.Status != PaperStatusEnum.Published)
        {
            throw new UserFriendlyException($"试卷《{paper.Title}》还是草稿，请先在「试卷管理」中发布");
        }

        var questions = await GetQuestionsAsync(paper.Id);
        if (questions.Count == 0)
        {
            throw new UserFriendlyException($"试卷《{paper.Title}》没有题目，无法答题");
        }

        var sheet = new AnswerSheetEntity
        {
            PaperId = paper.Id,
            PaperTitle = paper.Title,
            StudentId = student?.Id,
            StudentName = student?.Name ?? string.Empty,
            Status = (int)AnswerSheetStatusEnum.Answering,
            StartTime = DateTime.Now,
            DurationMinutes = paper.DurationMinutes,
            FullScore = paper.TotalScore,
            QuestionCount = questions.Count
        };
        sheet = await _sheetRepository.InsertAsync(sheet);

        var sort = 1;
        var details = questions.Select(question => new AnswerDetailEntity
        {
            SheetId = sheet.Id,
            QuestionId = question.Id,
            QuestionType = question.Type,
            QuestionContent = question.Content,
            OptionsJson = question.OptionsJson,
            CorrectAnswer = question.Answer,
            Analysis = question.Analysis,
            Score = question.Score,
            JudgeMode = (int)JudgeModeEnum.None,
            Sort = sort++
        }).ToList();

        await _detailRepository.InsertManyAsync(details);
        await CurrentUnitOfWork!.SaveChangesAsync();

        return new(BuildStart(sheet, questions, details));
    }

    public async Task<BaseOutput<AnswerStartDto>> GetAnsweringAsync(Guid id)
    {
        var sheet = await _sheetRepository.GetAsync(id);
        var questions = await GetQuestionsAsync(sheet.PaperId);
        var details = await _detailRepository.GetListAsync(x => x.SheetId == id);

        return new(BuildStart(sheet, questions, details));
    }

    public async Task<BaseOutput<AnswerSheetDto>> SaveAsync(Guid id, SaveAnswerInputDto input)
    {
        var sheet = await _sheetRepository.GetAsync(id);
        EnsureNotGraded(sheet);

        var answers = ToAnswerMap(input.Items);
        var details = await _detailRepository.GetListAsync(x => x.SheetId == id);
        foreach (var detail in details)
        {
            if (answers.TryGetValue(detail.QuestionId, out var answer))
            {
                detail.StudentAnswer = answer;
            }
        }

        sheet.Status = (int)AnswerSheetStatusEnum.Saved;
        await _detailRepository.UpdateManyAsync(details);
        await _sheetRepository.UpdateAsync(sheet);
        await CurrentUnitOfWork!.SaveChangesAsync();

        return new(sheet.ToDto());
    }

    public async Task<BaseOutput<AnswerResultDto>> SubmitAsync(Guid id, SubmitAnswerInputDto input)
    {
        var sheet = await _sheetRepository.GetAsync(id);

        // 重复交卷不再重新批改，直接返回已有成绩
        if (sheet.Status == (int)AnswerSheetStatusEnum.Graded)
        {
            var gradedDetails = await _detailRepository.GetListAsync(x => x.SheetId == id);
            gradedDetails.Sort((a, b) => a.Sort.CompareTo(b.Sort));
            return new(BuildResult(sheet, gradedDetails));
        }

        var answers = ToAnswerMap(input.Items);
        var questions = (await GetQuestionsAsync(sheet.PaperId)).ToDictionary(x => x.Id);
        var details = await _detailRepository.GetListAsync(x => x.SheetId == id);
        details.Sort((a, b) => a.Sort.CompareTo(b.Sort));

        decimal objectiveScore = 0;
        decimal subjectiveScore = 0;
        var correctCount = 0;
        var keywordJudged = 0;

        foreach (var detail in details)
        {
            var studentAnswer = answers.TryGetValue(detail.QuestionId, out var answer) ? answer : detail.StudentAnswer;
            detail.StudentAnswer = studentAnswer;

            // 题目被删除时用答卷里的快照补一份可判分对象，保证历史答卷仍能出分
            var question = questions.TryGetValue(detail.QuestionId, out var found)
                ? found
                : new ExamQuestionEntity
                {
                    PaperId = sheet.PaperId,
                    Type = detail.QuestionType,
                    Content = detail.QuestionContent,
                    Answer = detail.CorrectAnswer,
                    Analysis = detail.Analysis,
                    Score = detail.Score,
                    Sort = detail.Sort
                };

            var outcome = await _grader.GradeAsync(question, studentAnswer);
            detail.AwardedScore = outcome.AwardedScore;
            detail.IsCorrect = outcome.IsCorrect;
            detail.JudgeMode = (int)outcome.JudgeMode;
            detail.Feedback = outcome.Feedback;

            if (((QuestionTypeEnum)detail.QuestionType).IsObjective())
            {
                objectiveScore += outcome.AwardedScore;
            }
            else
            {
                subjectiveScore += outcome.AwardedScore;
            }

            if (outcome.IsCorrect == true) correctCount++;
            if (outcome.JudgeMode == JudgeModeEnum.Keyword) keywordJudged++;
        }

        var totalScore = objectiveScore + subjectiveScore;

        sheet.Status = (int)AnswerSheetStatusEnum.Graded;
        sheet.SubmitTime = DateTime.Now;
        sheet.DurationSeconds = input.DurationSeconds;
        sheet.ObjectiveScore = objectiveScore;
        sheet.SubjectiveScore = subjectiveScore;
        sheet.TotalScore = totalScore;
        sheet.CorrectCount = correctCount;
        sheet.AccuracyRate = sheet.FullScore > 0 ? Math.Round(totalScore / sheet.FullScore * 100, 1) : 0;
        sheet.GradeNote = keywordJudged > 0
            ? $"{keywordJudged} 道主观题由 AI 批改不可用，已按参考答案要点估算得分"
            : null;

        await _detailRepository.UpdateManyAsync(details);
        await _sheetRepository.UpdateAsync(sheet);
        await CurrentUnitOfWork!.SaveChangesAsync();

        return new(BuildResult(sheet, details));
    }

    public async Task<BaseOutput> DeleteAsync(Guid id)
    {
        var sheet = await _sheetRepository.GetAsync(id);

        await _detailRepository.DeleteAsync(x => x.SheetId == id);
        await _sheetRepository.DeleteAsync(sheet);

        return new();
    }

    private async Task<List<ExamQuestionEntity>> GetQuestionsAsync(Guid paperId)
    {
        var questions = await _questionRepository.GetListAsync(x => x.PaperId == paperId);
        questions.Sort((a, b) => a.Sort.CompareTo(b.Sort));
        return questions;
    }

    private void EnsureNotGraded(AnswerSheetEntity sheet)
    {
        if (sheet.Status == (int)AnswerSheetStatusEnum.Graded)
        {
            throw new UserFriendlyException("该答卷已交卷批改完成，不能再修改作答");
        }
    }

    private static Dictionary<Guid, string?> ToAnswerMap(List<AnswerInputItemDto> items)
    {
        var map = new Dictionary<Guid, string?>();
        foreach (var item in items)
        {
            map[item.QuestionId] = item.Answer;
        }
        return map;
    }

    private static AnswerStartDto BuildStart(
        AnswerSheetEntity sheet,
        List<ExamQuestionEntity> questions,
        List<AnswerDetailEntity> details)
    {
        var answers = details
            .GroupBy(x => x.QuestionId)
            .ToDictionary(g => g.Key, g => g.First().StudentAnswer);

        return new AnswerStartDto
        {
            Sheet = sheet.ToDto(),
            Questions = questions.Select(question => new AnswerQuestionDto
            {
                QuestionId = question.Id,
                Section = question.Section,
                Type = (QuestionTypeEnum)question.Type,
                Content = question.Content,
                Options = EduJson.ReadOptions(question.OptionsJson),
                Score = question.Score,
                Sort = question.Sort,
                StudentAnswer = answers.TryGetValue(question.Id, out var answer) ? answer : null
            }).ToList()
        };
    }

    private static AnswerResultDto BuildResult(AnswerSheetEntity sheet, List<AnswerDetailEntity> details)
    {
        var result = sheet.ToResultDto();
        result.Details = details.Select(x => x.ToDto()).ToList();
        return result;
    }
}
