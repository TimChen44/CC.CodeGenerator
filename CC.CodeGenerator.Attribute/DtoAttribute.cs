using System;

namespace CC.CodeGenerator;

/// <summary>
/// 标记类是否是Dto类
/// </summary>
[AttributeUsage(AttributeTargets.Class, Inherited = true, AllowMultiple = false)]
public class DtoAttribute : Attribute
{
    /// <summary>
    /// EF Core上下文名字
    /// </summary>
    public string Context { get; set; }

    /// <summary>
    /// EF 实体类型
    /// </summary>
    public Type Entity { get; set; }
}

//标记属性是否需要忽略
[AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
public class DtoIgnoreAttribute : Attribute
{

}

//标记对象的外键
[AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
public class DtoForeignKeyAttribute : Attribute
{
    public string ForeignKey { get; }
    public bool AllowNull { get; }

    public DtoForeignKeyAttribute(string foreignKey, bool allowNull = true)
    {
        ForeignKey = foreignKey;
        AllowNull = allowNull;
    }

}
