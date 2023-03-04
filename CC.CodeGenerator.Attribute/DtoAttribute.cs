using System;

namespace CC.CodeGenerator;

/// <summary>
/// 标记类是否是Dto类
/// </summary>
[AttributeUsage(AttributeTargets.Class, Inherited = true, AllowMultiple = false)]
public class DtoAttribute : Attribute
{
    public DtoAttribute(Type dbContext, Type entity)
    {
        Context = dbContext;
        Entity = entity;
    }
    /// <summary>
    /// EF Core上下类型
    /// </summary>
    public Type Context { get; set; }

    /// <summary>
    /// EF 实体类型
    /// </summary>
    public Type Entity { get; set; }
}

/// <summary>
/// 标记属性是否需要忽略
/// </summary>
[AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
public class DtoIgnoreAttribute : Attribute
{

}

/// <summary>
/// 标记属性是否不可编辑
/// </summary>
[AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
public class DtoEditDisableAttribute : Attribute
{

}

//标记对象的外键
[AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
public class DtoForeignKeyAttribute : Attribute
{
    public string ForeignTable { get; }
    public string ForeignKey { get; }
    public bool AutoCascadeSave { get; }
    public bool AutoDeleteExcess { get; }

    public DtoForeignKeyAttribute(string foreignTable,string foreignKey, bool autoCascadeSave = true, bool autoDeleteExcess = true)
    {
        ForeignTable = foreignTable;
        ForeignKey = foreignKey;
        AutoCascadeSave = autoCascadeSave;
        AutoDeleteExcess = autoDeleteExcess;
    }
}


//标记对象的外键
[AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
public class DtoKeyAttribute : Attribute
{

    public DtoKeyAttribute()
    {

    }

}