namespace CC.CodeGenerator;
public abstract class NodeBase : ITargetValidation
{
    public SyntaxNode SyntaxNode { get; set; } = null!;

    public ContextData ContextData { get; private set; } = null!;

    public virtual NodeBase SetContext(ContextData context)
    {
        ContextData = context;
        return this;
    }

    public abstract bool IsOk();
}
