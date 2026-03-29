using H.Organization.Application.Contracts;
using System.Net.Http.Json;

namespace H.Organization.Client;

/// <summary>
/// 组织架构客户端
/// </summary>
public class OrganizationClient
{
    private readonly HttpClient _httpClient;

    public OrganizationClient(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    #region 部门管理

    /// <summary>
    /// 获取所有部门（树形结构）
    /// </summary>
    public async Task<List<OrganizationTreeDto>> GetAllAsTreeAsync()
    {
        return await _httpClient.GetFromJsonAsync<List<OrganizationTreeDto>>("api/organizations/tree") ?? new();
    }

    /// <summary>
    /// 获取部门列表
    /// </summary>
    public async Task<PagedResult<OrganizationDto>> GetOrganizationsAsync(OrganizationQueryParams queryParams)
    {
        var url = BuildQueryString("api/organizations", queryParams);
        return await _httpClient.GetFromJsonAsync<PagedResult<OrganizationDto>>(url) ?? new PagedResult<OrganizationDto>();
    }

    /// <summary>
    /// 获取部门详情
    /// </summary>
    public async Task<OrganizationDto?> GetOrganizationAsync(Guid id)
    {
        return await _httpClient.GetFromJsonAsync<OrganizationDto>($"api/organizations/{id}");
    }

    /// <summary>
    /// 创建部门
    /// </summary>
    public async Task<OrganizationDto> CreateOrganizationAsync(CreateOrganizationDto request)
    {
        var response = await _httpClient.PostAsJsonAsync("api/organizations", request);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<OrganizationDto>() ?? throw new Exception("创建部门失败");
    }

    /// <summary>
    /// 更新部门
    /// </summary>
    public async Task<OrganizationDto> UpdateOrganizationAsync(Guid id, UpdateOrganizationDto request)
    {
        var response = await _httpClient.PutAsJsonAsync($"api/organizations/{id}", request);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<OrganizationDto>() ?? throw new Exception("更新部门失败");
    }

    /// <summary>
    /// 删除部门
    /// </summary>
    public async Task DeleteOrganizationAsync(Guid id)
    {
        var response = await _httpClient.DeleteAsync($"api/organizations/{id}");
        response.EnsureSuccessStatusCode();
    }

    /// <summary>
    /// 批量删除部门
    /// </summary>
    public async Task BatchDeleteOrganizationsAsync(List<Guid> ids)
    {
        var response = await _httpClient.PostAsJsonAsync("api/organizations/batch-delete", ids);
        response.EnsureSuccessStatusCode();
    }

    /// <summary>
    /// 获取部门用户
    /// </summary>
    public async Task<List<Guid>> GetOrganizationUserIdsAsync(Guid organizationId, bool includeChildren = true)
    {
        var url = $"api/organizations/{organizationId}/users?includeChildren={includeChildren}";
        return await _httpClient.GetFromJsonAsync<List<Guid>>(url) ?? new List<Guid>();
    }

    #endregion

    #region 成员管理

    /// <summary>
    /// 获取成员列表
    /// </summary>
    public async Task<PagedResult<MemberDto>> GetMembersAsync(MemberQueryParams queryParams)
    {
        var url = BuildQueryString("api/members", queryParams);
        return await _httpClient.GetFromJsonAsync<PagedResult<MemberDto>>(url) ?? new PagedResult<MemberDto>();
    }

    /// <summary>
    /// 获取成员详情
    /// </summary>
    public async Task<MemberDto?> GetMemberAsync(Guid id)
    {
        return await _httpClient.GetFromJsonAsync<MemberDto>($"api/members/{id}");
    }

    /// <summary>
    /// 添加成员
    /// </summary>
    public async Task<MemberDto> AddMemberAsync(AddMemberDto request)
    {
        var response = await _httpClient.PostAsJsonAsync("api/members", request);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<MemberDto>() ?? throw new Exception("添加成员失败");
    }

    /// <summary>
    /// 更新成员
    /// </summary>
    public async Task<MemberDto> UpdateMemberAsync(Guid id, UpdateMemberDto request)
    {
        var response = await _httpClient.PutAsJsonAsync($"api/members/{id}", request);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<MemberDto>() ?? throw new Exception("更新成员失败");
    }

    /// <summary>
    /// 删除成员
    /// </summary>
    public async Task DeleteMemberAsync(Guid id)
    {
        var response = await _httpClient.DeleteAsync($"api/members/{id}");
        response.EnsureSuccessStatusCode();
    }

    /// <summary>
    /// 批量删除成员
    /// </summary>
    public async Task BatchDeleteMembersAsync(List<Guid> ids)
    {
        var response = await _httpClient.PostAsJsonAsync("api/members/batch-delete", ids);
        response.EnsureSuccessStatusCode();
    }

    /// <summary>
    /// 获取部门下所有成员
    /// </summary>
    public async Task<List<MemberDto>> GetMembersByOrganizationIdAsync(Guid organizationId)
    {
        return await _httpClient.GetFromJsonAsync<List<MemberDto>>($"api/members/by-organization/{organizationId}") ?? new List<MemberDto>();
    }

    /// <summary>
    /// 检查用户是否已是部门成员
    /// </summary>
    public async Task<bool> MemberExistsAsync(Guid organizationId, Guid userId)
    {
        return await _httpClient.GetFromJsonAsync<bool>($"api/members/exists?organizationId={organizationId}&userId={userId}");
    }

    #endregion

    #region 角色管理

    /// <summary>
    /// 获取角色列表
    /// </summary>
    public async Task<PagedResult<RoleDto>> GetRolesAsync(RoleQueryParams queryParams)
    {
        var url = BuildQueryString("api/roles", queryParams);
        return await _httpClient.GetFromJsonAsync<PagedResult<RoleDto>>(url) ?? new PagedResult<RoleDto>();
    }

    /// <summary>
    /// 获取角色详情
    /// </summary>
    public async Task<RoleDto?> GetRoleAsync(Guid id)
    {
        return await _httpClient.GetFromJsonAsync<RoleDto>($"api/roles/{id}");
    }

    /// <summary>
    /// 创建角色
    /// </summary>
    public async Task<RoleDto> CreateRoleAsync(CreateRoleDto request)
    {
        var response = await _httpClient.PostAsJsonAsync("api/roles", request);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<RoleDto>() ?? throw new Exception("创建角色失败");
    }

    /// <summary>
    /// 更新角色
    /// </summary>
    public async Task<RoleDto> UpdateRoleAsync(Guid id, UpdateRoleDto request)
    {
        var response = await _httpClient.PutAsJsonAsync($"api/roles/{id}", request);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<RoleDto>() ?? throw new Exception("更新角色失败");
    }

    /// <summary>
    /// 删除角色
    /// </summary>
    public async Task DeleteRoleAsync(Guid id)
    {
        var response = await _httpClient.DeleteAsync($"api/roles/{id}");
        response.EnsureSuccessStatusCode();
    }

    /// <summary>
    /// 批量删除角色
    /// </summary>
    public async Task BatchDeleteRolesAsync(List<Guid> ids)
    {
        var response = await _httpClient.PostAsJsonAsync("api/roles/batch-delete", ids);
        response.EnsureSuccessStatusCode();
    }

    /// <summary>
    /// 获取所有启用的角色
    /// </summary>
    public async Task<List<RoleDto>> GetAllEnabledRolesAsync()
    {
        return await _httpClient.GetFromJsonAsync<List<RoleDto>>("api/roles/enabled") ?? new List<RoleDto>();
    }

    #endregion

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
