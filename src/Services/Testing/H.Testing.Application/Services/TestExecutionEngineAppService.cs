using H.Testing.Application.Contracts;
using H.Util.Ids;
using Microsoft.Playwright;
using System.Diagnostics;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.RegularExpressions;
using Volo.Abp.Application.Services;

namespace H.Testing.Application;

/// <summary>
/// 测试执行引擎
/// </summary>
public class TestExecutionEngineAppService : ApplicationService, ITestExecutionEngineAppService
{    
    private readonly HttpClient _httpClient;
    private readonly IExecutionRecordAppService _executionRecordService;
    private readonly IEnvironmentAppService _environmentService;
    private readonly IProjectCaseAppService _projectCaseService;
    private readonly ITestExecutionEventNotifier _eventNotifier;
    private readonly Dictionary<string, string> _variables;
    
    public TestExecutionEngineAppService(
        HttpClient httpClient,
        IExecutionRecordAppService executionRecordService,
        IEnvironmentAppService environmentService,
        IProjectCaseAppService projectCaseService,
        ITestExecutionEventNotifier eventNotifier)
    {
        _httpClient = httpClient;
        _executionRecordService = executionRecordService;
        _environmentService = environmentService;
        _projectCaseService = projectCaseService;
        _eventNotifier = eventNotifier;
        _variables = new Dictionary<string, string>();
    }

    /// <summary>
    /// 获取截图存储目录
    /// </summary>
    private static string GetScreenshotDir(ProjectCaseDto testCase)
        => Path.Combine(AppContext.BaseDirectory, "testing-screenshots",
            testCase.ProjectId.ToString(), testCase.Id.ToString());
    
