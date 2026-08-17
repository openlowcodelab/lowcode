using CommunityToolkit.Mvvm.ComponentModel;
using H.AppLab.Desktop.Services;
using H.Assistant.Application.Contracts;
using System.Collections.ObjectModel;

namespace H.AppLab.Desktop.ViewModels;

/// <summary>
/// 模型管理 ViewModel（只读列表，数据统一由 Assistant.Web 维护）
/// </summary>
public partial class LlmSettingsViewModel : ObservableObject
{
    private readonly ILLMAppService _llmAppService;
    private readonly ToastService _toast;

    public ObservableCollection<LlmCardItem> Configs { get; } = [];

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(HasConfigs))]
    private bool loading;

    public bool HasConfigs => !Loading && Configs.Count > 0;

    public LlmSettingsViewModel(ILLMAppService llmAppService, ToastService toast)
    {
        _llmAppService = llmAppService;
        _toast = toast;
    }

    public async Task LoadAsync()
    {
        Loading = true;
        try
        {
            var configs = (await _llmAppService.GetAllAsync()).Data ?? [];
            Configs.Clear();
            foreach (var config in configs)
            {
                Configs.Add(new LlmCardItem(config));
            }
        }
        catch (Exception ex)
        {
            _toast.Show($"加载模型配置失败: {ex.Message}", "error");
        }
        finally
        {
            Loading = false;
            OnPropertyChanged(nameof(HasConfigs));
        }
    }
}

/// <summary>
/// 模型卡片项
/// </summary>
public class LlmCardItem(LLMDto dto)
{
    public LLMDto Dto { get; } = dto;

    public string ProviderDisplayName => Dto.ProviderDisplayName;
    public string ProviderName => Dto.ProviderName;
    public string Model => Dto.Model;
    public bool IsDefault => Dto.IsDefault;
    public bool IsEnabled => Dto.IsEnabled;
    public string MaskedApiKey => Mask(Dto.ApiKey);
    public string? BaseUrl => Dto.BaseUrl;
    public bool HasBaseUrl => !string.IsNullOrEmpty(Dto.BaseUrl);
    public string MaxTokensText => Dto.MaxTokens.ToString();
    public string TemperatureText => Dto.Temperature.ToString();

    internal static string Mask(string value)
    {
        if (string.IsNullOrEmpty(value)) return "(未设置)";
        if (value.Length <= 8) return "****";
        return value[..4] + "****" + value[^4..];
    }
}
