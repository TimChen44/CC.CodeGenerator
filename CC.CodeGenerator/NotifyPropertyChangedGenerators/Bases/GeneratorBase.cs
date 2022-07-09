using CC.CodeGenerator.NotifyPropertyChangedGenerators;
using Microsoft.CodeAnalysis;

namespace CC.CodeGenerator.NotifyPropertyChangedGenerators;

public abstract class GeneratorBase : ISourceGenerator
{
    public virtual void Initialize(GeneratorInitializationContext context)
    {
        context.RegisterForPostInitialization(RegCallback);
        context.RegisterForSyntaxNotifications(GetSyntaxReceiver);
    }

    protected virtual void RegCallback(GeneratorPostInitializationContext context) { }


    /// <summary>
    /// 返回特性的FullName
    /// </summary>
    protected abstract string AttributeFullName { get; }

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
        var attributeSymbol = context.Compilation.GetTypeByMetadataName(AttributeFullName);
        var contextData = new ContextData(context, attributeSymbol)
        {
            Compilation = GetCompilation(context)
        };

        //查找标记了特性的成员
        var members = (context.SyntaxReceiver as ReceiverBase)?.Nodes
            .Select(node => node.SetContext(contextData))
            .Where(node => node.IsTarget())
            .ToArray() ?? Array.Empty<NodeBase>();

        //将成员合并到相同的类型中
        MergeType(members);

        //构建代码
        BuildCode();
    }

    protected virtual Compilation GetCompilation(GeneratorExecutionContext context) => context.Compilation;

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
        //if (!Debugger.IsAttached) Debugger.Launch();
        //Debug.Print("\r\n\r\n\r\n\r\n\r\n\r\n\r\n\r\n\r\n");
    }

}