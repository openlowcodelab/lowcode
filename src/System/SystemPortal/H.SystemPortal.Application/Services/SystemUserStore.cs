using H.SystemPortal.Application.Contracts;
using System.Security.Cryptography;
using System.Text.Json;

namespace H.SystemPortal.Application;

/// <summary>
/// 系统用户存储模型（JSON 序列化用）
/// </summary>
public class SystemUserEntity
{
    public Guid Id { get; set; }
    public string UserName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string PhoneNumber { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public UserType UserType { get; set; }
    public List<string> RoleNames { get; set; } = new();
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; }
    public DateTime? LastLoginAt { get; set; }
    public string? Remark { get; set; }
}

/// <summary>
/// JSON 文件根结构
/// </summary>
public class SystemUsersData
{
    public List<SystemUserEntity> Users { get; set; } = new();
}

/// <summary>
/// 基于 JSON 文件的系统用户存储服务
/// 提供系统管理员账号的 CRUD 和密码验证功能
/// </summary>
public class SystemUserStore
{
    private readonly string _jsonFilePath;
    private static readonly object _fileLock = new();

    public SystemUserStore()
    {
        _jsonFilePath = FindUsersJsonFile();
    }

    private static string FindUsersJsonFile()
    {
#if DEBUG
        var path = Path.Combine(Directory.GetCurrentDirectory(), "..", "..", "..", "System", "SystemPortal", "data", "system-users.json");
#else
        var path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "data", "system-users.json");
#endif
        return path;
    }

    /// <summary>
    /// 获取所有用户
    /// </summary>
    public List<SystemUserEntity> GetAll()
    {
        var data = LoadData();
        return data.Users;
    }

    /// <summary>
    /// 根据 ID 查找用户
    /// </summary>
    public SystemUserEntity? FindById(Guid id)
    {
        return GetAll().FirstOrDefault(u => u.Id == id);
    }

    /// <summary>
    /// 根据用户名查找用户
    /// </summary>
    public SystemUserEntity? FindByUserName(string userName)
    {
        return GetAll().FirstOrDefault(u =>
            u.UserName.Equals(userName, StringComparison.OrdinalIgnoreCase));
    }

    /// <summary>
    /// 根据邮箱查找用户
    /// </summary>
    public SystemUserEntity? FindByEmail(string email)
    {
        return GetAll().FirstOrDefault(u =>
            u.Email.Equals(email, StringComparison.OrdinalIgnoreCase));
    }

    /// <summary>
    /// 根据手机号查找用户
    /// </summary>
    public SystemUserEntity? FindByPhoneNumber(string phone)
    {
        return GetAll().FirstOrDefault(u =>
            !string.IsNullOrEmpty(u.PhoneNumber) && u.PhoneNumber == phone);
    }

    /// <summary>
    /// 添加用户
    /// </summary>
    public void Add(SystemUserEntity user)
    {
        var data = LoadData();
        data.Users.Add(user);
        SaveData(data);
    }

    /// <summary>
    /// 更新用户
    /// </summary>
    public void Update(SystemUserEntity user)
    {
        var data = LoadData();
        var index = data.Users.FindIndex(u => u.Id == user.Id);
        if (index >= 0)
        {
            data.Users[index] = user;
            SaveData(data);
        }
    }

    /// <summary>
    /// 删除用户
    /// </summary>
    public void Delete(Guid id)
    {
        var data = LoadData();
        data.Users.RemoveAll(u => u.Id == id);
        SaveData(data);
    }

    /// <summary>
    /// 验证密码（支持 PLAIN: 前缀的明文密码，首次验证后自动转为哈希存储）
    /// </summary>
    public bool VerifyPassword(SystemUserEntity user, string password)
    {
        // 支持初始明文密码（格式: PLAIN:xxx），验证成功后自动转为哈希
        if (user.PasswordHash.StartsWith("PLAIN:", StringComparison.Ordinal))
        {
            var plainPassword = user.PasswordHash["PLAIN:".Length..];
            if (password == plainPassword)
            {
                // 首次验证成功，自动将明文转为哈希存储
                user.PasswordHash = HashPassword(password);
                Update(user);
                return true;
            }
            return false;
        }

        return VerifyPasswordHash(password, user.PasswordHash);
    }

    /// <summary>
    /// 生成密码哈希
    /// </summary>
    public static string HashPassword(string password)
    {
        // 使用 PBKDF2 (RFC2898) 进行密码哈希
        var salt = RandomNumberGenerator.GetBytes(16);
        var hash = Rfc2898DeriveBytes.Pbkdf2(password, salt, 100000, HashAlgorithmName.SHA256, 32);

        // 格式: Base64(salt + hash)
        var combined = new byte[salt.Length + hash.Length];
        Buffer.BlockCopy(salt, 0, combined, 0, salt.Length);
        Buffer.BlockCopy(hash, 0, combined, salt.Length, hash.Length);

        return Convert.ToBase64String(combined);
    }

    /// <summary>
    /// 验证密码哈希
    /// </summary>
    private static bool VerifyPasswordHash(string password, string storedHash)
    {
        if (string.IsNullOrEmpty(storedHash))
            return false;

        try
        {
            var combined = Convert.FromBase64String(storedHash);
            if (combined.Length < 48) // 16 salt + 32 hash
                return false;

            var salt = combined.AsSpan(0, 16).ToArray();
            var hash = Rfc2898DeriveBytes.Pbkdf2(password, salt, 100000, HashAlgorithmName.SHA256, 32);

            // 时间安全比较
            return CryptographicOperations.FixedTimeEquals(
                combined.AsSpan(16),
                hash.AsSpan());
        }
        catch
        {
            return false;
        }
    }

    private SystemUsersData LoadData()
    {
        lock (_fileLock)
        {
            if (!File.Exists(_jsonFilePath))
            {
                return new SystemUsersData();
            }

            var json = File.ReadAllText(_jsonFilePath);
            return json.FromJson<SystemUsersData>() ?? new SystemUsersData();
        }
    }

    private void SaveData(SystemUsersData data)
    {
        lock (_fileLock)
        {
            var directory = Path.GetDirectoryName(_jsonFilePath);
            if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }

            var options = new JsonSerializerOptions
            {
                WriteIndented = true,
                Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping
            };
            var json = JsonSerializer.Serialize(data, options);
            File.WriteAllText(_jsonFilePath, json);
        }
    }
}
