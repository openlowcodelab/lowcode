using JetBrains.Annotations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace H.LowCode.EntityFrameworkCore;

/// <summary>
/// Emit扩展
/// </summary>
internal static class EmitExtension
{
    #region Assembly, Module
    /// <summary>
    /// 创建一个可运行的动态程序集
    /// </summary>
    /// <param name="assemblyName">程序集名称</param>
    /// <param name="options"></param>
    /// <returns>动态程序集</returns>
    /// <exception cref="ArgumentNullException">assemblyName为空</exception>
    [NotNull]
    public static AssemblyBuilder CreateDynamicAssembly([NotNull] string assemblyName,
        [NotNull] AssemblyCreateOptions options)
    {
        if (string.IsNullOrEmpty(assemblyName))
            throw new ArgumentNullException(nameof(assemblyName));
        if (options == null)
            throw new ArgumentNullException(nameof(options));

        var assemblyNameObj = new AssemblyName(assemblyName);

        if (options.StrongName)
        {
            assemblyNameObj.SetPublicKey(options.PublicKey);
        }

        var assemblyBuilder = AssemblyBuilder.DefineDynamicAssembly(assemblyNameObj, AssemblyBuilderAccess.Run);

        // 设置程序集的内部可见
        if (options.InternalsVisibleTo != null && options.InternalsVisibleTo.Length > 0)
        {
            var constructorInfo = typeof(InternalsVisibleToAttribute).GetConstructor(new[] { typeof(string) });

            // InternalsVisibleToAttribute类型一定会有一个构造函数接收1个字符串
            if (constructorInfo != null)
            {
                var customAttributeBuilder = new CustomAttributeBuilder(constructorInfo,
                    options.InternalsVisibleTo.OfType<object>().ToArray());
                assemblyBuilder.SetCustomAttribute(customAttributeBuilder);
            }
        }

        return assemblyBuilder;
    }

    /// <summary>
    /// 为动态程序集创建一个模块
    /// </summary>
    /// <param name="assemblyBuilder">动态程序集</param>
    /// <param name="moduleName">模块名</param>
    /// <returns>动态程序集中的模块</returns>
    /// <exception cref="ArgumentNullException">assemblyBuilder或moduleName为空</exception>
    [NotNull]
    public static ModuleBuilder CreateModule([NotNull] this AssemblyBuilder assemblyBuilder,
        [NotNull] string moduleName)
    {
        if (assemblyBuilder == null)
            throw new ArgumentNullException(nameof(assemblyBuilder));

        if (string.IsNullOrEmpty(moduleName))
            throw new ArgumentNullException(nameof(moduleName));

        var moduleBuilder = assemblyBuilder.DefineDynamicModule(moduleName);
        return moduleBuilder;
    }
    #endregion

    #region Type
    /// <summary>
    /// 为动态程序集创建一个类型，继承指定基类，实现一些接口
    /// </summary>
    /// <param name="moduleBuilder"></param>
    /// <param name="typeName"></param>
    /// <param name="baseType"></param>
    /// <param name="interfaceTypes"></param>
    /// <returns></returns>
    /// <exception cref="ArgumentNullException">moduleBuilder或typeName或baseType为空时</exception>
    [NotNull]
    public static TypeBuilder DefineType([NotNull] this ModuleBuilder moduleBuilder,
        [NotNull] string typeName,
        [NotNull] Type baseType,
        [CanBeNull][ItemNotNull] IEnumerable<Type> interfaceTypes = null)
    {
        if (moduleBuilder == null)
            throw new ArgumentNullException(nameof(moduleBuilder));
        if (string.IsNullOrEmpty(typeName))
            throw new ArgumentNullException(nameof(typeName));
        if (baseType == null)
            throw new ArgumentNullException(nameof(baseType));

        var typeAttributes = TypeAttributes.Public | TypeAttributes.Class;
        var typeBuilder = moduleBuilder.DefineType(typeName, typeAttributes, baseType);

        // 实现接口
        if (interfaceTypes != null)
        {
            foreach (var interfaceType in interfaceTypes)
            {
                typeBuilder.AddInterfaceImplementation(interfaceType);
            }
        }

        return typeBuilder;
    }
    #endregion

