using H.Testing.Application.Contracts;
using H.Util.Base;
using Microsoft.Extensions.Logging;
using Microsoft.Playwright;
using System.Diagnostics;
using System.Text.RegularExpressions;
using Volo.Abp.Application.Services;

namespace H.Testing.Application;

/// <summary>
/// Playwright 录制服务
/// </summary>
public class PlaywrightRecorderAppService : ApplicationService, IPlaywrightRecorderAppService
{
    private readonly ILogger<PlaywrightRecorderAppService> _logger;
    private readonly ITestingSettingsAppService _testingSettingsService;
    private IPlaywright? _playwright;
    private IBrowser? _browser;
    private IBrowserContext? _context;
    private IPage? _page;
    private Process? _recorderProcess;
    private readonly List<string> _recordedActions = new();
    private bool _isRecording = false;

    public PlaywrightRecorderAppService(
        ILogger<PlaywrightRecorderAppService> logger,
        ITestingSettingsAppService testingSettingsService)
    {
        _logger = logger;
        _testingSettingsService = testingSettingsService;
    }

    /// <summary>
    /// 启动 Playwright 录制器
    /// </summary>
    /// <param name="startUrl">起始URL</param>
    /// <returns>录制会话ID</returns>
    public async Task<BaseOutput<StartRecordingResponse>> StartRecordingAsync(string startUrl = "")
    {
        try
        {
            _logger.LogInformation("Starting Playwright recorder...");

            // 创建临时文件来保存录制的代码
            var tempDir = Path.Combine(Path.GetTempPath(), "playwright-recordings");
            Directory.CreateDirectory(tempDir);
            var recordingFile = Path.Combine(tempDir, $"recording_{Guid.NewGuid()}.cs");

            // 构建 Playwright 录制命令
            var playwrightPath = GetPlaywrightPath();
            var arguments = "";

            // 检查是否使用node.exe
            if (playwrightPath.EndsWith("node.exe"))
            {
                // 使用node.exe运行Playwright CLI
                var baseDirectory = AppDomain.CurrentDomain.BaseDirectory;
                var cliPath = Path.Combine(baseDirectory, ".playwright", "package", "cli.js");
                arguments = $"\"{cliPath}\" codegen --target=csharp";
            }
            else
            {
                // 直接使用playwright命令
                arguments = "codegen --target=csharp";
            }

            // 添加优化的录制参数
            arguments += $" --output=\"{recordingFile}\"";
            arguments += " --viewport-size=1280,720";  // 设置固定的视口大小
            arguments += " --timeout=30000";           // 设置30秒超时
            arguments += " --ignore-https-errors";     // 忽略HTTPS错误

            // 使用"设置"中配置的浏览器地址（若已配置且有效），保证录制与执行使用同一浏览器
            var settings = (await _testingSettingsService.GetBrowserPathAsync()).Data;
            var browserPath = settings?.BrowserPath;
            if (!string.IsNullOrWhiteSpace(browserPath))
            {
                if (File.Exists(browserPath))
                {
                    arguments += $" --executable-path=\"{browserPath}\"";
                }
                else
                {
                    _logger.LogWarning($"配置的浏览器地址无效，录制器回退默认浏览器：{browserPath}");
                }
            }

            // 如果有起始URL，添加到参数中
            if (!string.IsNullOrEmpty(startUrl))
            {
                arguments += $" \"{startUrl}\"";
            }

            _logger.LogInformation($"Starting Playwright with command: {playwrightPath} {arguments}");

            // 启动录制进程
            var processStartInfo = new ProcessStartInfo
            {
                FileName = playwrightPath,
                Arguments = arguments,
                UseShellExecute = false,
                CreateNoWindow = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                WorkingDirectory = tempDir
            };

            _recorderProcess = Process.Start(processStartInfo);

            if (_recorderProcess == null)
            {
                throw new InvalidOperationException("Failed to start Playwright process");
            }

            _isRecording = true;

            _logger.LogInformation($"Playwright recorder started with PID: {_recorderProcess.Id}");
            _logger.LogInformation($"Recording file will be saved to: {recordingFile}");
            _logger.LogInformation($"Working directory: {tempDir}");
            _logger.LogInformation($"Full command: {playwrightPath} {arguments}");

            // 启动异步读取输出和错误流
            _ = Task.Run(async () =>
            {
                try
                {
                    while (!_recorderProcess.StandardOutput.EndOfStream)
                    {
                        var line = await _recorderProcess.StandardOutput.ReadLineAsync();
                        if (!string.IsNullOrEmpty(line))
                        {
                            _logger.LogInformation($"Playwright Output: {line}");
                        }
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogWarning($"Error reading Playwright output: {ex.Message}");
                }
            });

            _ = Task.Run(async () =>
            {
                try
                {
                    while (!_recorderProcess.StandardError.EndOfStream)
                    {
                        var line = await _recorderProcess.StandardError.ReadLineAsync();
                        if (!string.IsNullOrEmpty(line))
                        {
                            _logger.LogWarning($"Playwright Error: {line}");
                        }
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogWarning($"Error reading Playwright error stream: {ex.Message}");
                }
            });

            // 等待一下让进程启动
            await Task.Delay(3000);

            // 检查进程是否还在运行
            if (_recorderProcess.HasExited)
            {
                _logger.LogError($"Playwright process exited immediately with code: {_recorderProcess.ExitCode}");
                throw new InvalidOperationException($"Playwright process exited with code: {_recorderProcess.ExitCode}");
            }

            // 返回录制文件路径作为会话ID
            return new(new StartRecordingResponse
            {
                SessionId = recordingFile,
                IsSuccess = true,
                Message = "Playwright recorder started successfully"
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to start Playwright recorder");
            throw;
        }
    }

    /// <summary>
    /// 停止录制
    /// </summary>
    /// <param name="sessionId">录制会话ID（录制文件路径）</param>
    /// <returns>录制的代码内容</returns>
    public async Task<BaseOutput<StopRecordingResponse>> StopRecordingAsync(string sessionId)
    {
        try
        {
            _logger.LogInformation("Stopping Playwright recorder...");

            _isRecording = false;

            // 停止录制进程
            if (_recorderProcess != null && !_recorderProcess.HasExited)
            {
                _logger.LogInformation($"Terminating Playwright process with PID: {_recorderProcess.Id}");

                // 尝试优雅关闭
                _recorderProcess.CloseMainWindow();

                // 等待一段时间让进程自然退出
                if (!_recorderProcess.WaitForExit(5000))
                {
                    _logger.LogInformation("Process did not exit gracefully, forcing termination");
                    _recorderProcess.Kill();
                }

                _recorderProcess.Dispose();
                _recorderProcess = null;
            }

            // 等待文件写入完成，并检查文件是否存在
            _logger.LogInformation($"Waiting for recording file: {sessionId}");

            var maxWaitTime = TimeSpan.FromSeconds(10);
            var startTime = DateTime.Now;

            while (DateTime.Now - startTime < maxWaitTime)
            {
                if (File.Exists(sessionId))
                {
                    // 文件存在，再等待一下确保写入完成
                    await Task.Delay(1000);
                    break;
                }
                await Task.Delay(500);
            }

            // 读取录制文件内容
            if (File.Exists(sessionId))
            {
                var recordedCode = await File.ReadAllTextAsync(sessionId);
                _logger.LogInformation($"Recording stopped. Code length: {recordedCode.Length}");
                _logger.LogInformation($"Recording file content preview: {recordedCode.Substring(0, Math.Min(200, recordedCode.Length))}...");

                // 清理临时文件
                try
                {
                    File.Delete(sessionId);
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Failed to delete temporary recording file");
                }

                return new(new StopRecordingResponse
                {
                    RecordedCode = recordedCode,
                    IsSuccess = true,
                    Message = "Playwright recorder stopped successfully"
                });
            }
            else
            {
                _logger.LogWarning($"Recording file not found: {sessionId}");
                return new(new StopRecordingResponse
                {
                    RecordedCode = string.Empty,
                    IsSuccess = false,
                    Message = "Playwright recorder stopped but no recording found"
                });
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to stop Playwright recorder");
            throw;
        }
    }

    /// <summary>
    /// 解析录制的代码并转换为测试步骤
    /// </summary>
    /// <param name="recordedCode">录制的代码</param>
    /// <returns>测试步骤列表</returns>
    public List<CaseStepDto> ParseRecordedCode(string recordedCode)
    {
        var steps = new List<CaseStepDto>();

        if (string.IsNullOrEmpty(recordedCode))
            return steps;

        try
        {
            var lines = recordedCode.Split('\n', StringSplitOptions.RemoveEmptyEntries);
            var stepOrder = 1;

            foreach (var line in lines)
            {
                var trimmedLine = line.Trim();
                if (string.IsNullOrEmpty(trimmedLine) || trimmedLine.StartsWith("//") || trimmedLine.StartsWith("using"))
                    continue;

                var step = ParseCodeLineToStep(trimmedLine, stepOrder);
                if (step != null)
                {
                    steps.Add(step);
                    stepOrder++;
                }
            }

            _logger.LogInformation($"Parsed {steps.Count} steps from recorded code");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to parse recorded code");
        }

        return steps;
    }

    /// <summary>
    /// 解析单行代码为测试步骤
    /// </summary>
    private CaseStepDto? ParseCodeLineToStep(string codeLine, int order)
    {
        try
        {
            // 页面导航
            if (codeLine.Contains(".GotoAsync("))
            {
                var urlMatch = Regex.Match(codeLine, @"\.GotoAsync\(""([^""]+)""\)");
                if (urlMatch.Success)
                {
                    return new CaseStepDto
                    {
                        Id = Guid.NewGuid().ToString(),
                        Name = $"导航到页面",
                        Type = StepTypeEnum.Ui,
                        Order = order,
                        IsEnabled = true,
                        ExpectedResult = "成功导航到指定页面",
                        UiConfig = new UiStepConfig
                        {
                            Action = "navigate",
                            Value = urlMatch.Groups[1].Value,
                            TimeoutMs = 30000
                        }
                    };
                }
            }

            // 点击操作
            if (codeLine.Contains(".ClickAsync("))
            {
                var selectorMatch = Regex.Match(codeLine, @"\.ClickAsync\(""([^""]+)""\)");
                if (selectorMatch.Success)
                {
                    var selector = selectorMatch.Groups[1].Value;
                    return new CaseStepDto
                    {
                        Id = Guid.NewGuid().ToString(),
                        Name = $"点击元素",
                        Type = StepTypeEnum.Ui,
                        Order = order,
                        IsEnabled = true,
                        ExpectedResult = "成功点击指定元素",
                        UiConfig = new UiStepConfig
                        {
                            Action = "click",
                            Selector = selector,
                            SelectorType = GetSelectorType(selector),
                            TimeoutMs = 30000
                        }
                    };
                }
            }

            // 输入文本
            if (codeLine.Contains(".FillAsync(") || codeLine.Contains(".TypeAsync("))
            {
                var fillMatch = Regex.Match(codeLine, @"\.(?:FillAsync|TypeAsync)\(""([^""]+)"",\s*""([^""]*)""\)");
                if (fillMatch.Success)
                {
                    var selector = fillMatch.Groups[1].Value;
                    var value = fillMatch.Groups[2].Value;
                    return new CaseStepDto
                    {
                        Id = Guid.NewGuid().ToString(),
                        Name = $"输入文本",
                        Type = StepTypeEnum.Ui,
                        Order = order,
                        IsEnabled = true,
                        ExpectedResult = "成功输入文本",
                        UiConfig = new UiStepConfig
                        {
                            Action = "type",
                            Selector = selector,
                            SelectorType = GetSelectorType(selector),
                            Value = value,
                            TimeoutMs = 30000
                        }
                    };
                }
            }

            // 选择操作
            if (codeLine.Contains(".SelectOptionAsync("))
            {
                var selectMatch = Regex.Match(codeLine, @"\.SelectOptionAsync\(""([^""]+)"",\s*""([^""]*)""\)");
                if (selectMatch.Success)
                {
                    var selector = selectMatch.Groups[1].Value;
                    var value = selectMatch.Groups[2].Value;
                    return new CaseStepDto
                    {
                        Id = Guid.NewGuid().ToString(),
                        Name = $"选择选项",
                        Type = StepTypeEnum.Ui,
                        Order = order,
                        IsEnabled = true,
                        ExpectedResult = "成功选择指定选项",
                        UiConfig = new UiStepConfig
                        {
                            Action = "select",
                            Selector = selector,
                            SelectorType = GetSelectorType(selector),
                            Value = value,
                            TimeoutMs = 30000
                        }
                    };
                }
            }

            // 等待操作
            if (codeLine.Contains(".WaitForSelectorAsync("))
            {
                var waitMatch = Regex.Match(codeLine, @"\.WaitForSelectorAsync\(""([^""]+)""\)");
                if (waitMatch.Success)
                {
                    var selector = waitMatch.Groups[1].Value;
                    return new CaseStepDto
                    {
                        Id = Guid.NewGuid().ToString(),
                        Name = $"等待元素出现",
                        Type = StepTypeEnum.Ui,
                        Order = order,
                        IsEnabled = true,
                        ExpectedResult = "元素成功出现",
                        UiConfig = new UiStepConfig
                        {
                            Action = "wait",
                            Selector = selector,
                            SelectorType = GetSelectorType(selector),
                            TimeoutMs = 30000
                        }
                    };
                }
            }

            // 断言操作（基于常见的断言模式）
            if (codeLine.Contains(".TextContentAsync(") || codeLine.Contains(".InnerTextAsync("))
            {
                var textMatch = Regex.Match(codeLine, @"\.(?:TextContentAsync|InnerTextAsync)\(""([^""]+)""\)");
                if (textMatch.Success)
                {
                    var selector = textMatch.Groups[1].Value;
                    return new CaseStepDto
                    {
                        Id = Guid.NewGuid().ToString(),
                        Name = $"验证文本内容",
                        Type = StepTypeEnum.Ui,
                        Order = order,
                        IsEnabled = true,
                        ExpectedResult = "文本内容符合预期",
                        UiConfig = new UiStepConfig
                        {
                            Action = "assert",
                            Selector = selector,
                            SelectorType = GetSelectorType(selector),
                            Value = "",
                            TimeoutMs = 30000
                        }
                    };
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, $"Failed to parse code line: {codeLine}");
        }

        return null;
    }

    /// <summary>
    /// 根据选择器内容判断选择器类型
    /// </summary>
    private string GetSelectorType(string selector)
    {
        if (selector.StartsWith("#"))
            return "id";
        if (selector.StartsWith("."))
            return "css";
        if (selector.StartsWith("//") || selector.StartsWith("xpath="))
            return "xpath";
        if (selector.StartsWith("text="))
            return "text";
        if (selector.Contains("[") && selector.Contains("]"))
            return "css";

        return "css"; // 默认为CSS选择器
    }

    /// <summary>
    /// 获取 Playwright 可执行文件路径
    /// </summary>
    private string GetPlaywrightPath()
    {
        // 获取当前应用程序的基础目录
        var baseDirectory = AppDomain.CurrentDomain.BaseDirectory;

        // 构建 Playwright node.exe 路径
        var nodePath = Path.Combine(baseDirectory, ".playwright", "node", "win32_x64", "node.exe");

        if (File.Exists(nodePath))
        {
            _logger.LogInformation($"Found Playwright node.exe at: {nodePath}");
            return nodePath;
        }

        // 如果找不到，记录错误并尝试全局路径
        _logger.LogWarning($"Playwright node.exe not found at: {nodePath}");

        // 尝试全局安装路径
        var userProfile = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
        var playwrightPath = Path.Combine(userProfile, ".dotnet", "tools", "playwright.exe");

        if (File.Exists(playwrightPath))
        {
            _logger.LogInformation($"Found global Playwright at: {playwrightPath}");
            return playwrightPath;
        }

        // 最后尝试系统PATH中的playwright
        _logger.LogWarning("Using system PATH for playwright command");
        return "playwright";
    }

    /// <summary>
    /// 检查录制状态
    /// </summary>
    public bool IsRecording => _isRecording;

    /// <summary>
    /// 清理资源
    /// </summary>
    public async Task DisposeAsync()
    {
        if (_isRecording)
        {
            await StopRecordingAsync("");
        }

        _recorderProcess?.Dispose();

        if (_context != null)
        {
            await _context.DisposeAsync();
        }

        if (_browser != null)
        {
            await _browser.DisposeAsync();
        }

        _playwright?.Dispose();
    }
}