    /// <summary>
    /// 执行测试用例
    /// </summary>
    public async Task<ExecutionRecordDto> ExecuteTestCaseAsync(
        ProjectCaseDto testCase, 
        long environmentId, 
        CancellationToken cancellationToken = default)
    {
        var environment = await _environmentService.GetByIdAsync(testCase.ProjectId, environmentId);
        if (environment == null)
        {
            throw new ArgumentException($"Environment with ID {environmentId} not found");
        }

        // 处理模板继承逻辑
        if (testCase.TemplateId.HasValue)
        {
            var templateCase = await _projectCaseService.GetByIdAsync(testCase.TemplateId.Value);
            if (templateCase != null)
            {
                // 使用模板的步骤，但保持当前用例的变量/数据
                // 注意：这里我们临时替换 steps 用于执行，不修改原始对象
                var originalSteps = testCase.Steps;
                testCase.Steps = templateCase.Steps;
                
                // 如果当前用例没有步骤，或者我们决定完全使用模板步骤
                // 如果需要支持"部分覆盖"或"追加步骤"，逻辑会更复杂
                // 目前假设：如果有 TemplateId，则完全使用模板的步骤
            }
        }
        
        // 创建执行记录
        var executionRecord = new ExecutionRecordDto
        {
            TestCaseId = testCase.Id,
            TestCaseName = testCase.Name,
            ProjectId = testCase.ProjectId,
            EnvironmentId = environmentId,
            EnvironmentName = environment.Name,
            Status = ExecutionStatus.Running,
            TotalSteps = testCase.Steps.Count,
            EnvironmentSnapshot = environment.Config
        };
        
        await _executionRecordService.CreateAsync(executionRecord);
        
        // 初始化 Playwright（在整个测试用例执行期间保持同一个浏览器实例）
        IPlaywright playwright = null;
        IBrowser browser = null;
        IBrowserContext context = null;
        IPage page = null;
        
        try
        {
            // 初始化变量
            InitializeVariables(environment, testCase.TestData);

            // 初始化 Playwright（仅对包含 UI 步骤的测试用例）
            if (testCase.Steps.Any(s => IsUiStepType(s.Type)))
            {
                playwright = await Playwright.CreateAsync();
                browser = await playwright.Chromium.LaunchAsync(new BrowserTypeLaunchOptions
                {
                    Headless = false, // 设置为false可以看到浏览器操作
                    Args = new[] {
                        "--start-maximized", // 最大化浏览器窗口
                        "--activate", // 激活窗口（macOS）
                        "--foreground" // 前台运行
                    }
                });
                context = await browser.NewContextAsync(new BrowserNewContextOptions
                {
                    ViewportSize = null // 使用最大视口尺寸
                });
                page = await context.NewPageAsync();

                // 多重确保浏览器窗口处于前台
                await page.BringToFrontAsync(); // 将页面窗口带到前台
                await page.EvaluateAsync("() => { window.focus(); }");

                // 添加额外的焦点设置以确保窗口置顶
                await page.EvaluateAsync(@"() => {
                        window.focus();
                        window.addEventListener('blur', () => {
                            setTimeout(() => window.focus(), 100);
                        });
                    }");
            }

            var stopwatch = Stopwatch.StartNew();

            // 按顺序执行步骤
            foreach (var step in testCase.Steps.OrderBy(s => s.Order))
            {
                if (cancellationToken.IsCancellationRequested)
                {
                    executionRecord.Status = ExecutionStatus.Cancelled;
                    break;
                }
                
                var stepRecord = await ExecuteStepAsync(step, environment, page, cancellationToken, testCase);
                if (!executionRecord.StepRecords.Contains(stepRecord))
                {
                    executionRecord.StepRecords.Add(stepRecord);
                }
                
                // 更新统计
                switch (stepRecord.Status)
                {
                    case StepExecutionStatus.Success:
                        executionRecord.SuccessSteps++;
                        break;
                    case StepExecutionStatus.Failed:
                        executionRecord.FailedSteps++;
                        // 步骤失败时停止执行
                        executionRecord.Status = ExecutionStatus.Failed;
                        executionRecord.ErrorMessage = stepRecord.ErrorMessage;
                        goto ExecutionComplete;
                    case StepExecutionStatus.Skipped:
                        executionRecord.SkippedSteps++;
                        break;
                }
                
                // 通知执行记录更新
                _eventNotifier.RaiseExecutionUpdated(testCase.Id, executionRecord);
            }
            
            ExecutionComplete:
            stopwatch.Stop();
            
            // 如果没有失败且没有取消，则标记为成功
            if (executionRecord.Status == ExecutionStatus.Running)
            {
                executionRecord.Status = ExecutionStatus.Success;
            }
            
            executionRecord.EndTime = DateTime.Now;
            executionRecord.Duration = stopwatch.ElapsedMilliseconds;
        }
        catch (Exception ex)
        {
            executionRecord.Status = ExecutionStatus.Failed;
            executionRecord.ErrorMessage = ex.Message;
            executionRecord.EndTime = DateTime.Now;
        }
        finally
        {
            // 测试完成后保持浏览器打开状态，不关闭资源
            // 注释掉以下代码以保持浏览器窗口打开
            // if (context != null)
            //     await context.CloseAsync();
            // if (browser != null)
            //     await browser.CloseAsync();
            // if (playwright != null)
            //     playwright.Dispose();
            
            // 输出提示信息
            Console.WriteLine("UI自动化测试完成，浏览器窗口保持打开状态");
        }
        
        // 更新执行记录
        await _executionRecordService.UpdateAsync(executionRecord.ProjectId, executionRecord);
        
        // 更新测试用例的执行结果
        try
        {
            testCase.LastExecutionResult = executionRecord.Status;
            testCase.LastExecutionTime = executionRecord.EndTime ?? DateTime.Now;
            await _projectCaseService.UpdateAsync(testCase.Id, testCase);
        }
        catch (Exception ex)
        {
            // 记录更新失败的日志，但不影响主流程
            Console.WriteLine($"Failed to update test case execution result: {ex.Message}");
        }
        
        return executionRecord;
    }
    
