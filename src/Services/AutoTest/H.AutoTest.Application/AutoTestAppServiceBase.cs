using System.Text.Json;
using Volo.Abp.Application.Services;

namespace H.AutoTest.Application;

/// <summary>
/// Base service for JSON file data operations
/// </summary>
public abstract class AutoTestAppServiceBase<T> : ApplicationService where T : class
{
    private readonly string _filePath;

    protected AutoTestAppServiceBase(string filePath)
    {
        _filePath = filePath;
    }

    public async Task<List<T>> GetAllAsync()
    {
        return await LoadDataAsync();
    }

    public async Task<T?> GetByIdAsync(string id)
    {
        var items = await LoadDataAsync();
        return items.FirstOrDefault(x => GetId(x) == id);
    }

    public async Task<T> CreateAsync(T item)
    {
        if (string.IsNullOrEmpty(GetId(item)))
        {
            SetId(item, GenerateId(item));
        }
        SetCreatedAt(item, DateTime.Now);
        SetUpdatedAt(item, DateTime.Now);
        
        var items = await LoadDataAsync();
        items.Add(item);
        await SaveDataAsync(items);
        return item;
    }

    public async Task<T> UpdateAsync(string id, T item)
    {
        var items = await LoadDataAsync();
        var index = items.FindIndex(x => GetId(x) == id);
        if (index >= 0)
        {
            SetUpdatedAt(item, DateTime.Now);
            items[index] = item;
            await SaveDataAsync(items);
        }
        return item;
    }

    public async Task DeleteAsync(string id)
    {
        var items = await LoadDataAsync();
        items.RemoveAll(x => GetId(x) == id);
        await SaveDataAsync(items);
    }

    public async Task<List<T>> SearchAsync(Func<T, bool> predicate)
    {
        var items = await LoadDataAsync();
        return items.Where(predicate).ToList();
    }

    protected async Task<List<T>> LoadDataAsync()
    {
        if (!File.Exists(_filePath))
        {
            return new List<T>();
        }

        var json = await File.ReadAllTextAsync(_filePath);
        if (string.IsNullOrWhiteSpace(json))
        {
            return new List<T>();
        }

        return JsonSerializer.Deserialize<List<T>>(json, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        }) ?? new List<T>();
    }

    protected async Task SaveDataAsync(List<T> data)
    {
        var directory = Path.GetDirectoryName(_filePath);
        if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
        {
            Directory.CreateDirectory(directory);
        }

        var json = JsonSerializer.Serialize(data, new JsonSerializerOptions
        {
            WriteIndented = true
        });
        await File.WriteAllTextAsync(_filePath, json);
    }

    /// <summary>
    /// Generate a short random ID (8 lowercase letters)
    /// </summary>
    protected string GenerateShortId()
    {
        const string chars = "abcdefghijklmnopqrstuvwxyz";
        var random = new Random();
        var result = new char[8];
        for (int i = 0; i < 8; i++)
        {
            result[i] = chars[random.Next(chars.Length)];
        }
        return new string(result);
    }

    // Virtual methods with default implementations
    protected virtual string GetId(T item) => string.Empty;
    protected virtual void SetId(T item, string id) { }
    protected virtual string GenerateId(T item) => Guid.NewGuid().ToString();
    protected virtual DateTime? GetCreatedAt(T item) => null;
    protected virtual void SetCreatedAt(T item, DateTime date) { }
    protected virtual DateTime? GetUpdatedAt(T item) => null;
    protected virtual void SetUpdatedAt(T item, DateTime date) { }
}
