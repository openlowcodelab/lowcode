using H.Education.Application.Exam;
using H.Education.Application.Grading;
using H.Education.Application.Practice;
using H.Education.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Volo.Abp.Modularity;

namespace H.Education.Application;

[DependsOn(
    typeof(EducationEntityFrameworkCoreModule)
)]
public class EducationApplicationModule : AbpModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        // 命题与批改
        context.Services.AddScoped<AiPaperGenerator>();
        context.Services.AddScoped<AnswerGrader>();
        // AI 命题的 Hangfire 后台任务（宿主里 BackgroundTask 应用已配好 Hangfire 与服务器）
        context.Services.AddTransient<PaperGenerationJob>();

        // 趣味练习出题器：新增游戏只需实现 IPracticeItemGenerator 并在此注册
        context.Services.AddTransient<IPracticeItemGenerator, EnglishWordGenerator>();
        context.Services.AddTransient<IPracticeItemGenerator, ChinesePoemGenerator>();
        context.Services.AddTransient<IPracticeItemGenerator, FeihuaLingGenerator>();
        context.Services.AddTransient<IPracticeItemGenerator, EnglishListeningGenerator>();
        context.Services.AddTransient<IPracticeItemGenerator, EnglishDialogueGenerator>();
    }
}
