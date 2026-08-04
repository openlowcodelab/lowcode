using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using H.Assistant.Application.Contracts;
using H.AppLab.Desktop.Services;

namespace H.AppLab.Desktop.ViewModels;

/// <summary>
/// MCP 管理 ViewModel（只读列表，数据统一由 Assistant.Web 维护）
/// </summary>
public partial class McpSettingsViewModel : ObservableObject
{
    private readonly IMcpServerAppService _mcpServerAppService;
    private readonly ToastService _toast;

    public ObservableCollection<McpCardItem> Servers { get; } = [];

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(HasServers))]
    private bool loading;

    public bool HasServers => !Loading && Servers.Count > 0;

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
    public string Name => Dto.Name;
    public string Endpoint => Dto.Endpoint;
    public string TimeoutText => $"{Dto.TimeoutSeconds} 秒";
    public bool HasAuthToken => !string.IsNullOrEmpty(Dto.AuthToken);
    public string MaskedAuthToken => LlmCardItem.Mask(Dto.AuthToken ?? "");
    public bool HasApiKey => !string.IsNullOrEmpty(Dto.ApiKey);
    public string MaskedApiKey => LlmCardItem.Mask(Dto.ApiKey ?? "");
}
