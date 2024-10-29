using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;
using H.LowCode.Domain;

namespace H.LowCode.EntityFrameworkCore;

public class EntityProxyBuilder
{
    private readonly Type _targeType;
    private readonly TypeBuilder _typeBuilder;
    private readonly IEnumerable<PropertyInfo> _virtualProperties;
    private ILGenerator _typeInitGenerator;
    private static readonly TypeCache s_typeCache;
    private static readonly object[] s_emptyArgs = new object[0];
    private static readonly string s_memberPrefix = "__";

    static EntityProxyBuilder()
    {
        s_typeCache = new TypeCache();
    }

    private FieldBuilder _fieldBuilder;


    /// <summary>
    /// 初始化 <see cref="T:System.Object"/> 类的新实例。
    /// </summary>
    public EntityProxyBuilder(Type targeType, TypeBuilder typeBuilder, IEnumerable<PropertyInfo> virtualProperties)
    {
        this._targeType = targeType;
        this._typeBuilder = typeBuilder;
        this._virtualProperties = virtualProperties;
    }

    public Type Build()
    {
        this.DeclareType();

        this.DeclareFieldsAndStaticConstructorHeader();

        //重写所有的虚属性
        foreach (PropertyInfo propertyInfo in this._virtualProperties)
            this.DeclareProperty(propertyInfo);

        this.DeclareFieldsAndStaticConstructorFooder();

        return this._typeBuilder.CreateTypeInfo();
    }

    /// <summary>
    /// 定义类型。
    /// </summary>
    private void DeclareType()
    {
        //[Serializable] 可序列化标记。
        CustomAttributeBuilder serializableAttributeBuilder = new CustomAttributeBuilder(s_typeCache.SerializableAttributeConstructorInfo, s_emptyArgs);
        this._typeBuilder.SetCustomAttribute(serializableAttributeBuilder);
        if (this._targeType != typeof(EntityBase))
        {
            //自定义实体不能直接得到实体的名称。
            //[XmlRoot("MyEntity")] Xml序列化标记。
            //TODO
            CustomAttributeBuilder xmlRootAttributeBuilder = new CustomAttributeBuilder(s_typeCache.XmlRootAttributeConstructorInfo, new object[] { "EntityNameCache.GetEntityName(this._targeType)" });
            this._typeBuilder.SetCustomAttribute(xmlRootAttributeBuilder);
        }
    }

    /// <summary>
    /// 定义字段和构造函数。
    /// </summary>
    private void DeclareFieldsAndStaticConstructorHeader()
    {
        MethodInfo getEntityFieldInfoMethodInfo = s_typeCache.GetEntityFieldInfoMethodInfo;

        //定义 Dictionary<string, EntityFieldInfo> 字段，用于缓存实体本身定义的字段。
        this._fieldBuilder = this._typeBuilder.DefineField(s_memberPrefix + "dictionary", getEntityFieldInfoMethodInfo.ReturnType, FieldAttributes.Private | FieldAttributes.SpecialName | FieldAttributes.Static | FieldAttributes.InitOnly);

        //静态构造函数。
        ConstructorBuilder constructorBuilder = this._typeBuilder.DefineTypeInitializer();
        this._typeInitGenerator = constructorBuilder.GetILGenerator();

        this._typeInitGenerator.DeclareLocal(typeof(string)); //FieldName
        //TODO
        //this._typeInitGenerator.DeclareLocal(typeof(EntityFieldInfo));

        //初始化默静态字段 __dictionary = new Dictionary<string, EntityFieldInfo>(StringComparer.OrdinalIgnoreCase);
        PropertyInfo ordinalIgnoreCasePropertyInfo = typeof(StringComparer).GetProperty("OrdinalIgnoreCase");

        this._typeInitGenerator.Emit(OpCodes.Call, ordinalIgnoreCasePropertyInfo.GetMethod);
        ConstructorInfo constructorInfo = getEntityFieldInfoMethodInfo.ReturnType.GetConstructor(new Type[] { typeof(StringComparer) });
        this._typeInitGenerator.Emit(OpCodes.Newobj, constructorInfo);
        this._typeInitGenerator.Emit(OpCodes.Stsfld, this._fieldBuilder);

        //重写抽象方法：Dictionary<string, EntityFieldInfo> GetEntityFieldInfo()
        MethodBuilder getEntityFieldInfoMethodBuilder = this._typeBuilder.DefineMethod(getEntityFieldInfoMethodInfo.Name, MethodAttributes.Virtual | MethodAttributes.Family | MethodAttributes.HideBySig, getEntityFieldInfoMethodInfo.ReturnType, Type.EmptyTypes);
        ILGenerator getEntityFieldInfoGenerator = getEntityFieldInfoMethodBuilder.GetILGenerator();
        //直接返回静态字段
        getEntityFieldInfoGenerator.Emit(OpCodes.Ldsfld, this._fieldBuilder);
        getEntityFieldInfoGenerator.Emit(OpCodes.Ret);
    }

