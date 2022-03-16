#pragma warning disable CS8632
namespace CC.CodeGenerato;

public abstract class ReceiverBase<T> : ISyntaxReceiver where T : SyntaxNode
{
    protected readonly List<T> nodes = new();

    /// <summary>
    /// 筛选出来的节点
    /// </summary>
    public IEnumerable<T> Nodes => nodes;

    public abstract void OnVisitSyntaxNode(SyntaxNode syntaxNode);

    protected bool TestAttributeCount(MemberDeclarationSyntax member) =>
        member.AttributeLists.Count > 0;
}