    /// <summary>
    /// 执行单个步骤
    /// </summary>
    private async Task<StepExecutionRecord> ExecuteStepAsync(
        ProjectCaseStep step, 
        EnvironmentDto environment, 
        IPage page,
        CancellationToken cancellationToken,
        ProjectCaseDto testCase)
    {
        var stepRecord = new StepExecutionRecord
        {
            Id = Guid.NewGuid().ToString(),
            StepId = step.Id,
            StepName = step.Name,
            StepType = step.Type,
            Order = step.Order,
            Status = StepExecutionStatus.Running,
            StartTime = DateTime.Now
        };
        
        // 通知步骤开始执行
        _eventNotifier.RaiseStepUpdated(testCase.Id, stepRecord);
        
        var stopwatch = Stopwatch.StartNew();
        
        try
        {
            // 根据步骤类型执行相应的逻辑
            if (IsApiStepType(step.Type))
            {
                await ExecuteApiStepAsync(step, stepRecord, environment, cancellationToken);
            }
            else if (IsUiStepType(step.Type))
            {
                await ExecuteUiStepAsync(step, stepRecord, environment, page, cancellationToken, testCase);
            }
            else if (IsScriptStepType(step.Type))
            {
                await ExecuteScriptStepAsync(step, stepRecord, environment, cancellationToken);
            }
            else if (step.Type == StepType.Delay)
            {
                await ExecuteDelayStepAsync(step, stepRecord, cancellationToken);
            }
            else
            {
                throw new NotSupportedException($"Step type {step.Type} is not supported");
            }
            
            stepRecord.Status = StepExecutionStatus.Success;
        }
        catch (Exception ex)
        {
            stepRecord.Status = StepExecutionStatus.Failed;
            stepRecord.ErrorMessage = ex.Message;
            stepRecord.Logs.Add($"Error: {ex.Message}");
        }
        finally
        {
            stopwatch.Stop();
            stepRecord.EndTime = DateTime.Now;
            stepRecord.Duration = stopwatch.ElapsedMilliseconds;
            
            // 通知步骤执行完成
            _eventNotifier.RaiseStepUpdated(testCase.Id, stepRecord);
        }
        
        return stepRecord;
    }
    
    /// <summary>
    /// 执行API步骤
    /// </summary>
    private async Task ExecuteApiStepAsync(
        ProjectCaseStep step, 
        StepExecutionRecord stepRecord, 
        EnvironmentDto environment, 
        CancellationToken cancellationToken)
    {
        if (step.ApiConfig == null)
        {
            throw new InvalidOperationException("API configuration is required for API step");
        }
        
        var config = step.ApiConfig;
        
        // 构建请求URL
        var baseUrl = GetServiceEndpoint(environment, config.ServiceId);
        var url = baseUrl.TrimEnd('/') + '/' + config.Url.TrimStart('/');
        url = ReplaceVariables(url);
        
        // 添加查询参数
        if (config.Params.Any())
        {
            var queryParams = config.Params.Select(p => $"{p.Key}={Uri.EscapeDataString(ReplaceVariables(p.Value?.ToString() ?? ""))}");
            url += (url.Contains('?') ? "&" : "?") + string.Join("&", queryParams);
        }
        
        stepRecord.Logs.Add($"Request URL: {url}");
        
        // 创建HTTP请求
        var request = new HttpRequestMessage(new HttpMethod(config.Method), url);
        
        // 设置请求头
        foreach (var header in config.Headers)
        {
            var headerValue = ReplaceVariables(header.Value);
            request.Headers.TryAddWithoutValidation(header.Key, headerValue);
        }
        
        // 设置认证
        if (config.Auth?.Type == "Bearer" && !string.IsNullOrEmpty(config.Auth.Token))
        {
            var token = ReplaceVariables(config.Auth.Token);
            request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
            stepRecord.Logs.Add($"Authorization: Bearer {token.Substring(0, Math.Min(10, token.Length))}...");
        }
        
        // 设置请求体
        if (!string.IsNullOrEmpty(config.Body))
        {
            var body = ReplaceVariables(config.Body);
            stepRecord.RequestData = body;
            
            if (config.BodyType == "json")
            {
                request.Content = new StringContent(body, Encoding.UTF8, "application/json");
            }
            else
            {
                request.Content = new StringContent(body, Encoding.UTF8, "text/plain");
            }
        }
        
        // 记录完整的请求信息
        var requestInfo = new
        {
            Method = config.Method,
            Url = url,
            Headers = request.Headers.ToDictionary(h => h.Key, h => string.Join(", ", h.Value)),
            Body = stepRecord.RequestData,
            Auth = config.Auth?.Type == "Bearer" ? $"Bearer {ReplaceVariables(config.Auth.Token ?? "")}" : null
        };
        stepRecord.Logs.Add($"Complete Request Info: {System.Text.Json.JsonSerializer.Serialize(requestInfo, new System.Text.Json.JsonSerializerOptions { WriteIndented = true })}");
        
        // 发送请求
        stepRecord.Logs.Add($"Sending {config.Method} request...");
        
        using var response = await _httpClient.SendAsync(request, cancellationToken);
        var responseContent = await response.Content.ReadAsStringAsync(cancellationToken);
        
        stepRecord.ResponseData = responseContent;
        stepRecord.Logs.Add($"Response Status: {(int)response.StatusCode} {response.StatusCode}");
        stepRecord.Logs.Add($"Response Content: {responseContent}");
        
        // 执行断言
        if (config.Assertions.Any())
        {
            foreach (var assertion in config.Assertions)
            {
                var result = ExecuteAssertion(assertion, responseContent, (int)response.StatusCode);
                stepRecord.AssertionResults.Add(result);
                
                if (!result.Passed)
                {
                    throw new Exception($"Assertion failed: {result.ErrorMessage}");
                }
            }
        }
        
        // 提取变量
        if (config.VariableExtractions.Any())
        {
            foreach (var extraction in config.VariableExtractions)
            {
                var value = ExtractVariable(extraction.Value, responseContent);
                if (!string.IsNullOrEmpty(value))
                {
                    _variables[extraction.Key] = value;
                    stepRecord.ExtractedVariables[extraction.Key] = value;
                    stepRecord.Logs.Add($"Extracted variable {extraction.Key}: {value}");
                }
            }
        }
    }
    