    private void DeclareFieldsAndStaticConstructorFooder()
    {
        this._typeInitGenerator.Emit(OpCodes.Ret);
    }

    /// <summary>
    /// 定义属性。
    /// </summary>
    private void DeclareProperty(PropertyInfo propertyInfo)
    {
        //获取属性在数据库对应的字段。
        //TODO
        //EntityColumnAttribute entityColumnAttribute = propertyInfo.GetMyAttribute<EntityColumnAttribute>();
        //string fieldName = entityColumnAttribute == null || string.IsNullOrEmpty(entityColumnAttribute.ColumnName) ? propertyInfo.Name : entityColumnAttribute.ColumnName;
        string fieldName = string.Empty;

        PropertyBuilder propertyBuilder = this._typeBuilder.DefineProperty(propertyInfo.Name, PropertyAttributes.None, propertyInfo.PropertyType, Type.EmptyTypes);

        this.BuildGetMethod(propertyInfo, propertyBuilder);

        this.BuidSetMethod(propertyInfo, fieldName, propertyBuilder);

        MethodBuilder staticGetMathodBuilder = this.BuildStaticGetMethod(propertyInfo);

        MethodBuilder staticSetMathodBuilder = this.BuildStaticSetMethod(propertyInfo, propertyBuilder);

        MethodBuilder setNoChangeMathodBuilder = BuildSetValueNoChange(propertyInfo);

        MethodBuilder staticSetNoChangeMathodBuilder = this.BuildStaticSetValueNoChange(propertyInfo, setNoChangeMathodBuilder);

        this.BuildPropertyStaticConstructor(propertyInfo, fieldName, staticSetMathodBuilder, staticSetNoChangeMathodBuilder, staticGetMathodBuilder);

    }


    private void BuildPropertyStaticConstructor(PropertyInfo propertyInfo, string fieldName, MethodBuilder staticSetMathodBuilder, MethodBuilder staticSetNoChangeMathodBuilder, MethodBuilder staticGetMathodBuilder)
    {
        #region 静态构造函数

        //生成该属性的实体信息
        _typeInitGenerator.Emit(OpCodes.Ldstr, fieldName); //这里要使用别名；

        // 这里就是入栈啊,由于代码规范,我需要在这里写这句废话。
        // 之后是字符串入栈啊。
        _typeInitGenerator.Emit(OpCodes.Stloc_0);
        _typeInitGenerator.Emit(OpCodes.Ldsfld, this._fieldBuilder);

        // 其实以下几行，就是为了完成一个new操作。
        // 然后把参数传递给类型的构造函数。
        // new EntityFieldInfo();
        _typeInitGenerator.Emit(OpCodes.Ldloc_0);
        _typeInitGenerator.Emit(OpCodes.Newobj, s_typeCache.EntityFieldInfoCconstructorInfo);
        _typeInitGenerator.Emit(OpCodes.Stloc_1);
        _typeInitGenerator.Emit(OpCodes.Ldloc_1);

        //这里就是入栈一个字符串名称啊。
        //EntityFieldInfo.FieldName
        _typeInitGenerator.Emit(OpCodes.Ldloc_0);
        _typeInitGenerator.Emit(OpCodes.Stfld, s_typeCache.FieldNameFieldInfo);


        this.BuildEnumDescriptionColumnName(propertyInfo);

        this.BuildFieldType(propertyInfo);

        this.BuildSetValueAction(staticSetMathodBuilder);

        this.BuildSetValueNoChangeAction(staticSetNoChangeMathodBuilder);

        this.BuildEntityInfoGetValueFun(staticGetMathodBuilder);

        this.BuildOtherField();

        #endregion
    }


    #region Build EntityFieldInfo

