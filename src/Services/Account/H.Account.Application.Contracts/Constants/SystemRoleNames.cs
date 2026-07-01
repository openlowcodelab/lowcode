namespace H.Account.Application.Contracts;

/// <summary>
/// 系统级内置角色名称常量
/// 对应 ABP IdentityRole 表中的 Name 字段
/// </summary>
public static class SystemRoleNames
{
    /// <summary>
    /// 超级管理员 - 拥有所有企业的所有权限
    /// </summary>
    public const string SuperAdmin = "SuperAdmin";

    /// <summary>
    /// 系统管理员 - 拥有所有企业的部分权限
    /// </summary>
    public const string Admin = "Admin";

    /// <summary>
    /// 所有内置角色列表（不可删除）
    /// </summary>
    public static readonly string[] BuiltInRoles = [SuperAdmin, Admin];

    /// <summary>
    /// 判断是否为内置角色
    /// </summary>
    public static bool IsBuiltIn(string roleName)
        => BuiltInRoles.Contains(roleName, StringComparer.OrdinalIgnoreCase);
}
