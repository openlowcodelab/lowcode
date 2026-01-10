using System;
using System.Reflection;

namespace System;

public static class TypeExtension
{
    public static Type ResolveType(this string typeString)
    {
        if (string.IsNullOrEmpty(typeString))
        {
            return null;
        }

        Type type;
        
        // 直接尝试使用完整类型名获取类型
        type = Type.GetType(typeString, false);
        if (type != null)
        {
            Console.WriteLine($"Direct resolve successful: {type.FullName}");
            return type;
        }

        // 处理 AntDesign 组件的特殊情况
        string typeName = typeString.Split(',')[0].Trim();
        string assemblyName = typeString.Split(',')[1].Trim();
        
        // 记录当前正在解析的类型
        Console.WriteLine($"Resolving type: {typeString}");
        
        // 直接返回已知的 AntDesign 组件类型映射
        if (typeName == "AntDesign.AntList`1[System.Object]")
        {
            // 这个类型已经能直接解析，不需要特殊处理
            Type antListType = Type.GetType(typeString, false);
            if (antListType != null)
            {
                Console.WriteLine($"Direct resolve for AntList: {antListType.FullName}");
                return antListType;
            }
        }
        
        // 处理 AntDesign.List 类型（用户可能使用的旧名称）
        if (typeName == "AntDesign.List" || typeName == "AntDesign.List`1[System.Object]")
        {
            // 先尝试直接解析 AntDesign.List
            Type listType = Type.GetType($"{typeName}, {assemblyName}", false);
            if (listType != null)
            {
                Console.WriteLine($"Direct resolve for List: {listType.FullName}");
                return listType;
            }
            
            // 再尝试解析 AntDesign.AntList（这是 AntDesign 库中的实际类型名）
            string antListTypeName = typeName.Replace("AntDesign.List", "AntDesign.AntList");
            Type antListType = Type.GetType($"{antListTypeName}, {assemblyName}", false);
            if (antListType != null)
            {
                Console.WriteLine($"Resolved List to AntList: {antListType.FullName}");
                return antListType;
            }
        }
        
        // 针对 AntDesign.Input 的特殊处理
        if (typeName == "AntDesign.Input")
        {
            // 尝试使用 AntDesign.Input 类型（非泛型）
            Type inputType = Type.GetType("AntDesign.Input, AntDesign", false);
            if (inputType != null && !inputType.IsGenericTypeDefinition)
            {
                Console.WriteLine($"Found AntDesign.Input directly: {inputType.FullName}");
                return inputType;
            }
            
            // 尝试使用 AntDesign.AntInput 类型（非泛型）
            Type antInputType = Type.GetType("AntDesign.AntInput, AntDesign", false);
            if (antInputType != null && !antInputType.IsGenericTypeDefinition)
            {
                Console.WriteLine($"AntDesign.Input resolved to AntDesign.AntInput");
                return antInputType;
            }
            
            // 尝试获取所有已加载的程序集中的 AntDesign 相关类型
            var inputAssemblies = AppDomain.CurrentDomain.GetAssemblies();
            foreach (var assembly in inputAssemblies)
            {
                if (assembly.FullName?.Contains("AntDesign") == true)
                {
                    try
                    {
                        // 查找名称为 "Input" 或 "AntInput" 的类型
                        foreach (var assemblyType in assembly.GetTypes())
                        {
                            if ((assemblyType.Name == "Input" || assemblyType.Name == "AntInput") && 
                                !assemblyType.IsGenericTypeDefinition && 
                                assemblyType.Namespace == "AntDesign")
                            {
                                Console.WriteLine($"Found AntDesign.Input in assembly: {assemblyType.FullName}");
                                return assemblyType;
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Error scanning assembly {assembly.FullName}: {ex.Message}");
                    }
                }
            }
        }
        
        // 针对 AntDesign.TextArea 的特殊处理
        if (typeName == "AntDesign.TextArea")
        {
            // 直接返回 AntDesign.TextArea 类型
            Type textAreaType = Type.GetType("AntDesign.TextArea, AntDesign", false);
            if (textAreaType != null && !textAreaType.IsGenericTypeDefinition)
            {
                Console.WriteLine($"Found AntDesign.TextArea directly: {textAreaType.FullName}");
                return textAreaType;
            }
            
            // 尝试获取所有已加载的程序集中的 TextArea 类型
            var textAreaAssemblies = AppDomain.CurrentDomain.GetAssemblies();
            foreach (var assembly in textAreaAssemblies)
            {
                if (assembly.FullName?.Contains("AntDesign") == true)
                {
                    try
                    {
                        // 查找名称为 "TextArea" 的类型
                        foreach (var assemblyType in assembly.GetTypes())
                        {
                            if (assemblyType.Name == "TextArea" && 
                                !assemblyType.IsGenericTypeDefinition && 
                                assemblyType.Namespace == "AntDesign")
                            {
                                Console.WriteLine($"Found AntDesign.TextArea in assembly: {assemblyType.FullName}");
                                return assemblyType;
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Error scanning assembly {assembly.FullName}: {ex.Message}");
                    }
                }
            }
        }
        
        // 处理泛型类型，确保返回完全实例化的类型
        if (typeName.Contains('`') && !typeName.Contains('['))
        {
            // 这是一个未实例化的泛型类型，尝试使用 string 作为泛型参数
            Type genericType = Type.GetType($"{typeName}, {assemblyName}", false);
            if (genericType != null && genericType.IsGenericTypeDefinition)
            {
                try
                {
                    Type resultType = genericType.MakeGenericType(typeof(string));
                    Console.WriteLine($"Resolved generic type {typeName} to {resultType.FullName}");
                    return resultType;
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Failed to instantiate generic type {typeName}: {ex.Message}");
                }
            }
        }
        
        // 尝试获取所有已加载的程序集，然后在其中查找类型
        var assemblies = AppDomain.CurrentDomain.GetAssemblies();
        foreach (var assembly in assemblies)
        {
            try
            {
                // 只在 AntDesign 相关的程序集中查找
                if (assembly.FullName?.Contains("AntDesign") == true)
                {
                    // 尝试直接查找类型
                    var foundType = assembly.GetType(typeName, false);
                    if (foundType != null)
                    {
                        Console.WriteLine($"Found type in assembly {assembly.FullName}: {foundType.FullName}");
                        return foundType;
                    }
                    
                    // 尝试查找所有类型，进行名称匹配
                    foreach (var assemblyType in assembly.GetTypes())
                    {
                        // 检查类型名是否匹配（忽略命名空间）
                        string simpleTypeName = typeName.Split('.').Last();
                        if (assemblyType.Name.Equals(simpleTypeName, StringComparison.OrdinalIgnoreCase))
                        {
                            Console.WriteLine($"Found matching type in assembly {assembly.FullName}: {assemblyType.FullName}");
                            return assemblyType;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error scanning assembly {assembly.FullName}: {ex.Message}");
            }
        }
        
        // 如果仍然无法解析，尝试一些常见的 AntDesign 组件名称变体
        if (typeName == "AntDesign.Input")
        {
            // 尝试所有可能的 Input 类型名称
            string[] possibleInputTypes = {
                "AntDesign.Input",
                "AntDesign.AntInput"
            };
            
            foreach (var possibleType in possibleInputTypes)
            {
                Type resolvedType = Type.GetType($"{possibleType}, AntDesign", false);
                if (resolvedType != null && !resolvedType.IsGenericTypeDefinition)
                {
                    Console.WriteLine($"AntDesign.Input resolved to {resolvedType.FullName}");
                    return resolvedType;
                }
            }
        }
        
        Console.WriteLine($"Type not found after all attempts: {typeString}");
        return null;
    }

    public static object GetDefaultValue(this Type type)
    {
        if (type == null)
        {
            throw new ArgumentNullException(nameof(type));
        }

        if (type.IsValueType)
        {
            return Activator.CreateInstance(type);
        }

        return null;
    }
}