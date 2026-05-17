using H.SystemManagement.Application.Contracts;
using Volo.Abp.Application.Services;
using Volo.Abp.SettingManagement;

namespace H.SystemManagement.Application;

public class SettingAppService : ApplicationService, ISettingAppService
{
    private readonly ISettingManager _settingManager;

    public SettingAppService(ISettingManager settingManager)
    {
        _settingManager = settingManager;
    }

    public async Task<List<SettingGroupDto>> GetAllAsync()
    {
        var settingValues = await _settingManager.GetAllAsync(null, null, true);
        var groups = settingValues
            .GroupBy(x => x.Name)
            .Select(g => new SettingGroupDto
            {
                GroupName = g.Key,
                GroupDisplayName = g.Key,
                Items = g.Select(x => new SettingItemDto
                {
                    Name = x.Name,
                    DisplayName = x.Name,
                    Value = x.Value,
                    GroupName = x.Name
                }).ToList()
            }).ToList();

        return groups;
    }

    public async Task<List<SettingItemDto>> GetByGroupAsync(string groupName)
    {
        var settingValues = await _settingManager.GetAllAsync(null, null, true);
        return settingValues
            .Where(x => x.Name == groupName)
            .Select(x => new SettingItemDto
            {
                Name = x.Name,
                DisplayName = x.Name,
                Value = x.Value,
                GroupName = x.Name
            }).ToList();
    }

    public async Task<SettingItemDto> GetAsync(string name)
    {
        var value = await _settingManager.GetOrNullAsync(name, null, null, true);
        return new SettingItemDto
        {
            Name = name,
            DisplayName = name,
            Value = value,
            GroupName = name
        };
    }

    public async Task UpdateAsync(UpdateSettingItemDto input)
    {
        await _settingManager.SetAsync(input.Name, input.Value, null, null, true);
    }

    public async Task UpdateManyAsync(List<UpdateSettingItemDto> inputs)
    {
        foreach (var input in inputs)
        {
            await _settingManager.SetAsync(input.Name, input.Value, null, null, true);
        }
    }
}