    /// <summary>
    /// 执行UI步骤（使用Playwright实现）
    /// </summary>
    private async Task ExecuteUiStepAsync(
        ProjectCaseStep step, 
        StepExecutionRecord stepRecord, 
        EnvironmentDto environment, 
        IPage page,
        CancellationToken cancellationToken,
        ProjectCaseDto testCase)
    {
        if (step.UiConfig == null)
        {
            throw new InvalidOperationException("UI configuration is required for UI step");
        }

        var config = step.UiConfig;
        stepRecord.Logs.Add($"Executing UI step: {step.Name} (Action: {config.Action})");

        try
        {
            switch (config.Action.ToLower())
            {
                case "navigate":
                    // 导航到页面并等待网络空闲状态
                    await page.GotoAsync(config.Value, new PageGotoOptions { WaitUntil = WaitUntilState.NetworkIdle });
                    // 添加额外等待时间确保页面完全加载
                    await page.WaitForLoadStateAsync(LoadState.DOMContentLoaded);
                    await page.WaitForTimeoutAsync(2000); // 额外等待2秒确保页面元素完全渲染
                    stepRecord.Logs.Add($"Navigated to {config.Value} and waited for page to load");
                    break;
                    
                case "type":
                case "input":
                    try {
                        // 使用更可靠的方式等待元素可见
                        var locator = page.Locator(config.Selector);
                        stepRecord.Logs.Add($"等待元素 '{config.Selector}' 可见，超时时间: {config.TimeoutMs}ms");
                        
                        // 先尝试等待元素存在于DOM中
                        await locator.WaitForAsync(new LocatorWaitForOptions { 
                            State = WaitForSelectorState.Attached, 
                            Timeout = config.TimeoutMs 
                        });
                        
                        // 再等待元素可见
                        await locator.WaitForAsync(new LocatorWaitForOptions { 
                            State = WaitForSelectorState.Visible, 
                            Timeout = config.TimeoutMs 
                        });
                        
                        // 确保元素可交互
                        await page.WaitForTimeoutAsync(500); // 短暂等待确保元素完全可交互
                        
                        // 填充内容
                        await locator.FillAsync(config.Value);
                        stepRecord.Logs.Add($"成功填充 '{config.Value}' 到元素 '{config.Selector}'");
                    }
                    catch (Exception ex) {
                        // 记录详细错误信息
                        stepRecord.Logs.Add($"元素操作失败: {ex.Message}");
                        // 尝试截图记录当前页面状态
                        try {
                            var screenshotDir = GetScreenshotDir(testCase);
                            if (!Directory.Exists(screenshotDir))
                            {
                                Directory.CreateDirectory(screenshotDir);
                            }
                            var errorScreenshotPath = Path.Combine(screenshotDir, $"error_{step.Id}_{DateTime.Now:yyyyMMdd_HHmmss}.png");
                            await page.ScreenshotAsync(new PageScreenshotOptions { Path = errorScreenshotPath });
                            stepRecord.Logs.Add($"错误截图已保存: {errorScreenshotPath}");
                        }
                        catch (Exception screenshotEx) {
                            stepRecord.Logs.Add($"截图保存失败: {screenshotEx.Message}");
                        }
                        throw; // 重新抛出异常
                    }
                    break;

                case "click":
                    try {
                        // 使用更可靠的方式等待元素可点击
                        var locator = page.Locator(config.Selector);
                        stepRecord.Logs.Add($"等待元素 '{config.Selector}' 可点击，超时时间: {config.TimeoutMs}ms");
                        
                        // 先等待元素存在于DOM中
                        await locator.WaitForAsync(new LocatorWaitForOptions { 
                            State = WaitForSelectorState.Attached, 
                            Timeout = config.TimeoutMs 
                        });
                        
                        // 再等待元素可见
                        await locator.WaitForAsync(new LocatorWaitForOptions { 
                            State = WaitForSelectorState.Visible, 
                            Timeout = config.TimeoutMs 
                        });
                        
                        // 确保元素可交互
                        await page.WaitForTimeoutAsync(500); // 短暂等待确保元素完全可交互
                        
                        // 点击元素
                        await locator.ClickAsync(new LocatorClickOptions { Timeout = config.TimeoutMs });
                        stepRecord.Logs.Add($"成功点击元素 '{config.Selector}'");
                    }
                    catch (Exception ex) {
                        // 记录详细错误信息
                        stepRecord.Logs.Add($"元素点击失败: {ex.Message}");
                        // 尝试截图记录当前页面状态
                        try {
                            var screenshotDir = GetScreenshotDir(testCase);
                            if (!Directory.Exists(screenshotDir))
                            {
                                Directory.CreateDirectory(screenshotDir);
                            }
                            var errorScreenshotPath = Path.Combine(screenshotDir, $"error_{step.Id}_{DateTime.Now:yyyyMMdd_HHmmss}.png");
                            await page.ScreenshotAsync(new PageScreenshotOptions { Path = errorScreenshotPath });
                            stepRecord.Logs.Add($"错误截图已保存: {errorScreenshotPath}");
                        }
                        catch (Exception screenshotEx) {
                            stepRecord.Logs.Add($"截图保存失败: {screenshotEx.Message}");
                        }
                        throw; // 重新抛出异常
                    }
                    break;

                case "assert":
                    try {
                        // 使用更可靠的方式等待元素可见并断言
                        var locator = page.Locator(config.Selector);
                        stepRecord.Logs.Add($"等待元素 '{config.Selector}' 可见用于断言，超时时间: {config.TimeoutMs}ms");
                        
                        // 先等待页面加载完成
                        await page.WaitForLoadStateAsync(LoadState.NetworkIdle, new PageWaitForLoadStateOptions { Timeout = config.TimeoutMs });
                        
                        // 等待元素存在于DOM中
                        await locator.WaitForAsync(new LocatorWaitForOptions { 
                            State = WaitForSelectorState.Attached, 
                            Timeout = config.TimeoutMs 
                        });
                        
                        // 再等待元素可见
                        await locator.WaitForAsync(new LocatorWaitForOptions { 
                            State = WaitForSelectorState.Visible, 
                            Timeout = config.TimeoutMs 
                        });
                        
                        // 获取元素文本内容
                        var textContent = await locator.TextContentAsync();
                        
                        // 断言文本内容
                        if (string.IsNullOrEmpty(config.Value) || textContent.Contains(config.Value))
                        {
                            stepRecord.Logs.Add($"断言通过: 元素 '{config.Selector}' 包含文本 '{config.Value}'");
                        }
                        else
                        {
                            throw new Exception($"断言失败: 元素 '{config.Selector}' 不包含文本 '{config.Value}'。实际内容: {textContent}");
                        }
                    }
                    catch (Exception ex) {
                        // 记录详细错误信息
                        stepRecord.Logs.Add($"断言失败: {ex.Message}");
                        // 尝试截图记录当前页面状态
                        try {
                            var screenshotDir = GetScreenshotDir(testCase);
                            if (!Directory.Exists(screenshotDir))
                            {
                                Directory.CreateDirectory(screenshotDir);
                            }
                            var errorScreenshotPath = Path.Combine(screenshotDir, $"error_{step.Id}_{DateTime.Now:yyyyMMdd_HHmmss}.png");
                            await page.ScreenshotAsync(new PageScreenshotOptions { Path = errorScreenshotPath });
                            stepRecord.Logs.Add($"错误截图已保存: {errorScreenshotPath}");
                        }
                        catch (Exception screenshotEx) {
                            stepRecord.Logs.Add($"截图保存失败: {screenshotEx.Message}");
                        }
                        throw; // 重新抛出异常
                    }
                    break;

                case "wait":
                    await page.WaitForTimeoutAsync(int.Parse(config.Value));
                    stepRecord.Logs.Add($"Waited for {config.Value} milliseconds");
                    break;

                case "screenshot":
                    var screenshotDir1 = GetScreenshotDir(testCase);
                    if (!Directory.Exists(screenshotDir1))
                    {
                        Directory.CreateDirectory(screenshotDir1);
                    }
                    var screenshotPath = Path.Combine(screenshotDir1, $"screenshot_{step.Id}_{DateTime.Now:yyyyMMdd_HHmmss}.png");
                    await page.ScreenshotAsync(new PageScreenshotOptions { Path = screenshotPath });
                    stepRecord.Logs.Add($"Screenshot taken: {screenshotPath}");
                    break;

                default:
                    throw new NotSupportedException($"UI action '{config.Action}' is not supported");
            }

            // 如果需要截图，则执行截图操作
            if (config.TakeScreenshot)
            {
                var screenshotDir = GetScreenshotDir(testCase);
                if (!Directory.Exists(screenshotDir))
                {
                    Directory.CreateDirectory(screenshotDir);
                }
                var screenshotPath = Path.Combine(screenshotDir, $"screenshot_step_{step.Id}_{DateTime.Now:yyyyMMdd_HHmmss}.png");
                await page.ScreenshotAsync(new PageScreenshotOptions { Path = screenshotPath });
                stepRecord.Logs.Add($"Screenshot taken: {screenshotPath}");
            }
        }
        catch (Exception ex)
        {
            stepRecord.Logs.Add($"UI step failed: {ex.Message}");
            throw;
        }
    }
    