    #region Properties
    /// <summary>
    /// 为类型创建一个自动属性
    /// </summary>
    /// <param name="typeBuilder"></param>
    /// <param name="propertyName"></param>
    /// <param name="propertyType"></param>
    /// <returns></returns>
    /// <exception cref="ArgumentNullException">typeBuilder或propertyName或propertyType为空</exception>
    [NotNull]
    public static PropertyBuilder DefineField([NotNull] this TypeBuilder typeBuilder,
        [NotNull] string propertyName,
        [NotNull] Type propertyType)
    {
        if (typeBuilder == null)
            throw new ArgumentNullException(nameof(typeBuilder));
        if (string.IsNullOrEmpty(propertyName))
            throw new ArgumentNullException(nameof(propertyName));
        if (propertyType == null)
            throw new ArgumentNullException(nameof(propertyType));

        // c#
        // private type _field;
        var fieldBuilder = typeBuilder.DefineField(GetBackgroundFieldName(propertyName), propertyType,
            FieldAttributes.Private);
        // [CompilerGenerated]
        fieldBuilder.AddCustomAttribute(typeof(CompilerGeneratedAttribute));

        // c#
        // public type get_field() {}
        var getMethodBuilder = typeBuilder.DefineMethod("get_" + propertyName,
            MethodAttributes.Public | MethodAttributes.HideBySig | MethodAttributes.SpecialName, propertyType,
            Type.EmptyTypes);
        var getIL = getMethodBuilder.GetILGenerator();

        // c#
        // return _field;
        getIL.Emit(OpCodes.Ldarg_0);
        getIL.Emit(OpCodes.Ldfld, fieldBuilder);
        getIL.Emit(OpCodes.Ret);

        // c#
        // public void set_field(type value) {}
        var setMethodBuilder = typeBuilder.DefineMethod("set_" + propertyName,
            MethodAttributes.Public | MethodAttributes.HideBySig | MethodAttributes.SpecialName, null,
            new[] { propertyType });
        var setIL = setMethodBuilder.GetILGenerator();

        // c#
        // _field = value;
        setIL.Emit(OpCodes.Ldarg_0);
        setIL.Emit(OpCodes.Ldarg_1);
        setIL.Emit(OpCodes.Stfld, fieldBuilder);
        setIL.Emit(OpCodes.Ret);

        // define property
        // property = get + set
        var propertyBuilder = typeBuilder.DefineProperty(propertyName, PropertyAttributes.None, propertyType, null);

        propertyBuilder.SetGetMethod(getMethodBuilder);
        propertyBuilder.SetSetMethod(setMethodBuilder);

        return propertyBuilder;
    }

    /// <summary>
    /// 产生自动属性的后台字段
    /// </summary>
    private static string GetBackgroundFieldName(string propertyName)
    {
        return $"<{propertyName}>k__BackingField";
    }
    #endregion

    #region Attributes
    // /// <summary>
    // /// 向动态类型增加一个Attribute标记（只支持带有无参数构造函数的Attribute类型）
    // /// </summary>
    // /// <param name="assemblyBuilder">动态类型</param>
    // /// <param name="attributeType">Attribute类型</param>
    // /// <returns></returns>
    // /// <exception cref="InvalidOperationException">typeBuilder或attributeType为空</exception>
    // public static AssemblyBuilder AddCustomAttribute([NotNull] this AssemblyBuilder assemblyBuilder,
    //     [NotNull] Type attributeType)
    // {
    //     if (assemblyBuilder == null)
    //         throw new ArgumentNullException(nameof(assemblyBuilder));
    //
    //     assemblyBuilder.SetCustomAttribute(CreateCustomAttribute(attributeType));
    //
    //     return assemblyBuilder;
    // }

    /// <summary>
    /// 向动态类型增加一个Attribute标记（只支持带有无参数构造函数的Attribute类型）
    /// </summary>
    /// <param name="typeBuilder">动态类型</param>
    /// <param name="attributeType">Attribute类型</param>
    /// <returns></returns>
    /// <exception cref="InvalidOperationException">typeBuilder或attributeType为空</exception>
    public static TypeBuilder AddCustomAttribute([NotNull] this TypeBuilder typeBuilder,
        [NotNull] Type attributeType)
    {
        if (typeBuilder == null)
            throw new ArgumentNullException(nameof(typeBuilder));

        typeBuilder.SetCustomAttribute(CreateCustomAttribute(attributeType));

        return typeBuilder;
    }

