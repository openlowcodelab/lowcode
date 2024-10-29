using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace H.LowCode.EntityFrameworkCore;

internal class FieldTypeMapping
{
    public static Type GetFieldType(DbType dbType, string fieldType)
    {
        if(dbType == DbType.SqlServer)
        {
            return GetSqlServerFieldType(fieldType);
        }
        else if(dbType == DbType.MySql)
        {
            return GetMySqlFieldType(fieldType);
        }
        throw new ValidationException($"not support dbType: {dbType}");
    }

    private static Type GetSqlServerFieldType(string fieldType)
    {
        return typeof(string);
    }

    private static Type GetMySqlFieldType(string fieldType)
    {
        return typeof(string);
    }
}

internal enum DbType
{
    SqlServer,
    MySql,
    PostgreSql
}