using System.Diagnostics;
namespace CC.CodeGenerator;

public abstract class GeneratorBase : ISourceGenerator
{
    public virtual void Initialize(GeneratorInitializationContext context) =>
        context.RegisterForSyntaxNotifications(GetSyntaxReceiver);

    protected abstract ISyntaxReceiver GetSyntaxReceiver();

    public virtual void Execute(GeneratorExecutionContext context)
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

    protected virtual void Run(GeneratorExecutionContext context)
    {
        //创建特性
        CreateAttributeCode(context, out var compilation, out var attributeSymbol);
        var contextData = new ContextData(context, compilation, attributeSymbol);

        //查找标记了特性的成员
        var members = (context.SyntaxReceiver as ReceiverBase)?.Nodes
            .Select(node => node.SetContext(contextData))
            .Where(node => node.IsOk())
            .ToArray() ?? Array.Empty<NodeBase>();

        //将成员合并到相同的类型中
        MergeType(members);

        //构建代码
        BuildCode();
    }   

    /// <summary>
    /// 创建特性
    /// </summary>
    protected abstract void CreateAttributeCode(GeneratorExecutionContext context
        , out Compilation compilation
        , out INamedTypeSymbol attributeSymbol);

    protected abstract void BuildCode();

    /// <summary>
    /// 将成员合并到相同的类型中
    /// </summary>
    protected abstract void MergeType(IEnumerable<NodeBase> nodes);

    protected void CreateErrorFile(GeneratorExecutionContext context, Exception ex)
    {
        var error = $"/*\r\n{ex}\r\n*/";
        var fileName = $"Error_{GetType().Name}.cs";
        context.AddSource(fileName, error);
    }

    protected void DebuggerLaunch()
    {
        if (!Debugger.IsAttached) Debugger.Launch();
    }
}