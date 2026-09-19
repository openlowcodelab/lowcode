using System.Collections.Concurrent;

namespace H.Workbench.Core.Tools.Internal;

/// <summary>
/// 按仓库目录串行的写操作锁，防止同一工作副本被并发 clone/pull/commit 破坏。
/// 单例注入；只读命令（status/log）无需加锁。
/// </summary>
public class GitWorkspaceLocks
{
    private readonly ConcurrentDictionary<string, SemaphoreSlim> _semaphores =
        new(StringComparer.OrdinalIgnoreCase);

    public async Task<IDisposable> AcquireAsync(string directory, CancellationToken ct = default)
    {
        var key = Path.GetFullPath(directory);
        var semaphore = _semaphores.GetOrAdd(key, _ => new SemaphoreSlim(1, 1));
        await semaphore.WaitAsync(ct);
        return new Releaser(semaphore);
    }

    private sealed class Releaser(SemaphoreSlim semaphore) : IDisposable
    {
        public void Dispose() => semaphore.Release();
    }
}
