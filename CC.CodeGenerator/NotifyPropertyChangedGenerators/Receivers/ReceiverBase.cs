#nullable enable
namespace CC.CodeGenerator;

public abstract class ReceiverBase<T> : ISyntaxReceiver where T : SyntaxNode
{
    protected readonly List<T> nodes = new();

    /// <summary>
    /// 筛选出来的节点
    /// </summary>
    public IEnumerable<T> Nodes => nodes;

    public virtual void OnVisitSyntaxNode(SyntaxNode syntaxNode)
    {
        if (IsTarget<ClassDeclarationSyntax>(syntaxNode) || IsTarget<RecordDeclarationSyntax>(syntaxNode))
        {
            nodes.Add((T)syntaxNode);
        }
    }

    private bool IsTarget<TTarget>(SyntaxNode syntaxNode) where TTarget : MemberDeclarationSyntax => 
        syntaxNode is TTarget t && t.AttributeLists.Count > 0;

}
