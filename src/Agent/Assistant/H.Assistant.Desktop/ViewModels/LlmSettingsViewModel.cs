using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using H.Assistant.Application.Contracts;
using H.Assistant.Desktop.Services;

namespace H.Assistant.Desktop.ViewModels;

/// <summary>
/// 模型管理 ViewModel（对应 Web 端 SettingsLLMs.razor）
/// </summary>
public partial class LlmSettingsViewModel : ObservableObject
{
    private readonly ILLMAppService _llmAppService;
    private readonly ToastService _toast;

    public record TypeDef(string Name, string DisplayName, List<string> Models);
    public record ProviderDef(string ProviderName, string DisplayName, string DefaultEndpoint, string ApiKeyUrl, List<TypeDef> Types);

    /// <summary>Provider 预设（与 Web 端一致）</summary>
    public List<ProviderDef> ProviderDefs { get; } =
    [
        new("bailian", "阿里云百炼", "https://dashscope.aliyuncs.com/compatible-mode/v1",
            "https://bailian.console.aliyun.com/cn-beijing/?tab=plan#/efm/subscription/token-plan",
            [
                new("pay-as-you-go", "按量付费", ["qwen3.7-max", "qwen3.6-plus", "qwen3.6-flash", "glm-5.1", "kimi-k2.6", "deepseek-v4-pro", "deepseek-v4-flash"]),
                new("token-plan", "TokenPlan", ["qwen3.7-max", "qwen3.6-plus", "glm-5.1", "kimi-k2.6", "deepseek-v4-pro", "deepseek-v4-flash"])
            ]),
        new("deepseek", "DeepSeek", "https://api.deepseek.com",
            "https://platform.deepseek.com/api_keys",
            [
                new("pay-as-you-go", "按量付费", ["deepseek-v4-pro", "deepseek-v4-flash"])
            ])
    ];

