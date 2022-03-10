using Microsoft.CodeAnalysis;
using System.Diagnostics;

namespace CC.CodeGenerator;

public abstract class GeneratorBase : ISourceGenerator
{
    public virtual void Initialize(GeneratorInitializationContext context)
    {
        //注册一个语法修改通知
        context.RegisterForSyntaxNotifications(GetSyntaxReceiver);
    }

    protected void DebuggerLaunch()
    {
        if (!Debugger.IsAttached) Debugger.Launch();
    }


    protected abstract ISyntaxReceiver GetSyntaxReceiver();

    public abstract void Execute(GeneratorExecutionContext context);

    /// <summary>
    /// 创建XML文档标记
    /// </summary>
    /// <param name="content">内容</param>
    /// <param name="tabCount">对其</param>
    public static string CreateXmlDocumentation(string content, int tabCount = 0)
    {
        return @$"
{InsertTab(tabCount)}/// <summary>
{InsertTab(tabCount)}/// {content}
{InsertTab(tabCount)}/// </summary>";
    }

    /// <summary>
    ///  添加参数描述
    /// </summary>
    /// <param name="paramName">参数名称</param>
    /// <param name="content">描述内容</param>
    /// <param name="tabCount">对其</param>
    public static string CreateXmlParam(string paramName, string content, int tabCount = 0) =>
        @$"{InsertTab(tabCount)}/// <param name=""{paramName}"">{content}</param>";

    /// <summary>
    /// 插入TAB
    /// </summary>
    public static string InsertTab(int tabCount) => new('\t', tabCount);
}
