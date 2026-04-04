using H.LowCode.ComponentBase;

namespace H.LowCode.Host.All.Shared.Services;

/// <summary>
/// 服务端 Session 存储服务实现
/// </summary>
public class ServerSessionStorageService : ISessionStorageService
{
    // TODO: 需要在服务端项目中实现,此处仅提供接口
    public Task SetAsync(string key, string value)
    {
        throw new NotImplementedException("ServerSessionStorageService should be registered in server project");
    }

    public Task<string?> GetAsync(string key)
    {
        throw new NotImplementedException("ServerSessionStorageService should be registered in server project");
    }

    public Task RemoveAsync(string key)
    {
        throw new NotImplementedException("ServerSessionStorageService should be registered in server project");
    }

    public Task ClearAsync()
    {
        throw new NotImplementedException("ServerSessionStorageService should be registered in server project");
    }
}
