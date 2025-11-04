using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace H.LowCode.Entity;

public class FieldTypeMapping
{
    public static Type GetFieldType(string fieldType, bool isNullable)
    {
        if (string.IsNullOrWhiteSpace(fieldType))
            throw new ArgumentNullException(nameof(fieldType));

        // 规范化原始类型文本
        string raw = fieldType.Trim().ToLowerInvariant();

        // 处理数组类型（如 varchar[]）
        bool isArray = raw.EndsWith("[]");
        if (isArray)
            raw = raw[..^2];

        // 去除可空标记（如果来源包含 ?）
        raw = raw.TrimEnd('?');

        // 去除长度/精度说明（如 char(36)、varchar(255)、decimal(12,2)）
        int parenIndex = raw.IndexOf('(');
        string baseType = parenIndex >= 0 ? raw[..parenIndex].Trim() : raw;

        // 基础类型映射
        Type result = baseType switch
        {
            // 字符类型统一映射到 string
            "char" => typeof(string),
            "varchar" => typeof(string),
            "nchar" => typeof(string),
            "nvarchar" => typeof(string),
            "text" => typeof(string),
            "string" => typeof(string),

            // 布尔
            "bool" => typeof(bool),
            "bit" => typeof(bool),

            // 整数
            "int" => typeof(int),
            "integer" => typeof(int),
            "smallint" => typeof(int), // 简化映射
            "bigint" => typeof(long),
            "long" => typeof(long),
            "tinyint" => typeof(byte),

            // 小数
            "decimal" => typeof(decimal),
            "numeric" => typeof(decimal),
            "money" => typeof(decimal),

            // 日期时间
            "datetime" => typeof(DateTime),
            "timestamp" => typeof(DateTime),
            "date" => typeof(DateTime),
            "datetime2" => typeof(DateTime),

            // Guid
            "guid" => typeof(Guid),
            "uniqueidentifier" => typeof(Guid),

            // 浮点
            "float" => typeof(double),
            "double" => typeof(double),
            "real" => typeof(float),

            _ => null!
        };

        // 处理 varchar[] 等数组（仅支持字符串数组）
        if (isArray)
        {
            if (result == typeof(string))
                return typeof(string[]);
            throw new NotSupportedException($"not support array type: {fieldType}");
        }

        if (result == null)
            throw new NotSupportedException($"not support type: {fieldType}");

        // 处理可空值类型
        if (isNullable && result.IsValueType && Nullable.GetUnderlyingType(result) == null)
            return typeof(Nullable<>).MakeGenericType(result);

        return result;
    }
}