using System;
using System.Collections.Generic;
using System.Text;

namespace H.Account.Application.Contracts;

/// <summary>
/// 用户类型枚举
/// </summary>
public enum UserType
{
    /// <summary>
    /// 普通用户
    /// </summary>
    Normal = 0,

    /// <summary>
    /// 管理员
    /// </summary>
    Admin = 1,

    /// <summary>
    /// 超级管理员
    /// </summary>
    SuperAdmin = 2
}