    /// <summary>
    /// 执行脚本步骤
    /// </summary>
    private async Task ExecuteScriptStepAsync(
        ProjectCaseStep step, 
        StepExecutionRecord stepRecord, 
        EnvironmentDto environment, 
        CancellationToken cancellationToken)
    {
        if (step.ScriptConfig == null)
        {
            throw new InvalidOperationException("Script configuration is required for script steps");
        }
        
        stepRecord.Logs.Add($"Executing {step.ScriptConfig.ScriptType} script: {step.Name}");
        
        try
        {
            string result;
            
            if (step.Type == StepType.JavascriptScript)
            {
                result = await ExecuteJavaScriptAsync(step.ScriptConfig, cancellationToken);
            }
            else if (step.Type == StepType.CSharpScript)
            {
                result = await ExecuteCSharpScriptAsync(step.ScriptConfig, cancellationToken);
            }
            else
            {
                throw new NotSupportedException($"Script type {step.Type} is not supported");
            }
            
            stepRecord.Logs.Add($"Script execution completed. Result: {result}");
            
            // 提取变量
            if (step.ScriptConfig.VariableExtractions.Any())
            {
                foreach (var extraction in step.ScriptConfig.VariableExtractions)
                {
                    // 简单的变量提取，实际实现可能需要更复杂的逻辑
                    var value = result; // 这里简化处理，实际可能需要解析JSON或其他格式
                    _variables[extraction.Key] = value;
                    stepRecord.Logs.Add($"Extracted variable {extraction.Key}: {value}");
                }
            }
        }
        catch (Exception ex)
        {
            stepRecord.Logs.Add($"Script execution failed: {ex.Message}");
            throw;
        }
    }
    
