using Microsoft.EntityFrameworkCore.Diagnostics;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace H.LowCode.EntityFrameworkCore;

/// <summary>
/// SQL执行前进行拦截，加上WITH (NOLOCK)
/// </summary>
internal class QueryWithNoLockDbCommandInterceptor : DbCommandInterceptor
{
    private static readonly Regex TableAliasRegex =
   new Regex(@"(?<tableAlias>(FROM|JOIN) \[[a-zA-Z]\w*\] AS \[[a-zA-Z]\w*\](?! WITH \(NOLOCK\)))",
            RegexOptions.Multiline | RegexOptions.Compiled | RegexOptions.IgnoreCase);

    /// <summary>
    /// 执行sql前进行拦截并修改sql为支持WITH (NOLOCK)
    /// </summary>
    /// <param name="command"></param>
    /// <param name="eventData"></param>
    /// <param name="result"></param>
    /// <returns></returns>
    public override InterceptionResult<DbDataReader> ReaderExecuting(DbCommand command, CommandEventData eventData, InterceptionResult<DbDataReader> result)
    {
        command.CommandText = TableAliasRegex.Replace(
                command.CommandText,
                "${tableAlias} WITH (NOLOCK)"
                );
        return result;
    }
}