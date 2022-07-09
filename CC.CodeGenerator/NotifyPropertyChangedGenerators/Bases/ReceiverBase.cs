namespace CC.CodeGenerator.NotifyPropertyChangedGenerators;
public abstract class ReceiverBase : ISyntaxReceiver
{

    private readonly List<NodeBase> nodes = new();

    public abstract void OnVisitSyntaxNode(SyntaxNode syntaxNode);

    protected bool TestAttributeCount(MemberDeclarationSyntax member) =>
        member.AttributeLists.Count > 0;

    public IEnumerable<NodeBase> Nodes => nodes;

    protected virtual void AddNode(NodeBase node) => nodes.Add(node);
}