    private void BuildSetValueNoChangeAction(MethodBuilder staticSetNoChangeMathodBuilder)
    {
        //EntityFieldInfo.SetValueNoChangeAction
        _typeInitGenerator.Emit(OpCodes.Ldloc_1);
        _typeInitGenerator.Emit(OpCodes.Ldnull);
        _typeInitGenerator.Emit(OpCodes.Ldftn, staticSetNoChangeMathodBuilder);
        _typeInitGenerator.Emit(OpCodes.Newobj, s_typeCache.ActionConstructorInfo);
        _typeInitGenerator.Emit(OpCodes.Stfld, s_typeCache.SetValueNoChangeActionFieldInfo);
    }

    private void BuildSetValueAction(MethodBuilder staticSetMathodBuilder)
    {
        //EntityFieldInfo.SetValueAction
        _typeInitGenerator.Emit(OpCodes.Ldloc_1);
        _typeInitGenerator.Emit(OpCodes.Ldnull);
        _typeInitGenerator.Emit(OpCodes.Ldftn, staticSetMathodBuilder);
        _typeInitGenerator.Emit(OpCodes.Newobj, s_typeCache.ActionConstructorInfo);
        _typeInitGenerator.Emit(OpCodes.Stfld, s_typeCache.SetValueActionFieldInfo);
    }

    private void BuildFieldType(PropertyInfo propertyInfo)
    {
        //EntityFieldInfo.FieldType
        _typeInitGenerator.Emit(OpCodes.Ldloc_1);
        _typeInitGenerator.Emit(OpCodes.Ldtoken, propertyInfo.PropertyType);
        _typeInitGenerator.Emit(OpCodes.Call, s_typeCache.GetTypeFromHandleMethodInfo);
        _typeInitGenerator.Emit(OpCodes.Stfld, s_typeCache.FieldTypeFieldInfo);
    }

    private void BuildEnumDescriptionColumnName(PropertyInfo propertyInfo)
    {
        //EntityFieldInfo.EnumDescriptionColumnName
        //就是为了实现以上写法
        //TODO
        //EnumColumnAttribute enumColumnAttribute = propertyInfo.GetMyAttribute<EnumColumnAttribute>();
        //if (enumColumnAttribute != null)
        //{
        //    string descriptionColumnName = enumColumnAttribute.DescriptionColumnName;

        //    //这里就是为了得到一个等于啊
        //    //EnumDescriptionColumnName = "OfficePhone",
        //    _typeInitGenerator.Emit(OpCodes.Ldloc_1);
        //    //压入字符串
        //    _typeInitGenerator.Emit(OpCodes.Ldstr, descriptionColumnName);
        //    _typeInitGenerator.Emit(OpCodes.Stfld, s_typeCache.EnumDescriptionColumnNameFieldInfo);

        //    if (enumColumnAttribute.TargetType != null)
        //    {
        //        //这里也是为了得到一个等于，Emit就是这么蛋疼。
        //        //EnumDescriptionColumnType = typeof(UserKind),
        //        _typeInitGenerator.Emit(OpCodes.Ldloc_1);
        //        _typeInitGenerator.Emit(OpCodes.Ldtoken, enumColumnAttribute.TargetType);
        //        //调用方法
        //        _typeInitGenerator.Emit(OpCodes.Call, s_typeCache.GetTypeFromHandleMethodInfo);
        //        _typeInitGenerator.Emit(OpCodes.Stfld, s_typeCache.EnumDescriptionColumnTypeFieldInfo);

        //    }
        //}
    }

    private void BuildEntityInfoGetValueFun(MethodBuilder staticGetMathodBuilder)
    {
        //EntityFieldInfo.GetValueFunc
        _typeInitGenerator.Emit(OpCodes.Ldloc_1);
        _typeInitGenerator.Emit(OpCodes.Ldnull);
        _typeInitGenerator.Emit(OpCodes.Ldftn, staticGetMathodBuilder);
        _typeInitGenerator.Emit(OpCodes.Newobj, s_typeCache.FuncConstructorInfo);
        _typeInitGenerator.Emit(OpCodes.Stfld, s_typeCache.GetValueFuncFieldInfo);
    }

    private void BuildOtherField()
    {
        //EntityFieldInfo.JsonIgnore
        _typeInitGenerator.Emit(OpCodes.Ldloc_1);
        _typeInitGenerator.Emit(OpCodes.Ldc_I4_0);
        _typeInitGenerator.Emit(OpCodes.Stfld, s_typeCache.JsonIgnoreFieldInfo);

        //EntityFieldInfo.IsVirtual
        _typeInitGenerator.Emit(OpCodes.Ldloc_1);
        _typeInitGenerator.Emit(OpCodes.Ldc_I4_1);
        _typeInitGenerator.Emit(OpCodes.Stfld, s_typeCache.IsVirtualFieldInfo);

        _typeInitGenerator.Emit(OpCodes.Ldloc_1);
        _typeInitGenerator.Emit(OpCodes.Callvirt, s_typeCache.DictionaryAddMethodInfo);
    }

