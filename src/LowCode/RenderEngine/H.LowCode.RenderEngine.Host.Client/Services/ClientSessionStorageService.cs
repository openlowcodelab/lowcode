using H.LowCode.ComponentBase;
using Microsoft.JSInterop;

namespace H.LowCode.RenderEngine.Host.Client;

public class ClientSessionStorageService : ISessionStorageService
{
    private readonly IJSRuntime _jsRuntime;

    public ClientSessionStorageService(IJSRuntime jsRuntime)
    {
        _jsRuntime = jsRuntime;
    }

    public async Task SetAsync(string key, string value)
    {
        try
        {
            await _jsRuntime.InvokeVoidAsync("sessionStorage.setItem", key, value);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error storing session data: {ex.Message}");
            throw;
        }
    }

    public async Task<string?> GetAsync(string key)
    {
        try
        {
            var value = await _jsRuntime.InvokeAsync<string>(
                "sessionStorage.getItem", key);

            if (string.IsNullOrEmpty(value))
                return default;

            return value;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error retrieving session data: {ex.Message}");
            return default;
        }
    }

    public async Task RemoveAsync(string key)
    {
        try
        {
            await _jsRuntime.InvokeVoidAsync("sessionStorage.removeItem", key);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error removing session data: {ex.Message}");
        }
    }

    public async Task ClearAsync()
    {
        try
        {
            await _jsRuntime.InvokeVoidAsync("sessionStorage.clear");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error clearing session data: {ex.Message}");
        }
    }
}
