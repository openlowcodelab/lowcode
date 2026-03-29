using H.Account.Application.Contracts;
using System.Net.Http.Json;

namespace H.Account.Client;

public class AccountClient
{
    private readonly HttpClient _httpClient;

    public AccountClient(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<AuthResponseDto> RegisterAsync(RegisterRequestDto request)
    {
        var response = await _httpClient.PostAsJsonAsync("api/auth/register", request);
        
        // 即使是 BadRequest 也需要读取响应体，因为错误信息在其中
        var result = await response.Content.ReadFromJsonAsync<AuthResponseDto>();
        return result ?? new AuthResponseDto { Success = false, Message = "注册失败，请稍后再试" };
    }

    public async Task<AuthResponseDto> LoginAsync(LoginRequestDto request)
    {
        var response = await _httpClient.PostAsJsonAsync("api/auth/login", request);
        
        // 即使是 Unauthorized 也需要读取响应体，因为错误信息在其中
        var result = await response.Content.ReadFromJsonAsync<AuthResponseDto>();
        return result ?? new AuthResponseDto { Success = false, Message = "登录失败，请稍后再试" };
    }

    public async Task<bool> ValidateTokenAsync(string token)
    {
        var response = await _httpClient.GetAsync($"api/auth/validate?token={token}");
        if (response.IsSuccessStatusCode)
        {
            var result = await response.Content.ReadAsStringAsync();
            return bool.Parse(result);
        }
        return false;
    }

    public void SetAuthToken(string token)
    {
        _httpClient.DefaultRequestHeaders.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
    }

    // 用户管理方法
    public async Task<PagedResult<UserDto>> GetUsersAsync(UserQueryParams queryParams)
    {
        var url = BuildQueryString("api/users", queryParams);
        return await _httpClient.GetFromJsonAsync<PagedResult<UserDto>>(url) ?? new PagedResult<UserDto>();
    }

    public async Task<UserDto?> GetUserAsync(Guid id)
    {
        return await _httpClient.GetFromJsonAsync<UserDto>($"api/users/{id}");
    }

    public async Task<UserDto> CreateUserAsync(CreateUserDto request)
    {
        var response = await _httpClient.PostAsJsonAsync("api/users", request);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<UserDto>() ?? throw new Exception("创建用户失败");
    }

    public async Task<UserDto> UpdateUserAsync(Guid id, UpdateUserDto request)
    {
        var response = await _httpClient.PutAsJsonAsync($"api/users/{id}", request);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<UserDto>() ?? throw new Exception("更新用户失败");
    }

    public async Task UpdateUserStatusAsync(Guid id, UpdateUserStatusDto request)
    {
        var response = await _httpClient.PatchAsJsonAsync($"api/users/{id}/status", request);
        response.EnsureSuccessStatusCode();
    }

    public async Task ResetPasswordAsync(Guid id, ResetPasswordDto request)
    {
        var response = await _httpClient.PostAsJsonAsync($"api/users/{id}/reset-password", request);
        response.EnsureSuccessStatusCode();
    }

    public async Task DeleteUserAsync(Guid id)
    {
        var response = await _httpClient.DeleteAsync($"api/users/{id}");
        response.EnsureSuccessStatusCode();
    }

    public async Task<bool> CheckUserNameExistsAsync(string userName, Guid? excludeId = null)
    {
        var url = excludeId.HasValue
            ? $"api/users/check-username?userName={userName}&excludeId={excludeId}"
            : $"api/users/check-username?userName={userName}";
        return await _httpClient.GetFromJsonAsync<bool>(url);
    }

    public async Task<bool> CheckEmailExistsAsync(string email, Guid? excludeId = null)
    {
        var url = excludeId.HasValue
            ? $"api/users/check-email?email={email}&excludeId={excludeId}"
            : $"api/users/check-email?email={email}";
        return await _httpClient.GetFromJsonAsync<bool>(url);
    }

    private static string BuildQueryString<T>(string baseUrl, T queryParams)
    {
        var properties = typeof(T).GetProperties();
        var parameters = new List<string>();

        foreach (var prop in properties)
        {
            var value = prop.GetValue(queryParams);
            if (value != null)
            {
                parameters.Add($"{prop.Name}={value}");
            }
        }

        return parameters.Any()
            ? $"{baseUrl}?{string.Join("&", parameters)}"
            : baseUrl;
    }
}