    #endregion

    #region Build PropertyInfo

    private MethodBuilder BuildStaticSetValueNoChange(PropertyInfo propertyInfo, MethodBuilder setNoChangeMathodBuilder)
    {
        #region static setValueNoChage 

        //生成设置属性值的静态方法(不会修改属性的的状态。)：
        /* Demo:
        private static void __SetUserCodeNoChange(object entity, object value)
        {
            ((MyUser)entity).__SetUserCodeNoChange(value);
        }
        */

        //定义一个方法
        MethodBuilder staticSetNoChangeMathodBuilder = this._typeBuilder.DefineMethod(s_memberPrefix + "Set" + propertyInfo.Name + "NoChange", MethodAttributes.Static | MethodAttributes.Private | MethodAttributes.SpecialName | MethodAttributes.HideBySig, null, new Type[] { typeof(object), typeof(object) });

        //定义两个参数
        staticSetNoChangeMathodBuilder.DefineParameter(1, ParameterAttributes.None, "entity");
        staticSetNoChangeMathodBuilder.DefineParameter(2, ParameterAttributes.None, "value");

        //得到il代码
        ILGenerator staticSetNoChangeMethodGenerator = staticSetNoChangeMathodBuilder.GetILGenerator();
        staticSetNoChangeMethodGenerator.Emit(OpCodes.Ldarg_0);
        staticSetNoChangeMethodGenerator.Emit(OpCodes.Castclass, this._typeBuilder);
        staticSetNoChangeMethodGenerator.Emit(OpCodes.Ldarg_1);
        //如栈后调用方法
        staticSetNoChangeMethodGenerator.Emit(OpCodes.Call, setNoChangeMathodBuilder);
        staticSetNoChangeMethodGenerator.Emit(OpCodes.Ret);

        #endregion

        return staticSetNoChangeMathodBuilder;
    }

    private MethodBuilder BuildSetValueNoChange(PropertyInfo propertyInfo)
    {
        #region setValueNoChange

        //生成设置属性值的方法(不会修改属性的的状态。)：
        /* Demo:
        private void __SetUserCodeNoChange(object value)
        {
            base.UserCode = (string)value;
        }
        */
        MethodBuilder setNoChangeMathodBuilder = this._typeBuilder.DefineMethod(s_memberPrefix + "Set" + propertyInfo.Name + "NoChange", MethodAttributes.Private | MethodAttributes.SpecialName | MethodAttributes.HideBySig, null, new Type[] { typeof(object) });
        setNoChangeMathodBuilder.DefineParameter(1, ParameterAttributes.None, "value");

        //生成方法，没什么好说的。
        ILGenerator setNoChangeMethodGenerator = setNoChangeMathodBuilder.GetILGenerator();
        setNoChangeMethodGenerator.Emit(OpCodes.Ldarg_0);
        setNoChangeMethodGenerator.Emit(OpCodes.Ldarg_1);

        //判断是否值类型
        if (propertyInfo.PropertyType.IsValueType)
        {
            //如果是值类型，需要装箱。。。
            setNoChangeMethodGenerator.Emit(OpCodes.Unbox_Any, propertyInfo.PropertyType);
        }
        else
        {
            //非值类型，做次类型转换
            setNoChangeMethodGenerator.Emit(OpCodes.Castclass, propertyInfo.PropertyType);
        }

        //调用Set方法
        setNoChangeMethodGenerator.Emit(OpCodes.Call, propertyInfo.SetMethod);

        //这是设置函数返回值。
        setNoChangeMethodGenerator.Emit(OpCodes.Ret);

        #endregion

        return setNoChangeMathodBuilder;
    }

