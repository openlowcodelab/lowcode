using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace H.LowCode.Entity;

public class FieldTypeMapping
{
    public static Type GetFieldType(string fieldType, bool isNullable)
    {
        string type = fieldType.ToLower();
        if (fieldType != "char" && fieldType != "varchar"
            && fieldType != "varchar[]")
            type = $"{type}{(isNullable ? "?" : string.Empty)}";

        switch (type)
        {
            case "char":
            case "varchar":
                return typeof(string);
            case "bool":
                return typeof(bool);
            case "bool?":
                return typeof(bool?);
            case "int":
                return typeof(int);
            case "int?":
                return typeof(int?);
            case "long":
                return typeof(long);
            case "long?":
                return typeof(long?);
            case "decimal":
                return typeof(decimal);
            case "decimal?":
                return typeof(decimal?);
            case "datetime":
                return typeof(DateTime);
            case "datetime?":
                return typeof(DateTime?);
            case "varchar[]":
                return typeof(string[]);
            default:
                throw new NotSupportedException($"not support type: {type}");
        }
    }
}