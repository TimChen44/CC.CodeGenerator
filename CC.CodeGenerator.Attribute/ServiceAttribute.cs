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