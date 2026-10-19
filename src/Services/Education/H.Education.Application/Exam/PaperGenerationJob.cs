using H.Education.Application.Contracts;
using H.Education.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.MultiTenancy;

namespace H.Education.Application.Exam;

/// <summary>
/// AI 命题后台任务。
/// 整卷生成要跑几分钟（思考型模型更久），放在 HTTP 请求里必然撞上客户端超时，
/// 所以这里由 Hangfire 调度：进程重启后任务还在，卷子上先落一张"生成中"的空卷。
/// </summary>
public class PaperGenerationJob
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ICurrentTenant _currentTenant;

    public PaperGenerationJob(IServiceScopeFactory scopeFactory, ICurrentTenant currentTenant)
    {
        _scopeFactory = scopeFactory;
        _currentTenant = currentTenant;
    }

    public async Task ExecuteAsync(Guid paperId, Guid? tenantId = null)
    {
        // 后台任务没有请求上下文，按卷子所属租户切回去才查得到数据
        using var _ = _currentTenant.Change(tenantId);
        using var scope = _scopeFactory.CreateScope();

        var paperRepository = scope.ServiceProvider.GetRequiredService<IRepository<ExamPaperEntity, Guid>>();
        var questionRepository = scope.ServiceProvider.GetRequiredService<IRepository<ExamQuestionEntity, Guid>>();
        var generator = scope.ServiceProvider.GetRequiredService<AiPaperGenerator>();

        var paper = await paperRepository.FindAsync(paperId);
        if (paper is null || paper.GenerateState != (int)PaperGenerateStateEnum.Generating) return;

        var input = EduJson.Read<GeneratePaperInputDto>(paper.GenerateInput);
        if (input is null)
        {
            await MarkFailedAsync(paperRepository, paper, "找不到原始命题参数，无法继续生成");
            return;
        }

        try
        {
            var generated = await generator.GenerateAsync(input);

            // 重跑时先清空上一次的题目，避免半成品混进来
            await questionRepository.DeleteAsync(x => x.PaperId == paper.Id, autoSave: true);

            var sort = 1;
            var questions = generated.Questions.Select(question => new ExamQuestionEntity
            {
                PaperId = paper.Id,
                Section = question.Section,
                Type = (int)question.Type,
                Content = question.Content,
                OptionsJson = question.Options.Count == 0 ? null : EduJson.Write(question.Options),
                Answer = question.Answer,
                Analysis = question.Analysis,
                Score = question.Score,
                Sort = sort++
            }).ToList();

            await questionRepository.InsertManyAsync(questions, autoSave: true);

            paper.Title = generated.Title;
            paper.DurationMinutes = generated.DurationMinutes;
            paper.TotalScore = questions.Count == 0 ? 0 : questions.Sum(x => x.Score);
            paper.QuestionCount = questions.Count;
            paper.AiModel = generated.Model;
            paper.AiPrompt = generated.Prompt;
            paper.GenerateState = (int)PaperGenerateStateEnum.Completed;
            paper.GenerateError = null;

            await paperRepository.UpdateAsync(paper, autoSave: true);
        }
        catch (Exception ex)
        {
            await MarkFailedAsync(paperRepository, paper, ex.Message);
        }
    }

    private static async Task MarkFailedAsync(
        IRepository<ExamPaperEntity, Guid> paperRepository, ExamPaperEntity paper, string? error)
    {
        paper.GenerateState = (int)PaperGenerateStateEnum.Failed;
        paper.GenerateError = Truncate(error);
        await paperRepository.UpdateAsync(paper, autoSave: true);
    }

    private static string? Truncate(string? value)
    {
        if (string.IsNullOrWhiteSpace(value)) return "生成失败";
        var text = value.Trim();
        return text.Length <= 900 ? text : text[..900] + "…";
    }
}
