#pragma warning disable CS8632 
using System;
namespace CC.CodeGenerator;

/// <summary>
/// 创建具有变更通知的属性
/// </summary>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Field, AllowMultiple = true)]
public class AddNotifyPropertyChangedAttribute : Attribute
{
    /// <summary>
    /// <inheritdoc cref=""AddNotifyPropertyChangedAttribute""/>
    /// </summary>
    public AddNotifyPropertyChangedAttribute() { }

    /// <summary>
    /// <inheritdoc cref=""AddNotifyPropertyChangedAttribute""/>
    /// </summary>
    /// <param name=""propertyName"">属性名称</param>
    public AddNotifyPropertyChangedAttribute(string propertyName) =>
        PropertyName = propertyName;

    /// <summary>
    /// <inheritdoc cref=""AddNotifyPropertyChangedAttribute""/>
    /// </summary>
    /// <param name=""propertyName"">属性名称</param>
    /// <param name=""propertyType"">属性类型</param>
    public AddNotifyPropertyChangedAttribute(string propertyName, Type propertyType)
    {
        PropertyName = propertyName;
        PropertyType = propertyType;
    }

    /// <summary>
    /// 属性名称
    /// </summary>
    public string? PropertyName { get; }


    /// <summary>
    /// 属性类型
    /// </summary>
    public Type? PropertyType { get; }


    /// <summary>
    /// SetProperty方法的名称。
    /// <br/> 用于解决命名冲突。(仅用于类型)
    /// </summary>
    public string? SetPropertyMethodName { get; set; }

    /// <summary>
    /// OnPropertyChanged方法的名称。
    /// <br/> 用于解决命名冲突。(仅用于类型)
    /// </summary>
    public string? OnPropertyChangedMethodName { get; set; }

    /// <summary>
    /// 生成xml文档的字符串。
    /// </summary>
    public string? XmlSummary { get; set; }
}