    /// <summary>
    /// 执行延时步骤
    /// </summary>
    private async Task ExecuteDelayStepAsync(
        ProjectCaseStep step,
        StepExecutionRecord stepRecord,
        CancellationToken cancellationToken)
    {
        int delayMs = 1000;
        if (step.Parameters.TryGetValue("delay", out var delayValue) && delayValue != null)
        {
            int.TryParse(delayValue.ToString(), out delayMs);
        }

        stepRecord.Logs.Add($"Waiting for {delayMs} ms...");
        await Task.Delay(delayMs, cancellationToken);
        stepRecord.Logs.Add($"Waited for {delayMs} ms");
    }
    
    /// <summary>
    /// 初始化变量
    /// </summary>
    private void InitializeVariables(EnvironmentDto environment, Dictionary<string, object> testData)
    {
        _variables.Clear();
        
        // 添加环境变量
        foreach (var config in environment.Config)
        {
            _variables[config.Key] = config.Value?.ToString() ?? "";
        }
        
        // 添加测试数据
        foreach (var data in testData)
        {
            _variables[data.Key] = data.Value?.ToString() ?? "";
        }
    }
    
    /// <summary>
    /// 替换变量占位符
    /// </summary>
    private string ReplaceVariables(string input)
    {
        if (string.IsNullOrEmpty(input))
            return input;
        
        var pattern = @"\{\{([^}]+)\}\}";
        return Regex.Replace(input, pattern, match =>
        {
            var variableName = match.Groups[1].Value.Trim();

            // 处理内置Mock数据变量
            if (variableName.Equals("MockData.Guid", StringComparison.OrdinalIgnoreCase))
            {
                return Guid.NewGuid().ToString();
            }
            if (variableName.Equals("MockData.SnowId", StringComparison.OrdinalIgnoreCase))
            {
                return SnowflakeIdGenerator.NextId().ToString();
            }
            
            return _variables.TryGetValue(variableName, out var value) ? value : match.Value;
        });
    }
    
