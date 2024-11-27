using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace H.LowCode.EntityFrameworkCore;

internal class FieldTypeMapping
{
    public static Type GetFieldType(string fieldType, bool isNullable)
    {
        string type = $"{fieldType}{(isNullable ? "?" : string.Empty)}";
        switch (fieldType)
        {
            case "char":
            case "varchar":
                return typeof(string);
            case "bit":
                return typeof(bool);
            case "bit?":
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
            default:
                return typeof(string);
        }
    }
}