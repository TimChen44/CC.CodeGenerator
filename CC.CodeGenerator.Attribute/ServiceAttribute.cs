using System;

namespace CC.CodeGenerator;

/// <summary>
/// 标记类是否是Service类
/// </summary>
[AttributeUsage(AttributeTargets.Class, Inherited = true, AllowMultiple = false)]
public class ServiceAttribute : Attribute
{
    public ELifeCycle LifeCycle { get; set; } = ELifeCycle.Scoped;
}

/// <summary>
/// DI生命周期
/// </summary>
public enum ELifeCycle
{
    Singleton = 0,
    Scoped = 1,
    Transient = 2,
}

/// <summary>
/// 标记类是否自动注入
/// </summary>
[AttributeUsage(AttributeTargets.Property, Inherited = true, AllowMultiple = false)]
public class AutoInjectAttribute : Attribute
{
    /// <summary>
    /// 类型转换：将注入的类型转换成另一个类型
    /// </summary>
    public Type TypeConversion { get; set; } 
}