    /// <summary>
    /// 获取服务端点
    /// </summary>
    private string GetServiceEndpoint(EnvironmentDto environment, long serviceId)
    {
        if (environment.ServiceEndpoints.TryGetValue(serviceId, out var endpoint))
        {
            return endpoint;
        }
        
        // 如果没有找到特定的服务端点，尝试使用默认的baseUrl
        if (environment.Config.TryGetValue("baseUrl", out var baseUrl))
        {
            return baseUrl.ToString() ?? "";
        }
        
        throw new InvalidOperationException($"Service endpoint for {serviceId} not found in environment configuration");
    }
    
    /// <summary>
    /// 执行断言
    /// </summary>
    private AssertionResult ExecuteAssertion(ApiAssertion assertion, string responseContent, int statusCode)
    {
        var result = new AssertionResult
        {
            Expression = assertion.Target,
            Expected = assertion.ExpectedValue,
            Operator = assertion.Operator
        };
        
        try
        {
            string actualValue;
            
            if (assertion.Type == "JsonPath")
            {
                var json = JsonNode.Parse(responseContent);
                var token = GetJsonNodeValue(json, assertion.Target);
                actualValue = token ?? "";
            }
            else if (assertion.Type == "StatusCode")
            {
                actualValue = statusCode.ToString();
            }
            else
            {
                actualValue = responseContent;
            }
            
            result.Actual = actualValue;
            
            // 执行比较
            result.Passed = assertion.Operator.ToLower() switch
            {
                "equals" => actualValue == assertion.ExpectedValue,
                "contains" => actualValue.Contains(assertion.ExpectedValue),
                "startswith" => actualValue.StartsWith(assertion.ExpectedValue),
                "endswith" => actualValue.EndsWith(assertion.ExpectedValue),
                "greaterthan" => double.TryParse(actualValue, out var actual) && double.TryParse(assertion.ExpectedValue, out var expected) && actual > expected,
                "lessthan" => double.TryParse(actualValue, out var actual2) && double.TryParse(assertion.ExpectedValue, out var expected2) && actual2 < expected2,
                _ => false
            };
            
            if (!result.Passed)
            {
                result.ErrorMessage = $"Expected {assertion.ExpectedValue} but got {actualValue}";
            }
        }
        catch (Exception ex)
        {
            result.Passed = false;
            result.ErrorMessage = ex.Message;
        }
        
        return result;
    }
    
