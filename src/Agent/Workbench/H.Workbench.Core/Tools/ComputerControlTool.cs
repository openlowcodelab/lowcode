using H.Workbench.Core.Tools.Internal;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;
using System.Runtime.Versioning;
using System.Text;

namespace H.Workbench.Core.Tools;

/// <summary>
/// 计算机控制工具：截屏、窗口枚举/激活、鼠标移动点击、键盘输入。
/// 它操作的是宿主所在这台机器的真实桌面，没有任何沙箱可言，因此：
/// 1) 技能级默认要求人工批准（SeedingRequiresApproval 见 AgentSkillDataSeeder）；
/// 2) 每个动作只做一个原子操作并回报结果，不提供"批量脚本"入口；
/// 3) 非 Windows 直接拒绝，不做半套实现。
/// </summary>
[SupportedOSPlatform("windows")]
public class ComputerControlTool
{
    private const int MaxTypedChars = 2000;

    private static readonly Dictionary<string, ushort> VirtualKeys = new(StringComparer.OrdinalIgnoreCase)
    {
        ["enter"] = 0x0D, ["return"] = 0x0D, ["tab"] = 0x09, ["esc"] = 0x1B, ["escape"] = 0x1B,
        ["space"] = 0x20, ["backspace"] = 0x08, ["delete"] = 0x2E, ["del"] = 0x2E,
        ["insert"] = 0x2D, ["home"] = 0x24, ["end"] = 0x23, ["pageup"] = 0x21, ["pagedown"] = 0x22,
        ["up"] = 0x26, ["down"] = 0x28, ["left"] = 0x25, ["right"] = 0x27,
        ["printscreen"] = 0x2C, ["f1"] = 0x70, ["f2"] = 0x71, ["f3"] = 0x72, ["f4"] = 0x73,
        ["f5"] = 0x74, ["f6"] = 0x75, ["f7"] = 0x76, ["f8"] = 0x77, ["f9"] = 0x78,
        ["f10"] = 0x79, ["f11"] = 0x7A, ["f12"] = 0x7B
    };

    private static readonly Dictionary<string, ushort> Modifiers = new(StringComparer.OrdinalIgnoreCase)
    {
        ["ctrl"] = 0x11, ["control"] = 0x11, ["alt"] = 0x12, ["shift"] = 0x10, ["win"] = 0x5B, ["meta"] = 0x5B
    };

    private readonly GitWorkspaceResolver _resolver;
    private readonly GitWorkspaceLocks _locks;
    private readonly ILogger<ComputerControlTool> _logger;

    public ComputerControlTool(
        IOptions<WorkbenchToolOptions> options,
        GitWorkspaceLocks locks,
        ILogger<ComputerControlTool> logger)
    {
        _resolver = new GitWorkspaceResolver(options);
        _locks = locks;
        _logger = logger;
    }

