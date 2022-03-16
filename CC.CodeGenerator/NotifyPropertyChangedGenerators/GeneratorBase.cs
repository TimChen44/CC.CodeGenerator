#pragma warning disable CS8632
using System.Diagnostics;
namespace CC.CodeGenerato;
public abstract class GeneratorBase : ISourceGenerator
{
    public virtual void Initialize(GeneratorInitializationContext context) =>
        context.RegisterForSyntaxNotifications(GetSyntaxReceiver);

    /// <summary>
    /// 返回SyntaxNode
    /// </summary>
    protected abstract ISyntaxReceiver GetSyntaxReceiver();

    protected void DebuggerLaunch()
    {
        if (!Debugger.IsAttached) Debugger.Launch();
    }

    public void Execute(GeneratorExecutionContext context)
    {        
        try
        {
            Run(context);
        }
        catch (Exception ex)
        {
            CreateErrorFile(context, ex);
        }
    }

    /// <summary>
    /// 开始执行
    /// </summary>
    protected abstract void Run(GeneratorExecutionContext context);

    protected void CreateErrorFile(GeneratorExecutionContext context, Exception ex)
    {
        var error = $"/*\r\n{ex}\r\n*/";
        var fileName = $"Error_{GetType().Name}.cs";
        context.AddSource(fileName, error);
    }

    /// <summary>
    /// 验证是否为目标
    /// </summary>
    protected bool IsTarget<T>(T item) => item is ITargetValidation target && target.IsOk();
}

public abstract class GeneratorBase<T> : GeneratorBase
          where T : class, ISyntaxReceiver, new()
{
    protected override ISyntaxReceiver GetSyntaxReceiver() => new T();

    protected T? GetSyntaxReceiver(GeneratorExecutionContext context) => context.SyntaxReceiver as T;
}
