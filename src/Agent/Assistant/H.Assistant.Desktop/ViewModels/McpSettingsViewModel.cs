using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using H.Assistant.Application.Contracts;
using H.Assistant.Desktop.Services;

namespace H.Assistant.Desktop.ViewModels;

/// <summary>
/// MCP 管理 ViewModel（对应 Web 端 SettingsMcp.razor）
/// </summary>
public partial class McpSettingsViewModel : ObservableObject
{
    private readonly IMcpServerAppService _mcpServerAppService;
    private readonly ToastService _toast;

    public List<TransportOption> TransportOptions { get; } =
    [
        new("SSE", "SSE (Server-Sent Events)"),
        new("HTTP", "HTTP (Streamable HTTP)"),
        new("Stdio", "Stdio (标准输入输出)")
    ];

    public ObservableCollection<McpCardItem> Servers { get; } = [];

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(HasServers))]
    private bool loading;

    public bool HasServers => !Loading && Servers.Count > 0;

    [ObservableProperty]
    private bool showDialog;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(DialogTitle))]
    [NotifyPropertyChangedFor(nameof(DialogSaveText))]
    [NotifyPropertyChangedFor(nameof(ShowNameField))]
    private bool isEditing;

    public string DialogTitle => IsEditing ? "编辑 MCP 服务" : "添加 MCP 服务";
    public string DialogSaveText => IsEditing ? "保存" : "添加";
    public bool ShowNameField => !IsEditing;

    [ObservableProperty]
    private bool saving;

    private Guid? _editingId;

    [ObservableProperty]
    private string editName = string.Empty;

    [ObservableProperty]
    private string editDisplayName = string.Empty;

    [ObservableProperty]
    private string editEndpoint = string.Empty;

    [ObservableProperty]
    private TransportOption? editTransportType;

    [ObservableProperty]
    private string editAuthToken = string.Empty;

    [ObservableProperty]
    private string editApiKey = string.Empty;

    [ObservableProperty]
    private string editHeaders = string.Empty;

    [ObservableProperty]
    private decimal editTimeoutSeconds = 30;

    [ObservableProperty]
    private bool editIsEnabled = true;

    public McpSettingsViewModel(IMcpServerAppService mcpServerAppService, ToastService toast)
    {
        _mcpServerAppService = mcpServerAppService;
        _toast = toast;
    }

    public async Task LoadAsync()
    {
        Loading = true;
        try
        {
            var servers = await _mcpServerAppService.GetAllAsync();
            Servers.Clear();
            foreach (var server in servers)
            {
                Servers.Add(new McpCardItem(server));
            }
        }
        catch (Exception ex)
        {
            _toast.Show($"加载 MCP 服务列表失败: {ex.Message}", "error");
        }
        finally
        {
            Loading = false;
            OnPropertyChanged(nameof(HasServers));
        }
    }

    [RelayCommand]
    private void ShowAdd()
    {
        IsEditing = false;
        _editingId = null;
        EditName = string.Empty;
        EditDisplayName = string.Empty;
        EditEndpoint = string.Empty;
        EditTransportType = TransportOptions[0];
        EditAuthToken = string.Empty;
        EditApiKey = string.Empty;
        EditHeaders = string.Empty;
        EditTimeoutSeconds = 30;
        EditIsEnabled = true;
        ShowDialog = true;
    }

    [RelayCommand]
    private void Edit(McpCardItem item)
    {
        var server = item.Dto;
        IsEditing = true;
        _editingId = server.Id;
        EditName = server.Name;
        EditDisplayName = server.DisplayName;
        EditEndpoint = server.Endpoint;
        EditTransportType = TransportOptions.FirstOrDefault(t => t.Value == server.TransportType) ?? TransportOptions[0];
        EditAuthToken = server.AuthToken ?? string.Empty;
        EditApiKey = server.ApiKey ?? string.Empty;
        EditHeaders = server.Headers ?? string.Empty;
        EditTimeoutSeconds = server.TimeoutSeconds;
        EditIsEnabled = server.IsEnabled;
        ShowDialog = true;
    }

    [RelayCommand]
    private void CloseDialog()
    {
        ShowDialog = false;
    }

    [RelayCommand]
    private async Task SaveAsync()
    {
        if (string.IsNullOrWhiteSpace(EditName)) { _toast.Show("请输入名称", "warning"); return; }
        if (string.IsNullOrWhiteSpace(EditDisplayName)) { _toast.Show("请输入显示名称", "warning"); return; }
        if (string.IsNullOrWhiteSpace(EditEndpoint)) { _toast.Show("请输入端点 URL", "warning"); return; }

        Saving = true;
        try
        {
            if (IsEditing && _editingId.HasValue)
            {
                await _mcpServerAppService.UpdateAsync(_editingId.Value, new UpdateMcpServerDto
                {
                    DisplayName = EditDisplayName,
                    Endpoint = EditEndpoint,
                    TransportType = EditTransportType?.Value ?? "SSE",
                    AuthToken = string.IsNullOrWhiteSpace(EditAuthToken) ? null : EditAuthToken,
                    ApiKey = string.IsNullOrWhiteSpace(EditApiKey) ? null : EditApiKey,
                    Headers = string.IsNullOrWhiteSpace(EditHeaders) ? null : EditHeaders,
                    TimeoutSeconds = (int)EditTimeoutSeconds,
                    IsEnabled = EditIsEnabled
                });
                _toast.Show("MCP 服务已更新", "success");
            }
            else
            {
                await _mcpServerAppService.CreateAsync(new CreateMcpServerDto
                {
                    Name = EditName.Trim(),
                    DisplayName = EditDisplayName,
                    Endpoint = EditEndpoint,
                    TransportType = EditTransportType?.Value ?? "SSE",
                    AuthToken = string.IsNullOrWhiteSpace(EditAuthToken) ? null : EditAuthToken,
                    ApiKey = string.IsNullOrWhiteSpace(EditApiKey) ? null : EditApiKey,
                    Headers = string.IsNullOrWhiteSpace(EditHeaders) ? null : EditHeaders,
                    TimeoutSeconds = (int)EditTimeoutSeconds,
                    IsEnabled = EditIsEnabled
                });
                _toast.Show("MCP 服务已添加", "success");
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
    private async Task ToggleAsync(McpCardItem item)
    {
        try
        {
            await _mcpServerAppService.ToggleEnabledAsync(item.Dto.Id, !item.Dto.IsEnabled);
            _toast.Show(item.Dto.IsEnabled ? "MCP 服务已禁用" : "MCP 服务已启用", "success");
            await LoadAsync();
        }
        catch (Exception ex)
        {
            _toast.Show($"操作失败: {ex.Message}", "error");
        }
    }

    [RelayCommand]
    private async Task DeleteAsync(McpCardItem item)
    {
        try
        {
            await _mcpServerAppService.DeleteAsync(item.Dto.Id);
            _toast.Show("已删除", "success");
            await LoadAsync();
        }
        catch (Exception ex)
        {
            _toast.Show($"删除失败: {ex.Message}", "error");
        }
    }
}

public record TransportOption(string Value, string Label)
{
    public override string ToString() => Label;
}

/// <summary>
/// MCP 服务卡片项
/// </summary>
public class McpCardItem(McpServerDto dto)
{
    public McpServerDto Dto { get; } = dto;

    public string DisplayName => Dto.DisplayName;
    public string TransportType => Dto.TransportType;
    public bool IsEnabled => Dto.IsEnabled;
    public string ToggleText => Dto.IsEnabled ? "禁用" : "启用";
    public string Name => Dto.Name;
    public string Endpoint => Dto.Endpoint;
    public string TimeoutText => $"{Dto.TimeoutSeconds} 秒";
    public bool HasAuthToken => !string.IsNullOrEmpty(Dto.AuthToken);
    public string MaskedAuthToken => LlmCardItem.Mask(Dto.AuthToken ?? "");
    public bool HasApiKey => !string.IsNullOrEmpty(Dto.ApiKey);
    public string MaskedApiKey => LlmCardItem.Mask(Dto.ApiKey ?? "");
}