    private MethodBuilder BuildStaticSetMethod(PropertyInfo propertyInfo, PropertyBuilder propertyBuilder)
    {
        #region SetValue

        //生成设置属性值的静态方法：
        /* Demo:
        private static void __SetUserCode(object entity, object value)
        {
            ((MyUser)entity).UserCode = (string)value;
        }
        */
        MethodBuilder staticSetMathodBuilder = this._typeBuilder.DefineMethod(s_memberPrefix + "Set" + propertyInfo.Name, MethodAttributes.Static | MethodAttributes.Private | MethodAttributes.SpecialName | MethodAttributes.HideBySig, null, new Type[] { typeof(object), typeof(object) });

        //定义两个参数,entity, value
        staticSetMathodBuilder.DefineParameter(1, ParameterAttributes.None, "entity");
        staticSetMathodBuilder.DefineParameter(2, ParameterAttributes.None, "value");

        //生成方法IL代码
        ILGenerator staticSetMethodGenerator = staticSetMathodBuilder.GetILGenerator();

        staticSetMethodGenerator.Emit(OpCodes.Ldarg_0);
        staticSetMethodGenerator.Emit(OpCodes.Castclass, this._typeBuilder);
        staticSetMethodGenerator.Emit(OpCodes.Ldarg_1);

        //值类型则做装箱，非值类型则做类型转换
        if (propertyInfo.PropertyType.IsValueType)
            staticSetMethodGenerator.Emit(OpCodes.Unbox_Any, propertyInfo.PropertyType);
        else
            staticSetMethodGenerator.Emit(OpCodes.Castclass, propertyInfo.PropertyType);
        //调用方法
        staticSetMethodGenerator.Emit(OpCodes.Call, propertyBuilder.SetMethod);
        //设置返回值
        staticSetMethodGenerator.Emit(OpCodes.Ret);

        #endregion

        return staticSetMathodBuilder;
    }

    private void BuildGetMethod(PropertyInfo propertyInfo, PropertyBuilder propertyBuilder)
    {
        #region override get method

        MethodBuilder getMathodBuilder = this._typeBuilder.DefineMethod(propertyInfo.GetMethod.Name, MethodAttributes.Virtual | MethodAttributes.Public | MethodAttributes.SpecialName | MethodAttributes.HideBySig, propertyInfo.PropertyType, Type.EmptyTypes);

        ILGenerator getMethodGenerator = getMathodBuilder.GetILGenerator();

        getMethodGenerator.Emit(OpCodes.Ldarg_0);
        getMethodGenerator.Emit(OpCodes.Call, propertyInfo.GetMethod);
        getMethodGenerator.Emit(OpCodes.Ret);

        propertyBuilder.SetGetMethod(getMathodBuilder);

        #endregion
    }

    private MethodBuilder BuildStaticGetMethod(PropertyInfo propertyInfo)
    {
        #region GetValue

        //生成获取属性值的静态方法：
        /* Demo:
        private static Guid __GetUserCode(object entity)
        {
            return ((MyUser)entity).UserCode;
        }
        */
        MethodBuilder staticGetMathodBuilder = this._typeBuilder.DefineMethod(s_memberPrefix + "Get" + propertyInfo.Name, MethodAttributes.Static | MethodAttributes.Private | MethodAttributes.SpecialName | MethodAttributes.HideBySig, typeof(object), new Type[] { typeof(object) });
        //定义参数
        staticGetMathodBuilder.DefineParameter(1, ParameterAttributes.None, "entity");
        //生成方法IL代码
        ILGenerator staticGetMethodGenerator = staticGetMathodBuilder.GetILGenerator();
        //定义类型
        staticGetMethodGenerator.DeclareLocal(this._targeType);
        staticGetMethodGenerator.Emit(OpCodes.Ldarg_0);

        //类型转换后调用方法
        staticGetMethodGenerator.Emit(OpCodes.Castclass, this._targeType);
        staticGetMethodGenerator.Emit(OpCodes.Callvirt, propertyInfo.GetMethod);

        if (propertyInfo.PropertyType.IsValueType)
            staticGetMethodGenerator.Emit(OpCodes.Box, propertyInfo.PropertyType);

        //入栈，设置返回值
        staticGetMethodGenerator.Emit(OpCodes.Stloc_0);
        staticGetMethodGenerator.Emit(OpCodes.Ldloc_0);
        staticGetMethodGenerator.Emit(OpCodes.Ret);

        #endregion

        return staticGetMathodBuilder;
    }

