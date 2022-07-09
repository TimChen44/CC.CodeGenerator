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
[AttributeUsage(AttributeTargets.Class, Inherited = true, AllowMultiple = true)]
public class AutoInjectAttribute : Attribute
{
    public AutoInjectAttribute(Type serviceType)
    {
        ServiceType = serviceType;
    }

    public AutoInjectAttribute(Type serviceType, string rename)
    {
        ServiceType = serviceType;
        Rename = rename;
    }


    /// <summary>
    /// 服务类型
    /// </summary>
    public Type ServiceType { get; set; }
    /// <summary>
    /// 重命名
    /// </summary>
    public string Rename { get; set; }

}