    /// <summary>
    /// 提取变量
    /// </summary>
    private string ExtractVariable(string expression, string responseContent)
    {
        try
        {
            if (expression.StartsWith("$."))
            {
                // JSONPath表达式
                var json = JsonNode.Parse(responseContent);
                return GetJsonNodeValue(json, expression);
            }
            else
            {
                // 正则表达式或其他提取方式
                var match = Regex.Match(responseContent, expression);
                return match.Success ? match.Groups[1].Value : "";
            }
        }
        catch
        {
            return "";
        }
    }
    
    /// <summary>
    /// 判断是否为API步骤类型
    /// </summary>
    private static bool IsApiStepType(StepType stepType)
    {
        return stepType == StepType.HttpRequest || 
               stepType == StepType.ApiAssertion || 
               stepType == StepType.VariableExtraction;
    }
    
    /// <summary>
    /// 判断是否为UI步骤类型
    /// </summary>
    private static bool IsUiStepType(StepType stepType)
    {
        return stepType == StepType.Navigate || 
               stepType == StepType.Click || 
               stepType == StepType.Input || 
               stepType == StepType.Select || 
               stepType == StepType.Wait || 
               stepType == StepType.Assert || 
               stepType == StepType.Screenshot || 
               stepType == StepType.Scroll || 
               stepType == StepType.Hover || 
               stepType == StepType.KeyPress;
    }
    
    /// <summary>
    /// 判断是否为脚本步骤类型
    /// </summary>
    private static bool IsScriptStepType(StepType stepType)
    {
        return stepType == StepType.JavascriptScript || 
               stepType == StepType.CSharpScript;
    }
    
    /// <summary>
    /// 执行JavaScript脚本
    /// </summary>
    private async Task<string> ExecuteJavaScriptAsync(ScriptStepConfig config, CancellationToken cancellationToken)
    {
        // 这里是JavaScript执行的占位符实现
        // 实际实现可能需要集成JavaScript引擎，如V8或Jint
        await Task.Delay(100, cancellationToken); // 模拟执行时间
        
        // 简单的模拟执行结果
        var result = $"JavaScript executed: {config.ScriptContent.Substring(0, Math.Min(50, config.ScriptContent.Length))}...";
        return result;
    }
    
    /// <summary>
    /// 执行C#脚本
    /// </summary>
    private async Task<string> ExecuteCSharpScriptAsync(ScriptStepConfig config, CancellationToken cancellationToken)
    {
        // 这里是C#脚本执行的占位符实现
        // 实际实现可能需要集成Roslyn编译器或其他C#脚本引擎
        await Task.Delay(100, cancellationToken); // 模拟执行时间
        
        // 简单的模拟执行结果
        var result = $"C# script executed: {config.ScriptContent.Substring(0, Math.Min(50, config.ScriptContent.Length))}...";
        return result;
    }
    
    /// <summary>
    /// 简单的 JSONPath 解析器（支持 $.property.nestedProperty 格式）
    /// </summary>
    private string GetJsonNodeValue(JsonNode? jsonNode, string path)
    {
        if (jsonNode == null || string.IsNullOrEmpty(path))
        {
            return "";
        }

        try
        {
            // 移除开头的 $
            if (path.StartsWith("$"))
            {
                path = path.Substring(1);
            }

            // 移除开头的 .
            if (path.StartsWith("."))
            {
                path = path.Substring(1);
            }

            if (string.IsNullOrEmpty(path))
            {
                return jsonNode.ToString();
            }

            // 分割路径
            var parts = path.Split('.');
            JsonNode? currentNode = jsonNode;

            foreach (var part in parts)
            {
                if (currentNode == null)
                {
                    return "";
                }

                // 检查是否是数组索引 [index]
                if (part.StartsWith("[") && part.EndsWith("]"))
                {
                    var indexStr = part.Trim('[', ']');
                    if (int.TryParse(indexStr, out int index))
                    {
                        if (currentNode is JsonArray array && index >= 0 && index < array.Count)
                        {
                            currentNode = array[index];
                        }
                        else
                        {
                            return "";
                        }
                    }
                    else
                    {
                        return "";
                    }
                }
                else
                {
                    // 对象属性访问
                    if (currentNode is JsonObject obj)
                    {
                        currentNode = obj[part];
                    }
                    else
                    {
                        return "";
                    }
                }
            }

            return currentNode?.ToString() ?? "";
        }
        catch
        {
            return "";
        }
    }

}