    private void BuidSetMethod(PropertyInfo propertyInfo, string fieldName, PropertyBuilder propertyBuilder)
    {
        #region override set method

        MethodBuilder setMathodBuilder = this._typeBuilder.DefineMethod(propertyInfo.SetMethod.Name, MethodAttributes.Virtual | MethodAttributes.Public | MethodAttributes.SpecialName | MethodAttributes.HideBySig, null, new Type[] { propertyInfo.PropertyType });

        ILGenerator setMethodGenerator = setMathodBuilder.GetILGenerator();

        setMethodGenerator.Emit(OpCodes.Ldarg_0);
        setMethodGenerator.Emit(OpCodes.Ldarg_1);
        setMethodGenerator.Emit(OpCodes.Call, propertyInfo.SetMethod);

        setMethodGenerator.Emit(OpCodes.Ldarg_0);
        setMethodGenerator.Emit(OpCodes.Ldstr, fieldName);
        //设置字段变更
        setMethodGenerator.Emit(OpCodes.Call, s_typeCache.SetFieldChangedMethodInfo);
        setMethodGenerator.Emit(OpCodes.Ret);
        propertyBuilder.SetSetMethod(setMathodBuilder);

        #endregion
    }

    #endregion

    #region 反射过程中使用的缓存类

    /// <summary>
    /// 在反射过程中需要使用的的类型。
    /// </summary>
    private class TypeCache
    {
        public FieldInfo FieldNameFieldInfo;

        public FieldInfo FieldTypeFieldInfo;

        public FieldInfo SetValueActionFieldInfo;

        public FieldInfo SetValueNoChangeActionFieldInfo;

        public FieldInfo GetValueFuncFieldInfo;

        public FieldInfo JsonIgnoreFieldInfo;

        public FieldInfo IsVirtualFieldInfo;

        //实体枚举值
        public FieldInfo EnumDescriptionColumnNameFieldInfo;

        public FieldInfo EnumDescriptionColumnTypeFieldInfo;

        public MethodInfo GetTypeFromHandleMethodInfo;

        public MethodInfo GetEntityFieldInfoMethodInfo;

        public MethodInfo SetFieldChangedMethodInfo;

        public MethodInfo DictionaryAddMethodInfo;

        public ConstructorInfo EntityFieldInfoCconstructorInfo;

        public ConstructorInfo ActionConstructorInfo;

        public ConstructorInfo FuncConstructorInfo;

        public ConstructorInfo SerializableAttributeConstructorInfo;

        public ConstructorInfo XmlRootAttributeConstructorInfo;

        public TypeCache()
        {
            //TODO
            Type entityFieldInfoType = null;//typeof(EntityFieldInfo);

            FieldNameFieldInfo = entityFieldInfoType.GetField("FieldName");

            FieldTypeFieldInfo = entityFieldInfoType.GetField("FieldType");

            SetValueActionFieldInfo = entityFieldInfoType.GetField("SetValueAction");

            SetValueNoChangeActionFieldInfo = entityFieldInfoType.GetField("SetValueNoChangeAction");

            GetValueFuncFieldInfo = entityFieldInfoType.GetField("GetValueFunc");

            JsonIgnoreFieldInfo = entityFieldInfoType.GetField("JsonIgnore");

            IsVirtualFieldInfo = entityFieldInfoType.GetField("IsVirtual");

            EnumDescriptionColumnNameFieldInfo = entityFieldInfoType.GetField("EnumDescriptionColumnName");

            EnumDescriptionColumnTypeFieldInfo = entityFieldInfoType.GetField("EnumDescriptionColumnType");

            GetTypeFromHandleMethodInfo = typeof(System.Type).GetMethod("GetTypeFromHandle", new Type[] { typeof(RuntimeTypeHandle) });

            GetEntityFieldInfoMethodInfo = typeof(EntityBase).GetMethod("GetEntityFieldInfo", BindingFlags.Instance | BindingFlags.NonPublic, null, Type.EmptyTypes, null);

            SetFieldChangedMethodInfo = typeof(EntityBase).GetMethod("SetFieldChanged", BindingFlags.NonPublic | BindingFlags.Instance, null, new Type[] { typeof(string) }, null);

            DictionaryAddMethodInfo = GetEntityFieldInfoMethodInfo.ReturnType.GetMethod("Add");

            EntityFieldInfoCconstructorInfo = entityFieldInfoType.GetConstructor(Type.EmptyTypes);

            ActionConstructorInfo = typeof(Action<object, object>).GetConstructor(new Type[] { typeof(object), typeof(IntPtr) });

            FuncConstructorInfo = typeof(Func<object, object>).GetConstructor(new Type[] { typeof(object), typeof(IntPtr) });

            SerializableAttributeConstructorInfo = typeof(SerializableAttribute).GetConstructor(Type.EmptyTypes);

            XmlRootAttributeConstructorInfo = typeof(XmlRootAttribute).GetConstructor(new Type[] { typeof(string) });

        }
    }
    #endregion
}
