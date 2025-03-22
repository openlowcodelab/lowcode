﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace System;

public static class ObjectMerger
{
    public static void Merge(Type type, object source, object target)
    {
        if (target == null || source == null)
            throw new ArgumentNullException(target == null ? nameof(target) : nameof(source));

        var properties = type.GetProperties(BindingFlags.Public | BindingFlags.Instance);

        foreach (var property in properties)
        {
            var sourcePropertyValue = property.GetValue(source);
            if (sourcePropertyValue == null)
                continue;

            //默认值跳过
            if (IsDefaultValue(property.PropertyType, sourcePropertyValue))
                continue;

            if (IsCollectionType(property.PropertyType))
            {
                // 处理集合类型属性
                MergeCollections(property, target, sourcePropertyValue);
            }
            else if (IsArrayType(property.PropertyType))
            {
                MergeArray(property, target, sourcePropertyValue);
            }
            else if (property.PropertyType.IsClass && property.PropertyType != typeof(string))
            {
                // 递归处理引用类型属性 (不含string)
                var targetPropertyValue = property.GetValue(target);
                if (targetPropertyValue == null)
                {
                    targetPropertyValue = Activator.CreateInstance(property.PropertyType);
                    property.SetValue(target, targetPropertyValue);
                }

                Merge(property.PropertyType, sourcePropertyValue, targetPropertyValue);
            }
            else
            {
                // 直接覆盖简单类型 (值类型及string) 属性
                property.SetValue(target, sourcePropertyValue);
            }
        }
    }

    private static bool IsDefaultValue(Type type, object value)
    {
        // 对于值类型，比较其默认值
        if (type.IsValueType && Nullable.GetUnderlyingType(type) == null)
        {
            return Equals(value, Activator.CreateInstance(type));
        }

        // 对于引用类型，null 或者空字符串视为默认值
        if (value == null || (type == typeof(string) && string.IsNullOrEmpty((string)value)))
        {
            return true;
        }

        return false;
    }

    private static bool IsCollectionType(Type type)
    {
        return typeof(IEnumerable).IsAssignableFrom(type) &&
               !(type.IsArray || type == typeof(string));
    }

    private static void MergeCollections(PropertyInfo property, object target, object sourceCollection)
    {
        var targetType = property.PropertyType;
        var elementType = GetElementType(targetType);

        var targetCollection = property.GetValue(target) as IList;
        if (targetCollection == null)
        {
            targetCollection = (IList)Activator.CreateInstance(targetType);
            property.SetValue(target, targetCollection);
        }

        var sourceEnum = ((IEnumerable)sourceCollection).GetEnumerator();
        for (int i = 0; sourceEnum.MoveNext(); i++)
        {
            object sourceElement = sourceEnum.Current;
            if (i < targetCollection.Count)
            {
                // 如果存在对应的元素，则尝试递归合并
                var targetElement = targetCollection[i];
                if (elementType.IsClass && elementType != typeof(string))
                {
                    Merge(elementType, targetElement as dynamic, sourceElement);
                }
                else
                {
                    targetCollection[i] = sourceElement;
                }
            }
            else
            {
                // 如果目标集合较短，则添加新的元素
                targetCollection.Add(sourceElement);
            }
        }
    }

    private static bool IsArrayType(Type type)
    {
        return type.IsArray;
    }

    private static void MergeArray(PropertyInfo property, object target, object sourceArray)
    {
        var targetType = property.PropertyType;
        var elementType = GetElementType(targetType);

        var targetArray = property.GetValue(target) as Array;
        if (targetArray == null)
        {
            targetArray = Array.CreateInstance(elementType, ((Array)sourceArray).Length);
            property.SetValue(target, targetArray);
        }

        var sourceArrayLength = ((Array)sourceArray).Length;
        var targetArrayLength = targetArray.Length;
        var newArray = Array.CreateInstance(elementType, Math.Max(sourceArrayLength, targetArrayLength));

        Array.Copy(targetArray, newArray, targetArrayLength);

        for (int i = 0; i < sourceArrayLength; i++)
        {
            var sourceElement = ((Array)sourceArray).GetValue(i);
            if (i < targetArrayLength)
            {
                var targetElement = targetArray.GetValue(i);
                if (elementType.IsClass && elementType != typeof(string))
                {
                    Merge(elementType, targetElement, sourceElement);
                    newArray.SetValue(targetElement, i);
                }
                else
                {
                    newArray.SetValue(sourceElement, i);
                }
            }
            else
            {
                newArray.SetValue(sourceElement, i);
            }
        }

        property.SetValue(target, newArray);
    }

    private static Type GetElementType(Type collectionType)
    {
        if (collectionType.IsArray)
        {
            return collectionType.GetElementType();
        }
        else
        {
            var interfaces = collectionType.GetInterfaces();
            foreach (var iface in interfaces)
            {
                if (iface.IsGenericType && iface.GetGenericTypeDefinition() == typeof(IEnumerable<>))
                {
                    return iface.GetGenericArguments()[0];
                }
            }
        }
        return typeof(object);
    }
}