    /// <summary>
    /// 向属性增加一个Attribute标记（只支持带有无参数构造函数的Attribute类型）
    /// </summary>
    /// <param name="propertyBuilder">动态属性</param>
    /// <param name="attributeType">Attribute类型</param>
    /// <returns></returns>
    /// <exception cref="InvalidOperationException">propertyBuilder或attributeType为空</exception>
    public static PropertyBuilder AddCustomAttribute([NotNull] this PropertyBuilder propertyBuilder,
        [NotNull] Type attributeType)
    {
        if (propertyBuilder == null)
            throw new ArgumentNullException(nameof(propertyBuilder));

        propertyBuilder.SetCustomAttribute(CreateCustomAttribute(attributeType));

        return propertyBuilder;
    }

    /// <summary>
    /// 向字段增加一个Attribute标记（只支持带有无参数构造函数的Attribute类型）
    /// </summary>
    /// <param name="fieldBuilder">动态字段</param>
    /// <param name="attributeType">Attribute类型</param>
    /// <returns></returns>
    /// <exception cref="InvalidOperationException">fieldBuilder或attributeType为空</exception>
    public static FieldBuilder AddCustomAttribute([NotNull] this FieldBuilder fieldBuilder,
        [NotNull] Type attributeType)
    {
        if (fieldBuilder == null)
            throw new ArgumentNullException(nameof(fieldBuilder));

        fieldBuilder.SetCustomAttribute(CreateCustomAttribute(attributeType));

        return fieldBuilder;
    }

    private static CustomAttributeBuilder CreateCustomAttribute([NotNull] Type attributeType)
    {
        if (attributeType == null)
            throw new ArgumentNullException(nameof(attributeType));

        var constructorInfo = attributeType.GetConstructor(Type.EmptyTypes);
        if (constructorInfo == null)
            throw new InvalidOperationException($"类型[{attributeType.Name}]没有无参数的构造函数");

        return new CustomAttributeBuilder(constructorInfo, new object[0]);
    }
    #endregion

    #region Delegates

    /// <summary>
    /// 获取类型的无参数构造函数委托
    /// </summary>
    public static ObjectActivatorDelegate GetActivatorProxyFunc([NotNull] this Type type,
        [CanBeNull] DelegateMethodCreateOptions options = null)
    {
        if (type == null)
            throw new ArgumentNullException(nameof(type));

        var constructorInfo = type.GetConstructor(Type.EmptyTypes);
        if (constructorInfo == null)
            throw new InvalidOperationException($"类型[{type.Name}]没有无参数的构造函数");

        var method =
            options?.BelongModule == null
                ? new DynamicMethod("Create" + type.Name, type, Type.EmptyTypes)
                : new DynamicMethod("Create" + type.Name, type, Type.EmptyTypes, options.BelongModule);

        var ilGenerator = method.GetILGenerator();

        ilGenerator.Emit(OpCodes.Newobj, constructorInfo);
        ilGenerator.Emit(OpCodes.Ret);

        return (ObjectActivatorDelegate)method.CreateDelegate(typeof(ObjectActivatorDelegate));
    }

    #endregion
}

/// <summary>
/// 程序集创建参数
/// </summary>
internal class AssemblyCreateOptions
{
    /// <summary>
    /// 是否需要程序集强名称
    /// </summary>
    public bool StrongName { get; set; }

    /// <summary>
    /// 程序集强名称的PublicKey
    /// </summary>
    public byte[] PublicKey { get; set; }

    /// <summary>
    /// 程序集内部可见的设置
    /// </summary>
    public string[] InternalsVisibleTo { get; set; }
}

/// <summary>
/// 代理方法创建参数
/// </summary>
internal class DelegateMethodCreateOptions
{
    /// <summary>
    /// 方法归属的模块
    /// 如果方法访问了私有的类型或方法，归属模块尤其重要。
    /// 不设置或归属模块不正确，会导致无法访问私有类型或成员的问题。
    /// </summary>
    public Module BelongModule { get; set; }
}

internal delegate object ObjectActivatorDelegate();
internal delegate object GetValueDelegate(object obj);
internal delegate void SetValueDelegate(object obj, object value);