    [Description("截取当前桌面屏幕并保存为 PNG 到服务端工作目录，用于让员工「看见」屏幕。参数：fileName（.png 相对路径，留空自动命名）, repo（留空则存到工作目录 outputs 下）, screenScope（0=所有显示器拼成的虚拟桌面，1=仅主显示器）。")]
    public async Task<string> ScreenCaptureAsync(
        [Description("图片相对路径，必须以 .png 结尾，可空")] string? fileName = null,
        [Description("已克隆的仓库目录名或地址，可空")] string? repo = null,
        [Description("0=虚拟桌面（含所有显示器），1=仅主显示器")] int screenScope = 0,
        CancellationToken cancellationToken = default)
    {
        if (!OperatingSystem.IsWindows()) return ToolEnvelope.Fail("计算机控制仅在 Windows 桌面会话可用");

        var bounds = screenScope <= 0 ? VirtualScreenBounds() : PrimaryScreenBounds();

        var name = string.IsNullOrWhiteSpace(fileName)
            ? $"screenshots/desktop-{DateTime.Now:yyyyMMdd-HHmmss}.png"
            : fileName.Trim();

        if (!name.EndsWith(".png", StringComparison.OrdinalIgnoreCase))
        {
            return ToolEnvelope.Fail("fileName 必须以 .png 结尾");
        }

        if (!_resolver.TryResolveOutputPath(repo, name, out var fullPath, out var resolveError))
        {
            return ToolEnvelope.Fail(resolveError!);
        }

        try
        {
            Directory.CreateDirectory(Path.GetDirectoryName(fullPath!)!);
            byte[] bytes;

            using (await _locks.AcquireAsync(Path.GetDirectoryName(fullPath!)!, cancellationToken))
            using (var bitmap = new Bitmap(Math.Max(1, bounds.Width), Math.Max(1, bounds.Height), PixelFormat.Format32bppPArgb))
            {
                using (var graphics = Graphics.FromImage(bitmap))
                {
                    graphics.CopyFromScreen(bounds.Left, bounds.Top, 0, 0, bitmap.Size, CopyPixelOperation.SourceCopy);
                }

                using var stream = new MemoryStream();
                // 走 Stream 而不是 Save(path)：中文路径下 GDI+ 的编码行为不可靠
                bitmap.Save(stream, ImageFormat.Png);
                bytes = stream.ToArray();
            }

            await File.WriteAllBytesAsync(fullPath!, bytes, cancellationToken);

            return ToolEnvelope.Ok(new
            {
                file = Path.GetRelativePath(_resolver.WorkDir, fullPath!).Replace('\\', '/'),
                width = bounds.Width,
                height = bounds.Height,
                sizeBytes = bytes.LongLength,
                note = "坐标以屏幕左上角为原点，单位像素"
            });
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "屏幕截图失败");
            return ToolEnvelope.Fail($"屏幕截图失败: {ex.Message}");
        }
    }

    [Description("列出当前可见的窗口（标题、句柄、位置与大小）。参数：keyword（按标题筛选，可空）, maxCount（默认 40）。")]
    public Task<string> ListWindowsAsync(
        [Description("标题关键字，可空")] string? keyword = null,
        int maxCount = 40,
        CancellationToken cancellationToken = default)
    {
        if (!OperatingSystem.IsWindows()) return Task.FromResult(ToolEnvelope.Fail("计算机控制仅在 Windows 桌面会话可用"));

        var wanted = Math.Clamp(maxCount > 0 ? maxCount : 40, 1, 200);
        var found = new List<object>();

        EnumWindows((handle, _) =>
        {
            if (!IsWindowVisible(handle)) return true;

            var title = ReadWindowTitle(handle);
            if (string.IsNullOrWhiteSpace(title)) return true;
            if (!string.IsNullOrWhiteSpace(keyword) &&
                !title.Contains(keyword.Trim(), StringComparison.OrdinalIgnoreCase)) return true;

            GetWindowRect(handle, out var rect);
            found.Add(new
            {
                handle = (long)handle,
                title,
                left = rect.Left,
                top = rect.Top,
                right = rect.Right,
                bottom = rect.Bottom,
                minimized = IsIconic(handle)
            });

            return found.Count < wanted;
        }, IntPtr.Zero);

        return Task.FromResult(ToolEnvelope.Ok(new
        {
            count = found.Count,
            windows = found,
            note = "先 ActivateWindowAsync 聚焦目标窗口，再移动鼠标或输入，否则输入会落到当前焦点窗口"
        }));
    }

    [Description("激活（前置）指定句柄的窗口，必要时从最小化恢复。参数：handle（ListWindowsAsync 返回的句柄）。")]
    public Task<string> ActivateWindowAsync(
        [Description("窗口句柄")] long handle,
        CancellationToken cancellationToken = default)
    {
        if (!OperatingSystem.IsWindows()) return Task.FromResult(ToolEnvelope.Fail("计算机控制仅在 Windows 桌面会话可用"));

        var target = new IntPtr(handle);
        if (!IsWindow(target)) return Task.FromResult(ToolEnvelope.Fail($"窗口句柄无效: {handle}"));

        if (IsIconic(target)) ShowWindow(target, SwRestore);
        BringWindowToTop(target);
        var ok = SetForegroundWindow(target);
        var title = ReadWindowTitle(target);

        return Task.FromResult(ok || !string.IsNullOrWhiteSpace(title)
            ? ToolEnvelope.Ok(new { handle, title, focused = ok })
            : ToolEnvelope.Fail($"无法前置窗口 {handle}（系统可能限制后台进程改焦点）。可先用鼠标点击该窗口区域"));
    }

    [Description("移动鼠标指针到屏幕坐标。参数：x, y。")]
    public Task<string> MoveMouseAsync(int x, int y, CancellationToken cancellationToken = default)
    {
        if (!OperatingSystem.IsWindows()) return Task.FromResult(ToolEnvelope.Fail("计算机控制仅在 Windows 桌面会话可用"));

        var bounds = VirtualScreenBounds();
        if (!InBounds(x, y, bounds, out var boundError)) return Task.FromResult(ToolEnvelope.Fail(boundError!));

        var moved = SetCursorPos(x, y);
        return Task.FromResult(moved
            ? ToolEnvelope.Ok(new { x, y })
            : ToolEnvelope.Fail("SetCursorPos 失败，屏幕可能已锁或处于安全桌面"));
    }

    [Description("在屏幕坐标处点击鼠标（会先移动到该点）。参数：x, y, button（left/right/middle）, clicks（1 或 2）。")]
    public Task<string> ClickMouseAsync(
        int x, int y,
        [Description("left / right / middle，默认 left")] string button = "left",
        int clicks = 1,
        CancellationToken cancellationToken = default)
    {
        if (!OperatingSystem.IsWindows()) return Task.FromResult(ToolEnvelope.Fail("计算机控制仅在 Windows 桌面会话可用"));

        var bounds = VirtualScreenBounds();
        if (!InBounds(x, y, bounds, out var boundError)) return Task.FromResult(ToolEnvelope.Fail(boundError!));

        var kind = (button ?? "left").Trim().ToLowerInvariant();
        (uint down, uint up) = kind switch
        {
            "right" => (MouseEventRightDown, MouseEventRightUp),
            "middle" => (MouseEventMiddleDown, MouseEventMiddleUp),
            "left" => (MouseEventLeftDown, MouseEventLeftUp),
            _ => (0u, 0u)
        };

        if (down == 0) return Task.FromResult(ToolEnvelope.Fail($"不支持的鼠标键: {button}（可用 left/right/middle）"));

        if (!SetCursorPos(x, y)) return Task.FromResult(ToolEnvelope.Fail("SetCursorPos 失败"));

        var times = Math.Clamp(clicks > 0 ? clicks : 1, 1, 3);
        for (var i = 0; i < times; i++)
        {
            SendMouse(down);
            SendMouse(up);
        }

        var elementUnder = "";
        try
        {
            var point = new POINT { X = x, Y = y };
            var hwnd = WindowFromPoint(point);
            if (hwnd != IntPtr.Zero) elementUnder = ReadWindowTitle(hwnd);
        }
        catch
        {
            // 只为回报"点到哪了"，取不到不影响点击本身
        }

        return Task.FromResult(ToolEnvelope.Ok(new
        {
            x,
            y,
            button = kind,
            clicks = times,
            windowUnderCursor = string.IsNullOrWhiteSpace(elementUnder) ? null : elementUnder
        }));
    }

    [Description("用键盘输入文本（Unicode，支持中文；输入目标是当前焦点窗口）。参数：text（上限 2000 字）。")]
    public Task<string> TypeTextAsync(
        [Description("要输入的文本")] string text,
        CancellationToken cancellationToken = default)
    {
        if (!OperatingSystem.IsWindows()) return Task.FromResult(ToolEnvelope.Fail("计算机控制仅在 Windows 桌面会话可用"));

        var value = text ?? "";
        if (value.Length == 0) return Task.FromResult(ToolEnvelope.Fail("text 不能为空"));
        if (value.Length > MaxTypedChars) return Task.FromResult(ToolEnvelope.Fail($"text 超过 {MaxTypedChars} 字上限"));

        var inputs = new List<INPUT>();
        foreach (var ch in value)
        {
            inputs.Add(KeyInput(ch, KeyEventUnicode));
            inputs.Add(KeyInput((char)0, KeyEventUnicode | KeyEventKeyup));
        }

        var sent = SendInputBatch(inputs);
        return sent == inputs.Count
            ? Task.FromResult(ToolEnvelope.Ok(new { chars = value.Length, sent = sent }))
            : Task.FromResult(ToolEnvelope.Fail($"键盘输入未全部送达（发出 {sent}/{inputs.Count} 个事件）。焦点可能不在目标窗口"));
    }

    [Description("按下组合键或功能键。参数：keys，用 + 分隔，例如 ctrl+s、alt+tab、ctrl+shift+esc、f5、enter。可用修饰键 ctrl/alt/shift/win，可用功能键 enter/tab/esc/space/backspace/delete/home/end/pageup/pagedown/up/down/left/right/printscreen/f1-f12，也支持单个字母数字。")]
    public Task<string> PressKeysAsync(
        [Description("组合键写法，例如 ctrl+s")] string keys,
        CancellationToken cancellationToken = default)
    {
        if (!OperatingSystem.IsWindows()) return Task.FromResult(ToolEnvelope.Fail("计算机控制仅在 Windows 桌面会话可用"));

        var parts = (keys ?? "").Split('+', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        if (parts.Length == 0) return Task.FromResult(ToolEnvelope.Fail("keys 不能为空"));

        var inputs = new List<INPUT>();
        var pressed = new List<string>();

        foreach (var part in parts)
        {
            if (!TryResolveKey(part, out var vk, out var scan, out var unicode))
            {
                return Task.FromResult(ToolEnvelope.Fail($"无法识别的按键: {part}"));
            }

            var flags = unicode ? KeyEventUnicode : 0u;
            inputs.Add(new INPUT
            {
                type = InputKeyboard,
                ki = new KEYBDINPUT { wVk = vk, wScan = scan, dwFlags = flags }
            });
            inputs.Add(new INPUT
            {
                type = InputKeyboard,
                ki = new KEYBDINPUT { wVk = vk, wScan = scan, dwFlags = flags | KeyEventKeyup }
            });
            pressed.Add(part);
        }

        var sent = SendInputBatch(inputs);
        return sent == inputs.Count
            ? Task.FromResult(ToolEnvelope.Ok(new { keys = string.Join("+", pressed), sent = sent }))
            : Task.FromResult(ToolEnvelope.Fail($"按键未全部送达（发出 {sent}/{inputs.Count} 个事件）"));
    }

    private static bool TryResolveKey(string part, out ushort vk, out ushort scan, out bool unicode)
    {
        vk = 0;
        scan = 0;
        unicode = false;

        if (VirtualKeys.TryGetValue(part, out var mapped))
        {
            vk = mapped;
            return true;
        }

        if (Modifiers.TryGetValue(part, out var modifier))
        {
            vk = modifier;
            return true;
        }

        if (part.Length == 1)
        {
            var c = char.ToUpperInvariant(part[0]);
            if (c is >= 'A' and <= 'Z' || c is >= '0' and <= '9')
            {
                vk = c;
                return true;
            }

            // 其余单字符走 Unicode 注入：wVk 必须留 0，扫描码放字符本身，
            // 否则 SendInput 会把它当成虚拟键码而丢事件
            vk = 0;
            scan = part[0];
            unicode = true;
            return true;
        }

        return false;
    }

    private static INPUT KeyInput(char ch, uint flags) => new()
    {
        type = InputKeyboard,
        ki = new KEYBDINPUT { wVk = 0, wScan = ch, dwFlags = flags }
    };

    private static void SendMouse(uint flags)
    {
        var input = new INPUT
        {
            type = InputMouse,
            mi = new MOUSEINPUT { dx = 0, dy = 0, mouseData = 0, dwFlags = flags }
        };

        SendInput(1, [input], Marshal.SizeOf<INPUT>());
    }

    private static int SendInputBatch(List<INPUT> inputs)
    {
        if (inputs.Count == 0) return 0;
        var array = inputs.ToArray();
        return (int)SendInput((uint)array.Length, array, Marshal.SizeOf<INPUT>());
    }

    private static bool InBounds(int x, int y, Rectangle bounds, out string? error)
    {
        error = null;
        if (x < bounds.Left || y < bounds.Top || x >= bounds.Right || y >= bounds.Bottom)
        {
            error = $"坐标 ({x},{y}) 超出屏幕范围 {bounds.Left}..{bounds.Right} × {bounds.Top}..{bounds.Bottom}，" +
                    "先用 ScreenCaptureAsync 或 ListWindowsAsync 确认目标位置";
            return false;
        }

        return true;
    }

    private static Rectangle VirtualScreenBounds()
    {
        var x = GetSystemMetrics(SmXVirtualScreen);
        var y = GetSystemMetrics(SmYVirtualScreen);
        var width = GetSystemMetrics(SmCxVirtualScreen);
        var height = GetSystemMetrics(SmCyVirtualScreen);

        if (width <= 0 || height <= 0) return PrimaryScreenBounds();
        return new Rectangle(x, y, width, height);
    }

    private static Rectangle PrimaryScreenBounds()
    {
        var width = GetSystemMetrics(SmCxScreen);
        var height = GetSystemMetrics(SmCyScreen);
        return new Rectangle(0, 0, Math.Max(1, width), Math.Max(1, height));
    }

    private static string ReadWindowTitle(IntPtr handle)
    {
        var length = GetWindowTextLength(handle);
        if (length <= 0) return string.Empty;

        var builder = new StringBuilder(length + 1);
        GetWindowText(handle, builder, builder.Capacity);
        return builder.ToString().Trim();
    }

    private delegate bool EnumWindowsProc(IntPtr hWnd, IntPtr lParam);

    [DllImport("user32.dll")] private static extern bool EnumWindows(EnumWindowsProc lpEnumFunc, IntPtr lParam);
    [DllImport("user32.dll")] private static extern bool IsWindowVisible(IntPtr hWnd);
    [DllImport("user32.dll")] private static extern bool IsWindow(IntPtr hWnd);
    [DllImport("user32.dll")] private static extern bool IsIconic(IntPtr hWnd);
    [DllImport("user32.dll")] private static extern bool SetForegroundWindow(IntPtr hWnd);
    [DllImport("user32.dll")] private static extern bool BringWindowToTop(IntPtr hWnd);
    [DllImport("user32.dll")] private static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);
    [DllImport("user32.dll", CharSet = CharSet.Unicode)] private static extern int GetWindowTextLength(IntPtr hWnd);
    [DllImport("user32.dll", CharSet = CharSet.Unicode)] private static extern int GetWindowText(IntPtr hWnd, StringBuilder lpString, int nMaxCount);
    [DllImport("user32.dll")] private static extern bool SetCursorPos(int x, int y);
    [DllImport("user32.dll")] private static extern int GetSystemMetrics(int nIndex);
    [DllImport("user32.dll")] private static extern IntPtr WindowFromPoint(POINT point);
    [DllImport("user32.dll", SetLastError = true)] private static extern uint SendInput(uint nInputs, INPUT[] pInputs, int cbSize);

    [StructLayout(LayoutKind.Sequential)] private struct POINT { public int X; public int Y; }

    [StructLayout(LayoutKind.Sequential)] private struct RECT { public int Left; public int Top; public int Right; public int Bottom; }

    [DllImport("user32.dll")] private static extern bool GetWindowRect(IntPtr hWnd, out RECT lpRect);

    [StructLayout(LayoutKind.Sequential)]
    private struct MOUSEINPUT
    {
        public int dx;
        public int dy;
        public uint mouseData;
        public uint dwFlags;
        public uint time;
        public IntPtr dwExtraInfo;
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct KEYBDINPUT
    {
        public ushort wVk;
        public ushort wScan;
        public uint dwFlags;
        public uint time;
        public IntPtr dwExtraInfo;
    }

    [StructLayout(LayoutKind.Explicit)]
    private struct InputUnion
    {
        [FieldOffset(0)] public MOUSEINPUT mi;
        [FieldOffset(0)] public KEYBDINPUT ki;
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct INPUT
    {
        public uint type;
        public InputUnion union;

        public MOUSEINPUT mi { readonly get => union.mi; set => union.mi = value; }
        public KEYBDINPUT ki { readonly get => union.ki; set => union.ki = value; }
    }

    private const uint InputMouse = 0;
    private const uint InputKeyboard = 1;
    private const uint MouseEventLeftDown = 0x0002;
    private const uint MouseEventLeftUp = 0x0004;
    private const uint MouseEventRightDown = 0x0008;
    private const uint MouseEventRightUp = 0x0010;
    private const uint MouseEventMiddleDown = 0x0020;
    private const uint MouseEventMiddleUp = 0x0040;
    private const uint KeyEventExtendedKey = 0x0002;
    private const uint KeyEventUnicode = 0x0004;
    private const uint KeyEventKeyup = 0x0002;
    private const int SwRestore = 9;
    private const int SmCxScreen = 0;
    private const int SmCyScreen = 1;
    private const int SmXVirtualScreen = 76;
    private const int SmYVirtualScreen = 77;
    private const int SmCxVirtualScreen = 78;
    private const int SmCyVirtualScreen = 79;
}