    public ObservableCollection<LlmCardItem> Configs { get; } = [];

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(HasConfigs))]
    private bool loading;

    public bool HasConfigs => !Loading && Configs.Count > 0;

    [ObservableProperty]
    private bool showDialog;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(DialogTitle))]
    [NotifyPropertyChangedFor(nameof(DialogSaveText))]
    private bool isEditing;

    public string DialogTitle => IsEditing ? "编辑模型" : "添加模型";
    public string DialogSaveText => IsEditing ? "保存" : "添加";

    [ObservableProperty]
    private bool saving;

    private Guid? _editingId;
    private string _apiSecret = string.Empty;
    private decimal _maxTokens = 2000;
    private decimal _temperature = 0.7m;
    private bool _isEnabled = true;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(CurrentProviderTypes))]
    [NotifyPropertyChangedFor(nameof(CurrentApiKeyUrl))]
    private ProviderDef? selectedProvider;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(CurrentModels))]
    [NotifyPropertyChangedFor(nameof(HasModelSuggestions))]
    private TypeDef? selectedType;

    [ObservableProperty]
    private string editModel = string.Empty;

    [ObservableProperty]
    private string editBaseUrl = string.Empty;

    [ObservableProperty]
    private string editApiKey = string.Empty;

    public List<TypeDef> CurrentProviderTypes => SelectedProvider?.Types ?? [];

    public List<string> CurrentModels => SelectedType?.Models ?? [];

    public bool HasModelSuggestions => CurrentModels.Count > 0;

    public string? CurrentApiKeyUrl => SelectedProvider?.ApiKeyUrl;

    public LlmSettingsViewModel(ILLMAppService llmAppService, ToastService toast)
    {
        _llmAppService = llmAppService;
        _toast = toast;
    }

    partial void OnSelectedProviderChanged(ProviderDef? value)
    {
        if (value != null && !IsEditing)
        {
            EditBaseUrl = value.DefaultEndpoint;
            SelectedType = value.Types.FirstOrDefault();
            EditModel = string.Empty;
        }
        else if (value != null)
        {
            SelectedType = value.Types.FirstOrDefault();
        }
        OnPropertyChanged(nameof(CurrentModels));
        OnPropertyChanged(nameof(HasModelSuggestions));
    }

    public async Task LoadAsync()
    {
        Loading = true;
        try
        {
            var configs = await _llmAppService.GetAllAsync();
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

    [RelayCommand]
    private void ShowAdd()
    {
        IsEditing = false;
        _editingId = null;
        _apiSecret = string.Empty;
        _maxTokens = 2000;
        _temperature = 0.7m;
        _isEnabled = true;
        EditApiKey = string.Empty;
        EditModel = string.Empty;
        SelectedProvider = ProviderDefs[0];
        SelectedType = ProviderDefs[0].Types[0];
        EditBaseUrl = ProviderDefs[0].DefaultEndpoint;
        ShowDialog = true;
    }

    [RelayCommand]
    private void Edit(LlmCardItem item)
    {
        var config = item.Dto;
        IsEditing = true;
        _editingId = config.Id;
        _apiSecret = config.ApiSecret ?? string.Empty;
        _maxTokens = config.MaxTokens;
        _temperature = (decimal)config.Temperature;
        _isEnabled = config.IsEnabled;
        SelectedProvider = ProviderDefs.FirstOrDefault(p => p.ProviderName == config.ProviderName) ?? ProviderDefs[0];
        SelectedType = SelectedProvider.Types.FirstOrDefault();
        EditModel = config.Model;
        EditBaseUrl = config.BaseUrl ?? string.Empty;
        EditApiKey = config.ApiKey;
        ShowDialog = true;
    }

    [RelayCommand]
    private void CloseDialog()
    {
        ShowDialog = false;
    }

    [RelayCommand]
    private void SelectSuggestedModel(string model)
    {
        EditModel = model;
    }

    [RelayCommand]
    private void OpenApiKeyUrl()
    {
        if (!string.IsNullOrEmpty(CurrentApiKeyUrl))
        {
            try
            {
                System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo(CurrentApiKeyUrl) { UseShellExecute = true });
            }
            catch
            {
                // 打开浏览器失败忽略
            }
        }
    }

    [RelayCommand]
    private async Task SaveAsync()
    {
        if (SelectedProvider == null) { _toast.Show("请选择提供商", "warning"); return; }
        if (string.IsNullOrWhiteSpace(EditModel)) { _toast.Show("请输入模型名称", "warning"); return; }
        if (string.IsNullOrWhiteSpace(EditApiKey)) { _toast.Show("请输入 API 密钥", "warning"); return; }

        Saving = true;
        try
        {
            if (IsEditing && _editingId.HasValue)
            {
                await _llmAppService.UpdateAsync(_editingId.Value, new UpdateLLMDto
                {
                    ProviderDisplayName = SelectedProvider.DisplayName,
                    ApiKey = EditApiKey,
                    ApiSecret = string.IsNullOrEmpty(_apiSecret) ? null : _apiSecret,
                    BaseUrl = string.IsNullOrEmpty(EditBaseUrl) ? null : EditBaseUrl,
                    Model = EditModel,
                    IsEnabled = _isEnabled,
                    MaxTokens = (int)_maxTokens,
                    Temperature = (float)_temperature
                });
                _toast.Show("模型配置已更新", "success");
            }
            else
            {
                await _llmAppService.CreateAsync(new CreateLLMDto
                {
                    ProviderName = SelectedProvider.ProviderName.Trim().ToLower(),
                    ProviderDisplayName = SelectedProvider.DisplayName,
                    ApiKey = EditApiKey,
                    ApiSecret = string.IsNullOrEmpty(_apiSecret) ? null : _apiSecret,
                    BaseUrl = string.IsNullOrEmpty(EditBaseUrl) ? null : EditBaseUrl,
                    Model = EditModel,
                    IsEnabled = _isEnabled,
                    MaxTokens = (int)_maxTokens,
                    Temperature = (float)_temperature
                });
                _toast.Show("模型配置已添加", "success");
            }
            ShowDialog = false;
            await LoadAsync();
        }
        catch (Exception ex)
        {
            _toast.Show($"保存失败: {ex.Message}", "error");
        }
        finally
        {
            Saving = false;
        }
    }

    [RelayCommand]
    private async Task DeleteAsync(LlmCardItem item)
    {
        try
        {
            await _llmAppService.DeleteAsync(item.Dto.Id);
            _toast.Show("已删除", "success");
            await LoadAsync();
        }
        catch (Exception ex)
        {
            _toast.Show($"删除失败: {ex.Message}", "error");
        }
    }

    [RelayCommand]
    private async Task SetDefaultAsync(LlmCardItem item)
    {
        try
        {
            await _llmAppService.SetDefaultAsync(item.Dto.ProviderName);
            _toast.Show($"{item.Dto.ProviderName} 已设为默认", "success");
            await LoadAsync();
        }
        catch (Exception ex)
        {
            _toast.Show($"设置失败: {ex.Message}", "error");
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
    public bool IsNotDefault => !Dto.IsDefault;
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
